using UnityEngine;

public class CameraPivotSpringFollow : MonoBehaviour
{
	[Header("Target")]
	public Transform target;

	[Header("Follow Offset (world-space)")]
	public Vector3 offset = Vector3.zero;

	[Header("Spring Feel")]
	[Tooltip("Higher = snappier. Typical: 3 to 12")]
	public float frequency = 6f;

	[Tooltip("1 = critically damped (no oscillation). <1 bouncy, >1 heavier")]
	public float dampingRatio = 1f;

	[Header("Limits")]
	[Tooltip("Optional cap to prevent insane catch-up speeds")]
	public float maxSpeed = 40f;

	[Header("Axes")]
	public bool followX = true;
	public bool followY = true;
	public bool followZ = true;

	[Header("Update")]
	[Tooltip("LateUpdate recommended for camera rigs")]
	public bool useLateUpdate = true;

	Vector3 _vel; // internal spring velocity

	void Reset()
	{
		// reasonable defaults
		frequency = 6f;
		dampingRatio = 1f;
		maxSpeed = 40f;
		useLateUpdate = true;
	}

	void Update()
	{
		if (!useLateUpdate) Tick(Time.deltaTime);
	}

	void LateUpdate()
	{
		if (useLateUpdate) Tick(Time.deltaTime);
	}

	void Tick(float dt)
	{
		if (!target || dt <= 0f) return;

		Vector3 desired = target.position + offset;

		// Axis locking
		Vector3 pos = transform.position;
		if (!followX) desired.x = pos.x;
		if (!followY) desired.y = pos.y;
		if (!followZ) desired.z = pos.z;

		// Critically-damped spring (stable and distance-responsive)
		// accel = w^2 * (x_target - x) - 2*z*w*v
		float w = Mathf.Max(0.0001f, 2f * Mathf.PI * frequency);
		float z = Mathf.Clamp(dampingRatio, 0f, 5f);

		Vector3 x = pos;
		Vector3 v = _vel;

		Vector3 accel = (w * w) * (desired - x) - (2f * z * w) * v;

		v += accel * dt;

		// optional speed cap
		float spd = v.magnitude;
		if (maxSpeed > 0f && spd > maxSpeed)
			v = v * (maxSpeed / spd);

		x += v * dt;

		_vel = v;
		transform.position = x;
	}

	/// Call this if you teleport the target and want the pivot to snap instantly.
	public void SnapToTarget()
	{
		if (!target) return;
		Vector3 desired = target.position + offset;
		transform.position = desired;
		_vel = Vector3.zero;
	}
}
