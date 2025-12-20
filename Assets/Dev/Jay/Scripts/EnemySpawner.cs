using UnityEngine;
using UnityEngine.Splines;

public class EnemySpawner : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private SplinePathSet pathSet;

	[SerializeField] private GameObject enemyPrefab;

	[Header("Spawn")]
	[SerializeField] private Transform spawnPoint;
	[SerializeField] private float spawnInterval = 1.0f;
	[SerializeField] private int maxAlive = 30;

	[Header("Spawn Offset")]
	[SerializeField] private Vector3 localOffset = Vector3.zero;

	[SerializeField] private Vector3 localRandomOffsetMin = Vector3.zero;
	[SerializeField] private Vector3 localRandomOffsetMax = Vector3.zero;


	private float _timer;
	private int _alive;

	private void Reset()
	{
		spawnPoint = transform;
	}

	private void Update()
	{
		if (_alive >= maxAlive) return;

		_timer += Time.deltaTime;
		if (_timer >= spawnInterval)
		{
			_timer = 0f;
			Spawn();
		}
	}

	private void Spawn()
	{
		Transform sp = spawnPoint ? spawnPoint : transform;

		// Base
		Vector3 pos = sp.position;
		Quaternion rot = sp.rotation;

		// Configurable + random local-space offset
		Vector3 rand = new Vector3(
			UnityEngine.Random.Range(localRandomOffsetMin.x, localRandomOffsetMax.x),
			UnityEngine.Random.Range(localRandomOffsetMin.y, localRandomOffsetMax.y),
			UnityEngine.Random.Range(localRandomOffsetMin.z, localRandomOffsetMax.z)
		);

		Vector3 worldOffset = sp.TransformVector(localOffset + rand);
		pos += worldOffset;

		GameObject go = Instantiate(enemyPrefab, pos, rot);

		var motor = go.GetComponent<SplineEnemyMotor>();
		if (motor)
		{
			motor.SetPathSet(pathSet);
			motor.OnDespawned += HandleDespawned;
		}

		_alive++;
	}


	private void HandleDespawned(SplineEnemyMotor m)
	{
		_alive = Mathf.Max(0, _alive - 1);
	}
}
