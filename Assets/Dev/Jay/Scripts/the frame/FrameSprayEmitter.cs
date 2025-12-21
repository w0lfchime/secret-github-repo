using UnityEngine;

public class FrameSprayEmitter : MonoBehaviour
{
	public float icePower = 1;
	[Header("References")]
	[SerializeField] private ParticleSystem sprayParticles;

	[Header("Input")]
	[SerializeField] private bool holdToSpray = true;      // true: while held, false: single burst per click

	[Header("Spray Burst")]
	[SerializeField] private int particlesPerSecond = 120; // emitted while spraying (hold mode)
	[SerializeField] private int particlesPerClick = 40;   // emitted on click (tap mode)

	[Header("SphereCast (spray hit volume)")]
	[SerializeField] private float castRadius = 0.12f;
	[SerializeField] private float castDistance = 3.0f;
	[SerializeField] private LayerMask hitMask = ~0;
	[SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;
	[SerializeField] private float castStartOffset = 0.0f; // move start forward if you want (avoid self-hit)

	[Header("Debug / Gizmos")]
	[SerializeField] private bool drawGizmos = true;
	[SerializeField] private bool drawOnlyWhenSelected = true;
	// cached hit info for gizmos
	private bool _hasHit;
	private RaycastHit _hit;

	private void Reset()
	{
		// Try auto-find on same object or children
		if (!sprayParticles) sprayParticles = GetComponentInChildren<ParticleSystem>();
	}

	private void Awake()
	{
		if (sprayParticles)
			sprayParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
	}

	private void Update()
	{
		if (!sprayParticles) return;

		if (holdToSpray)
		{
			bool spraying = Input.GetButton("Fire2"); // Right-click or Left Ctrl
			if (spraying)
			{
				EmitContinuous();
				DoSphereCast();
			}
			else
			{
				_hasHit = false;
			}
		}
		else
		{
			if (Input.GetButtonDown("Fire2")) // Right-click or Left Ctrl
			{
				Debug.Log("Right click pressed - spraying");
				EmitClick();
				DoSphereCast();
			}
			else
			{
				_hasHit = false;
			}
		}
	}

	private void EmitContinuous()
	{
		// Emit based on time so it's stable across framerate
		int count = Mathf.Max(0, Mathf.RoundToInt(particlesPerSecond * Time.deltaTime));
		if (count > 0)
			sprayParticles.Emit(count);

		if (!sprayParticles.isPlaying)
			sprayParticles.Play(true);
	}

	private void EmitClick()
	{
		sprayParticles.Emit(Mathf.Max(1, particlesPerClick));
		sprayParticles.Play(true);
	}

	private void DoSphereCast()
	{
		Vector3 origin = transform.position + transform.forward * castStartOffset;
		Vector3 dir = transform.forward;

		_hasHit = Physics.SphereCast(
			origin,
			castRadius,
			dir,
			out _hit,
			castDistance,
			hitMask,
			triggerInteraction
		);

		if (_hasHit)
		{
			Debug.Log($"[SprayEmitter] Hit {_hit.collider.name} at {_hit.point} (normal {_hit.normal})", _hit.collider);
			if(_hit.collider.gameObject.tag == "Enemy")
			{
				_hit.collider.GetComponent<Hittable>().iced = Mathf.Clamp(_hit.collider.GetComponent<Hittable>().iced + icePower*Time.deltaTime, 0, 1);
			} else if (_hit.collider.gameObject.tag == "Cage")
			{
				_hit.collider.GetComponent<CageManager>().StartCageHeal();
			}
		}

		// If you want: apply paint, decals, splat, etc. right here when _hasHit is true.
		// Example hook:
		// if (_hasHit) { var paintable = _hit.collider.GetComponent<Paintable>(); if (paintable) paintable.Paint(_hit.point, _hit.normal); }
	}

	// -------- Gizmos --------

	private void OnDrawGizmos()
	{
		if (!drawGizmos) return;
		if (drawOnlyWhenSelected) return;
		DrawCastGizmos();
	}

	private void OnDrawGizmosSelected()
	{
		if (!drawGizmos) return;
		if (!drawOnlyWhenSelected) return;
		DrawCastGizmos();
	}

	private void DrawCastGizmos()
	{
		Vector3 origin = transform.position + transform.forward * castStartOffset;
		Vector3 end = origin + transform.forward * castDistance;

		// Draw cast path as a capsule (two spheres + connecting lines)
		Gizmos.color = new Color(0f, 1f, 1f, 0.9f);
		Gizmos.DrawWireSphere(origin, castRadius);
		Gizmos.DrawWireSphere(end, castRadius);

		// "Capsule" edges (approx)
		Vector3 right = transform.right * castRadius;
		Vector3 up = transform.up * castRadius;

		Gizmos.DrawLine(origin + right, end + right);
		Gizmos.DrawLine(origin - right, end - right);
		Gizmos.DrawLine(origin + up, end + up);
		Gizmos.DrawLine(origin - up, end - up);

		// center line
		Gizmos.color = new Color(0f, 0.6f, 1f, 0.6f);
		Gizmos.DrawLine(origin, end);

		// Hit marker
		if (_hasHit)
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(_hit.point, castRadius * 0.35f);

			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(_hit.point, _hit.point + _hit.normal * (castRadius * 1.5f));
		}
	}
}
