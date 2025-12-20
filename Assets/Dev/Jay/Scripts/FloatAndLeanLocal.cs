using UnityEngine;

public class FloatAndLeanLocal : MonoBehaviour
{
	[Header("Bob (Local Position)")]
	public float bobAmplitude = 0.25f;   // how far up/down
	public float bobFrequency = 0.8f;    // cycles per second

	[Header("Lean (Local Rotation)")]
	public float leanAngle = 8f;         // degrees
	public float leanFrequency = 0.6f;   // cycles per second

	[Header("Optional Polish")]
	public float phaseOffset = 0.0f;     // set different per object to desync
	public bool useUnscaledTime = false; // ignore timeScale (menus/slowmo)

	Vector3 _baseLocalPos;
	Quaternion _baseLocalRot;

	void Awake()
	{
		_baseLocalPos = transform.localPosition;
		_baseLocalRot = transform.localRotation;
	}

	void OnEnable()
	{
		// In case something moved it while disabled
		_baseLocalPos = transform.localPosition;
		_baseLocalRot = transform.localRotation;
	}

	void Update()
	{
		float t = (useUnscaledTime ? Time.unscaledTime : Time.time) + phaseOffset;

		// Bob (local Y)
		float bob = Mathf.Sin(t * Mathf.PI * 2f * bobFrequency) * bobAmplitude;
		transform.localPosition = _baseLocalPos + Vector3.up * bob;

		// Lean (local rotation): roll + a touch of pitch for organic motion
		float roll = Mathf.Sin(t * Mathf.PI * 2f * leanFrequency) * leanAngle;
		float pitch = Mathf.Sin(t * Mathf.PI * 2f * (leanFrequency * 0.85f) + 1.1f) * (leanAngle * 0.35f);

		transform.localRotation = _baseLocalRot * Quaternion.Euler(pitch, 0f, roll);
	}
}
