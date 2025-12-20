using System;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(Rigidbody))]
public class SplineEnemyMotor : MonoBehaviour
{
	public event Action<SplineEnemyMotor> OnDespawned;

	[Header("Spline")]
	[SerializeField] private SplineContainer spline;
	[SerializeField] private int splineIndex = 0;

	[Tooltip("How far ahead on the spline (0..1) to aim toward.")]
	[SerializeField] private float lookAheadT = 0.02f;

	[Tooltip("Search window around current T for closest spline sample.")]
	[SerializeField] private float searchWindowT = 0.05f;

	[Tooltip("How many samples to check each FixedUpdate inside the search window.")]
	[SerializeField] private int samplesPerStep = 16;

	[Header("Movement (Rigidbody Force)")]
	[SerializeField] private float targetSpeed = 4.0f;
	[SerializeField] private float maxAcceleration = 25.0f; // in m/s^2
	[SerializeField] private float lateralDamping = 6.0f;   // kills sideways drift

	[Header("Spline Rejoin")]
	[Tooltip("Within this distance, primarily move parallel to the spline.")]
	[SerializeField] private float alignDistance = 1.0f;

	[Tooltip("Past this distance, strongly steer back to the spline.")]
	[SerializeField] private float hardRejoinDistance = 3.0f;

	[SerializeField] private float rejoinStrength = 1.5f;

	[Header("Orientation")]
	[SerializeField] private bool rotateToVelocity = true;
	[SerializeField] private float turnSpeed = 12f;
	[SerializeField] private bool keepUpright = true; // lock pitch/roll

	private Rigidbody _rb;

	//more addtions: 


	[Header("Variation (per-enemy)")]
	[SerializeField] private bool randomizeOnAwake = true;
	[SerializeField] private Vector2 speedMulRange = new(0.9f, 1.1f);
	[SerializeField] private Vector2 laneOffsetRange = new(-0.6f, 0.6f);   // meters, right/left of spline
	[SerializeField] private Vector2 heightOffsetRange = new(0f, 0f);      // set >0 for floaty flyers

	[Header("Wander")]
	[SerializeField] private float wanderStrength = 1.0f;   // m/s^2
	[SerializeField] private float wanderFrequency = 0.7f;  // Hz-ish

	[Header("Separation (anti-clump)")]
	[SerializeField] private LayerMask neighborMask;
	[SerializeField] private float separationRadius = 0.8f;
	[SerializeField] private float separationStrength = 6.0f;


	//more privates: 
	private float _speedMul = 1f;
	private float _laneOffset = 0f;
	private float _heightOffset = 0f;
	private float _wanderPhase;
	private Collider[] _neighbors = new Collider[16];
	private SplinePathSet.PathRef _activePath;
	private bool _hasActivePath = false;

	private float _reselectTimer = 0f;


	//more
	[Header("Multi-Path")]
	[SerializeField] private SplinePathSet pathSet;

	[SerializeField] private float reselectInterval = 0.5f;     // how often to reconsider closest path
	[SerializeField] private float switchHysteresis = 0.25f;    // must be this much closer to switch
	[SerializeField] private int globalSearchSamples = 32;      // coarse samples across whole spline


	// our estimated progress along the spline [0..1]
	private float _t;

	private void Awake()
	{
		_rb = GetComponent<Rigidbody>();
		_rb.interpolation = RigidbodyInterpolation.Interpolate;
		_rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

		if (randomizeOnAwake)
			RandomizeMotion(UnityEngine.Random.Range(int.MinValue, int.MaxValue));
	}

	public void SetSpline(SplineContainer container)
	{
		spline = container;
		// initialize progress guess near our spawn position
		_t = FindClosestT(transform.position, _t);
	}

	public void RandomizeMotion(int seed)
	{
		var rng = new System.Random(seed);
		float R(float a, float b) => a + (float)rng.NextDouble() * (b - a);

		_speedMul = R(speedMulRange.x, speedMulRange.y);
		_laneOffset = R(laneOffsetRange.x, laneOffsetRange.y);
		_heightOffset = R(heightOffsetRange.x, heightOffsetRange.y);
		_wanderPhase = R(0f, 1000f);
	}

	public void SetPathSet(SplinePathSet set)
	{
		pathSet = set;

		if (pathSet != null && pathSet.Count > 0)
			SelectBestPath(force: true);
	}

