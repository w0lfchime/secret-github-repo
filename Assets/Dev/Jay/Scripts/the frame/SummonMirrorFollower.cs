using UnityEngine;

/// <summary>
/// Rigidbody-driven "space module" follower constrained to XZ.
/// Pulls toward target only when outside a distance band, otherwise drifts + lightly damps.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class MirrorSpaceFollowerXZ : MonoBehaviour
{
	[Header("Target")]
	public Transform target;

	[Header("Distance Band (XZ)")]
	[Tooltip("Inside this distance: no attraction force, mostly drift/damping.")]
	public float idleRadius = 2.5f;

	[Tooltip("Outside this distance: attraction force engages.")]
	public float engageRadius = 4.0f;

	[Header("Motion")]
	[Tooltip("Acceleration applied when outside engageRadius.")]
	public float accel = 18f;

	[Tooltip("Extra accel per meter beyond engageRadius.")]
	public float accelPerMeter = 6f;

	[Tooltip("Soft cap on planar speed.")]
	public float maxPlanarSpeed = 10f;

	[Tooltip("Damping when inside idleRadius (higher = slows more).")]
	public float idleDamping = 2.5f;

	[Tooltip("Damping when between idleRadius and engageRadius.")]
	public float cruiseDamping = 1.2f;

	[Header("Float / Wander (optional)")]
	[Tooltip("Small sideways wander force for 'floaty' behavior.")]
	public float wanderForce = 2.0f;

	[Tooltip("Wander speed (noise scrolling).")]
	public float wanderFrequency = 0.4f;

	[Header("Plane Lock")]
	[Tooltip("Locks Y to the starting height of this object.")]
	public bool lockYToStartHeight = true;

	private Rigidbody rb;
	private float startY;
	private float noiseSeed;

	void Awake()
	{
		target = GameObject.Find("Player").transform;
		rb = GetComponent<Rigidbody>();
		startY = transform.position.y;
		noiseSeed = Random.value * 1000f;

		// Space feel
		rb.useGravity = false;
		rb.linearDamping = 0f;
		rb.angularDamping = 0f;

		// Keep it from tipping (optional but usually desired for a mirror)
		rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
	}

	void FixedUpdate()
	{
		if (!target) return;

		Vector3 pos = rb.position;
		Vector3 targetPos = target.position;

		// XZ-only direction to target
		Vector3 toTarget = targetPos - pos;
		toTarget.y = 0f;

		float dist = toTarget.magnitude;
		Vector3 dir = dist > 0.0001f ? toTarget / dist : Vector3.zero;

		// Current planar velocity
		Vector3 v = rb.linearVelocity;
		Vector3 vPlanar = new Vector3(v.x, 0f, v.z);

		// Decide damping level based on distance band
		float damping = (dist <= idleRadius) ? idleDamping :
						(dist >= engageRadius) ? 0f : cruiseDamping;

		// 1) Attraction only when too far
		if (dist > engageRadius)
		{
			float extra = (dist - engageRadius) * accelPerMeter;
			Vector3 a = dir * (accel + extra);

			rb.AddForce(a, ForceMode.Acceleration);
		}

		// 2) Gentle damping when not actively pulling (and some even while cruising)
		if (damping > 0f)
		{
			// Damping toward zero planar velocity
			rb.AddForce(-vPlanar * damping, ForceMode.Acceleration);
		}

		// 3) Wander: add small sideways “float” so it isn’t perfect
		if (wanderForce > 0.0001f)
		{
			// Perlin-based direction on XZ
			float t = Time.time * wanderFrequency;
			float nx = Mathf.PerlinNoise(noiseSeed, t) * 2f - 1f;
			float nz = Mathf.PerlinNoise(noiseSeed + 13.37f, t) * 2f - 1f;
			Vector3 wanderDir = new Vector3(nx, 0f, nz);
			if (wanderDir.sqrMagnitude > 0.0001f) wanderDir.Normalize();

			rb.AddForce(wanderDir * wanderForce, ForceMode.Acceleration);
		}

		// 4) Soft cap planar speed (keeps it from rocketing off)
		v = rb.linearVelocity;
		vPlanar = new Vector3(v.x, 0f, v.z);
		float spd = vPlanar.magnitude;
		if (spd > maxPlanarSpeed)
		{
			Vector3 capped = vPlanar * (maxPlanarSpeed / spd);
			rb.linearVelocity = new Vector3(capped.x, rb.linearVelocity.y, capped.z);
		}

		// 5) Hard lock Y to start height (true XZ plane motion)
		if (lockYToStartHeight)
		{
			rb.position = new Vector3(rb.position.x, startY, rb.position.z);
			rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
		}
	}

	void OnDrawGizmosSelected()
	{
		if (!target) return;
		Gizmos.color = new Color(0f, 1f, 1f, 0.35f);
		Gizmos.DrawWireSphere(target.position, idleRadius);
		Gizmos.color = new Color(1f, 0.6f, 0f, 0.35f);
		Gizmos.DrawWireSphere(target.position, engageRadius);
	}
}
