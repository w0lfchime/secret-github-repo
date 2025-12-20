using UnityEngine;

/// <summary>
/// Put this on the Mirror root (or any object). It rotates a child "pivot" on Y only,
/// aiming at the mouse cursor raycast point, with smooth lag + overshoot.
/// </summary>
public class MirrorAimPivotMouse : MonoBehaviour
{
	[Header("References")]
	[Tooltip("Child transform that will rotate (typically holds the visible mirror mesh).")]
	public Transform pivot;

	[Tooltip("Camera used for mouse ray. Defaults to Camera.main if null.")]
	public Camera cam;

	[Header("Raycast")]
	[Tooltip("Layers to raycast against for aim point.")]
	public LayerMask hitMask = ~0;

	[Tooltip("Max ray distance.")]
	public float maxRayDistance = 500f;

	[Tooltip("If raycast misses, aim to this plane instead (world Y = planeY).")]
	public bool fallbackToPlane = true;
	public float planeY = 0f;

	[Header("Rotation (Yaw Only)")]
	[Tooltip("If true, only rotate around world up axis (Y).")]
	public bool yawOnly = true;

	[Tooltip("Extra yaw offset in degrees (e.g. if your mesh faces the wrong way).")]
	public float yawOffsetDegrees = 0f;

	[Header("Spring Feel")]
	[Tooltip("Higher = snappier. Think 'spring stiffness'.")]
	public float frequency = 6.0f;

	[Tooltip("0 = very bouncy, 1 = critically damped (no overshoot).")]
	[Range(0f, 1.5f)]
	public float dampingRatio = 0.35f;

	[Tooltip("Hard limit on turning speed (deg/sec). Set <= 0 to disable.")]
	public float maxDegPerSec = 0f;

	private float currentYaw;
	private float yawVel;

	void Awake()
	{
		if (!cam) cam = Camera.main;

		if (pivot)
		{
			// Initialize state from current pivot yaw
			Vector3 e = pivot.rotation.eulerAngles;
			currentYaw = e.y;
		}
	}

	void LateUpdate()
	{
		if (!pivot) return;
		if (!cam) cam = Camera.main;
		if (!cam) return;

		Vector3 aimPoint;
		if (!TryGetAimPoint(out aimPoint))
			return;

		Vector3 pivotPos = pivot.position;
		Vector3 to = aimPoint - pivotPos;

		if (yawOnly)
			to.y = 0f;

		if (to.sqrMagnitude < 0.0001f)
			return;

		float targetYaw = Quaternion.LookRotation(to.normalized, Vector3.up).eulerAngles.y + yawOffsetDegrees;

		// Spring-damper on yaw (handles overshoot + delay)
		float dt = Time.deltaTime;
		if (dt <= 0f) return;

		currentYaw = SpringYaw(currentYaw, ref yawVel, targetYaw, frequency, dampingRatio, dt);

		// Optional turn speed cap
		if (maxDegPerSec > 0f)
		{
			float raw = pivot.rotation.eulerAngles.y;
			float capped = Mathf.MoveTowardsAngle(raw, currentYaw, maxDegPerSec * dt);
			currentYaw = capped;
		}

		// Apply
		Quaternion rot = Quaternion.Euler(0f, currentYaw, 0f);
		pivot.rotation = rot;
	}

	bool TryGetAimPoint(out Vector3 aimPoint)
	{
		Ray ray = cam.ScreenPointToRay(Input.mousePosition);

		if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, hitMask, QueryTriggerInteraction.Ignore))
		{
			aimPoint = hit.point;
			return true;
		}

		if (fallbackToPlane)
		{
			// Intersect ray with horizontal plane at Y=planeY
			float denom = ray.direction.y;
			if (Mathf.Abs(denom) > 0.00001f)
			{
				float t = (planeY - ray.origin.y) / denom;
				if (t > 0f && t < maxRayDistance)
				{
					aimPoint = ray.origin + ray.direction * t;
					return true;
				}
			}
		}

		aimPoint = default;
		return false;
	}

	/// <summary>
	/// Critically-stable spring for angles (degrees), allowing overshoot by lowering dampingRatio.
	/// Uses a simple semi-implicit integration.
	/// </summary>
	static float SpringYaw(float current, ref float velocity, float target, float frequency, float dampingRatio, float dt)
	{
		// Work in shortest-angle space
		float x = Mathf.DeltaAngle(current, target); // error from current -> target
													 // We want to reduce error to 0; integrate on "error" then convert back.
													 // Spring constants
		float w = Mathf.Max(0.0001f, frequency * 2f * Mathf.PI); // rad/s-ish scaling
		float k = w * w;
		float c = 2f * dampingRatio * w;

		// Acceleration for error state: x'' + c x' + k x = 0  (target is 0 error)
		float a = -c * velocity - k * x;

		velocity += a * dt;
		x += velocity * dt;

		// Convert error back to yaw: target - error
		return target - x;
	}
}