	private void SelectBestPath(bool force)
	{
		if (pathSet == null || pathSet.Count == 0) return;

		Vector3 pos = transform.position;

		// Current distance to active path (if any)
		float currentDist = float.PositiveInfinity;
		float currentBestT = _t;

		if (_hasActivePath && _activePath.container != null)
		{
			currentBestT = FindClosestT_LocalWindow(_activePath, pos, _t); // cheap refine
			Vector3 p = EvalPos(_activePath, currentBestT);
			currentDist = (p - pos).magnitude;
		}

		// Find best path globally (coarse scan each path)
		float bestDist = float.PositiveInfinity;
		float bestT = 0f;
		int bestPathIndex = -1;

		for (int i = 0; i < pathSet.Count; i++)
		{
			var path = pathSet.paths[i];
			if (path.container == null) continue;
			if (path.container.Splines.Count == 0) continue;

			path.splineIndex = Mathf.Clamp(path.splineIndex, 0, path.container.Splines.Count - 1);

			float t = FindClosestT_Global(path, pos, globalSearchSamples);
			Vector3 p = EvalPos(path, t);
			float d = (p - pos).magnitude;

			if (d < bestDist)
			{
				bestDist = d;
				bestT = t;
				bestPathIndex = i;
			}
		}

		if (bestPathIndex < 0) return;

		// Decide switch
		bool shouldSwitch =
			force ||
			!_hasActivePath ||
			bestDist < (currentDist - switchHysteresis);

		if (shouldSwitch)
		{
			_activePath = pathSet.paths[bestPathIndex];
			_activePath.splineIndex = Mathf.Clamp(_activePath.splineIndex, 0, _activePath.container.Splines.Count - 1);

			_hasActivePath = true;
			_t = bestT;
		}
		else
		{
			// stay on current, but keep refined t
			_t = currentBestT;
		}
	}

	private float FindClosestT_Global(SplinePathSet.PathRef path, Vector3 worldPos, int samples)
	{
		// Coarse full-spline scan, good for selecting which path is closest.
		float bestT = 0f;
		float bestD2 = float.PositiveInfinity;

		bool closed = path.container.Splines[path.splineIndex].Closed;
		int s = Mathf.Max(2, samples);

		for (int i = 0; i < s; i++)
		{
			float t = (s == 1) ? 0f : (i / (s - 1f));
			if (closed) t = Wrap01(t);

			Vector3 p = path.container.EvaluatePosition(path.splineIndex, t);
			float d2 = (p - worldPos).sqrMagnitude;

			if (d2 < bestD2)
			{
				bestD2 = d2;
				bestT = t;
			}
		}

		// Quick local refine around the best sample (optional but helps)
		return FindClosestT_LocalWindow(path, worldPos, bestT);
	}

	private float FindClosestT_LocalWindow(SplinePathSet.PathRef path, Vector3 worldPos, float centerT)
	{
		// Same idea as your existing FindClosestT, but for an arbitrary path ref.
		float bestT = centerT;
		float bestD2 = float.PositiveInfinity;

		float startT = centerT - searchWindowT;
		float endT = centerT + searchWindowT;

		bool closed = path.container.Splines[path.splineIndex].Closed;

		for (int i = 0; i < samplesPerStep; i++)
		{
			float a = (samplesPerStep == 1) ? 0f : (i / (samplesPerStep - 1f));
			float t = Mathf.Lerp(startT, endT, a);
			t = closed ? Wrap01(t) : Mathf.Clamp01(t);

			Vector3 p = path.container.EvaluatePosition(path.splineIndex, t);
			float d2 = (p - worldPos).sqrMagnitude;

			if (d2 < bestD2)
			{
				bestD2 = d2;
				bestT = t;
			}
		}

		return bestT;
	}

	private Vector3 EvalPos(SplinePathSet.PathRef path, float t)
	{
		return path.container.EvaluatePosition(path.splineIndex, t);
	}

	private Vector3 EvalTangent(SplinePathSet.PathRef path, float t)
	{
		return path.container.EvaluateTangent(path.splineIndex, t);
	}

	private float AdvanceT(SplinePathSet.PathRef path, float t, float dt)
	{
		bool closed = path.container.Splines[path.splineIndex].Closed;
		float outT = t + dt;
		return closed ? Wrap01(outT) : Mathf.Clamp01(outT);
	}


	private void FixedUpdate()
	{
		if (pathSet == null || pathSet.Count == 0) return;

		// Periodically reselect closest path (and allow switching)
		_reselectTimer -= Time.fixedDeltaTime;
		if (!_hasActivePath || _reselectTimer <= 0f)
		{
			_reselectTimer = Mathf.Max(0.05f, reselectInterval);
			SelectBestPath(force: !_hasActivePath);
		}

		if (!_hasActivePath || _activePath.container == null) return;

		Vector3 pos = transform.position;

		// Refine t on the active path (cheap local search)
		_t = FindClosestT_LocalWindow(_activePath, pos, _t);

		// Compute nearest + lookahead ON ACTIVE PATH
		Vector3 nearest = EvalPos(_activePath, _t);
		float aheadT = AdvanceT(_activePath, _t, lookAheadT);
		Vector3 ahead = EvalPos(_activePath, aheadT);

		// Tangent ON ACTIVE PATH (for your lane offset logic)
		Vector3 tangent = EvalTangent(_activePath, _t);

		if (keepUpright) tangent.y = 0f;
		tangent = tangent.sqrMagnitude > 0.0001f ? tangent.normalized : transform.forward;

		Vector3 right = Vector3.Cross(Vector3.up, tangent);
		right = right.sqrMagnitude > 0.0001f ? right.normalized : transform.right;

		Vector3 offsetVec = right * _laneOffset + Vector3.up * _heightOffset;

		Vector3 nearestOffset = nearest + offsetVec;
		Vector3 aheadOffset = ahead + offsetVec;

		Vector3 toAhead = (aheadOffset - pos);
		Vector3 forwardDir = toAhead.sqrMagnitude > 0.0001f ? toAhead.normalized : transform.forward;

		// 3) rejoin steering (stronger when farther) - uses OFFSET target now
		Vector3 toSpline = (nearestOffset - pos);
		float dist = toSpline.magnitude;

		float rejoin01 = 0f;
		if (dist > alignDistance)
			rejoin01 = Mathf.InverseLerp(alignDistance, hardRejoinDistance, dist);

		Vector3 rejoinDir = dist > 0.0001f ? (toSpline / dist) : Vector3.zero;

		// Blend: mostly forward along spline, plus some pull back to it when drifting
		Vector3 desiredDir = forwardDir;
		if (rejoin01 > 0f)
			desiredDir = (forwardDir + rejoinDir * (rejoinStrength * rejoin01)).normalized;

		// Optional: keep movement on XZ plane (tower defense style)
		if (keepUpright)
		{
			desiredDir.y = 0f;
			if (desiredDir.sqrMagnitude > 0.0001f) desiredDir.Normalize();
		}

		// 4) velocity control via acceleration force (tries to reach targetSpeed)
		Vector3 vel = _rb.linearVelocity;

		// NEW: Wander (sideways noise so they don't all move identical)
		float tW = Time.time * wanderFrequency + _wanderPhase;
		float n = Mathf.PerlinNoise(tW, 0.123f) * 2f - 1f; // [-1,1]
		Vector3 wanderAccel = right * (n * wanderStrength);
		if (keepUpright) wanderAccel.y = 0f;
		_rb.AddForce(wanderAccel, ForceMode.Acceleration);

		// NEW: Separation (repel nearby enemies a bit so they clash / don't stack)
		if (separationStrength > 0f && separationRadius > 0f)
		{
			int count = Physics.OverlapSphereNonAlloc(pos, separationRadius, _neighbors, neighborMask);
			if (count > 0)
			{
				Vector3 push = Vector3.zero;
				for (int i = 0; i < count; i++)
				{
					var c = _neighbors[i];
					if (!c) continue;

					Rigidbody otherRb = c.attachedRigidbody;
					if (!otherRb || otherRb == _rb) continue;

					Vector3 away = pos - c.transform.position;
					float d = away.magnitude;
					if (d < 0.0001f) continue;

					float w = 1f - Mathf.Clamp01(d / separationRadius);
					push += (away / d) * w;
				}

				if (push.sqrMagnitude > 0.0001f)
				{
					Vector3 sepAccel = push.normalized * separationStrength;
					if (keepUpright) sepAccel.y = 0f;
					_rb.AddForce(sepAccel, ForceMode.Acceleration);
				}
			}
		}

		// Remove sideways drift relative to desiredDir (helps stay on path without “rail snapping”)
		if (vel.sqrMagnitude > 0.0001f)
		{
			Vector3 lateral = vel - Vector3.Project(vel, desiredDir);
			Vector3 lateralAccel = (-lateral) * lateralDamping;
			if (keepUpright) lateralAccel.y = 0f;
			_rb.AddForce(lateralAccel, ForceMode.Acceleration);
		}

		Vector3 desiredVel = desiredDir * (targetSpeed * _speedMul); // NEW: per-enemy speed
		Vector3 neededVelChange = desiredVel - vel;
		Vector3 neededAccel = neededVelChange / Time.fixedDeltaTime;

		if (neededAccel.magnitude > maxAcceleration)
			neededAccel = neededAccel.normalized * maxAcceleration;

		_rb.AddForce(neededAccel, ForceMode.Acceleration);

		// 5) face velocity (or desired direction) for visuals
		if (rotateToVelocity)
		{
			Vector3 faceDir = _rb.linearVelocity.sqrMagnitude > 0.05f ? _rb.linearVelocity : desiredDir;
			if (keepUpright) faceDir.y = 0f;

			if (faceDir.sqrMagnitude > 0.0001f)
			{
				Quaternion targetRot = Quaternion.LookRotation(faceDir.normalized, Vector3.up);
				transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.fixedDeltaTime);
			}
		}
	}


	private float FindClosestT(Vector3 worldPos, float centerT)
	{
		// searches [centerT - window, centerT + window] and picks closest sampled point
		float bestT = centerT;
		float bestD2 = float.PositiveInfinity;

		float startT = centerT - searchWindowT;
		float endT = centerT + searchWindowT;

		// If spline is not closed, clamp.
		bool closed = spline.Splines[splineIndex].Closed;

		for (int i = 0; i < samplesPerStep; i++)
		{
			float a = (samplesPerStep == 1) ? 0f : (i / (samplesPerStep - 1f));
			float t = Mathf.Lerp(startT, endT, a);
			t = closed ? Wrap01(t) : Mathf.Clamp01(t);

			Vector3 p = EvalPos(t);
			float d2 = (p - worldPos).sqrMagnitude;
			if (d2 < bestD2)
			{
				bestD2 = d2;
				bestT = t;
			}
		}

		return bestT;
	}

	private Vector3 EvalPos(float t)
	{
		// SplineContainer.EvaluatePosition returns float3; implicit conversion works in newer versions.
		// If your Unity complains, cast: (Vector3)spline.EvaluatePosition(...)
		return spline.EvaluatePosition(splineIndex, t);
	}

	private float AdvanceT(float t, float dt)
	{
		bool closed = spline.Splines[splineIndex].Closed;
		float outT = t + dt;
		return closed ? Wrap01(outT) : Mathf.Clamp01(outT);
	}

	private static float Wrap01(float t)
	{
		t %= 1f;
		if (t < 0f) t += 1f;
		return t;
	}

	// Call this when enemy dies / reaches goal
	public void Despawn()
	{
		OnDespawned?.Invoke(this);
		Destroy(gameObject);
	}

#if UNITY_EDITOR
	private void OnDrawGizmosSelected()
	{
		if (!spline || spline.Splines.Count == 0) return;

		Vector3 nearest = spline.EvaluatePosition(splineIndex, _t);

		Vector3 tangent = spline.EvaluateTangent(splineIndex, _t);
		if (keepUpright) tangent.y = 0f;
		tangent = tangent.sqrMagnitude > 0.0001f ? tangent.normalized : transform.forward;

		Vector3 right = Vector3.Cross(Vector3.up, tangent);
		right = right.sqrMagnitude > 0.0001f ? right.normalized : transform.right;

		Vector3 offsetVec = right * _laneOffset + Vector3.up * _heightOffset;

		Vector3 nearestOffset = nearest + offsetVec;
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(nearestOffset, 0.12f);

		float aheadT = AdvanceT(_t, lookAheadT);
		Vector3 ahead = (Vector3)spline.EvaluatePosition(splineIndex, aheadT) + offsetVec;

		Gizmos.color = Color.cyan;
		Gizmos.DrawSphere(ahead, 0.12f);
		Gizmos.DrawLine(transform.position, ahead);
	}
#endif

}
