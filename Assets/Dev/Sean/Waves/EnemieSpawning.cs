using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemieSpawning : MonoBehaviour
{
    public List<int> spawnMulti;
    public List<GameObject> prefabs;
    [SerializeField] private Vector3 localOffset = Vector3.zero;

	[SerializeField] private Vector3 localRandomOffsetMin = Vector3.zero;
	[SerializeField] private Vector3 localRandomOffsetMax = Vector3.zero;
    [SerializeField] private SplinePathSet pathSet;
    public float waveTime = 45f;

    public AudioSource audioSource;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(doTheRoar());
        StartCoroutine(waitToStartSpawning());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void spawn(GameObject prefab)
    {
        Transform sp = transform;

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

		GameObject go = Instantiate(prefab, pos, rot);

		var motor = go.GetComponent<SplineEnemyMotor>();
		if (motor)
		{
			motor.SetPathSet(pathSet);
		}
    }

    public IEnumerator doTheRoar()
    {
        audioSource.Play();
        yield return new WaitForSeconds(waveTime);
        StartCoroutine(doTheRoar());
    }

    // Edit this to change when things start to spawn
    public IEnumerator waitToStartSpawning()
    {
        StartCoroutine(spawnGreenSlimes());

        yield return new WaitForSeconds(waveTime);
        StartCoroutine(spawnYellowSlimes());

        yield return new WaitForSeconds(waveTime);
        StartCoroutine(spawnRedSlimes());

        yield return new WaitForSeconds(waveTime);
        StartCoroutine(spawnSnakes());

        yield return new WaitForSeconds(waveTime);
        StartCoroutine(spawnFireSpirit());

        yield return new WaitForSeconds(waveTime);
        StartCoroutine(spawnSmallBoi());

        yield return new WaitForSeconds(waveTime);
        StartCoroutine(spawnCentaur());
    }

    // Edit these to change enemy spawn amount and how often they spawn once initally spawned
    public IEnumerator spawnGreenSlimes()
    {
        for(int i = 0; i < spawnMulti[0]; i++)
        {
            for(int j = 0; j < 10; j++)
            {
                // Spawn amount
                spawn(prefabs[0]);
            }
        }
        // Spawn frequency
        yield return new WaitForSeconds(waveTime);
        StartCoroutine(spawnGreenSlimes());
    }

    

    public IEnumerator spawnYellowSlimes()
    {
        
        for(int i = 0; i < spawnMulti[1]; i++)
        {
            for(int j = 0; j < 5; j++)
            {
                spawn(prefabs[1]);
            }
        }
        yield return new WaitForSeconds(waveTime);
        StartCoroutine(spawnYellowSlimes());
    }

    public IEnumerator spawnRedSlimes()
    {
        
        for(int i = 0; i < spawnMulti[2]; i++)
        {
            for(int j = 0; j < 5; j++)
            {
                spawn(prefabs[2]);
            }
        }
        yield return new WaitForSeconds(waveTime);
        StartCoroutine(spawnRedSlimes());
    }

    public IEnumerator spawnSnakes()
    {
        
        for(int i = 0; i < spawnMulti[3]; i++)
        {
            for(int j = 0; j < 5; j++)
            {
                spawn(prefabs[3]);
            }
        }
        yield return new WaitForSeconds(waveTime);
        StartCoroutine(spawnSnakes());
    }
    
    public IEnumerator spawnFireSpirit()
    {
        
        for(int i = 0; i < spawnMulti[4]; i++)
        {
            for(int j = 0; j < 5; j++)
            {
                spawn(prefabs[4]);
            }
        }
        yield return new WaitForSeconds(waveTime);
        StartCoroutine(spawnFireSpirit());
    }

    public IEnumerator spawnFishMan()
    {
        
        for(int i = 0; i < spawnMulti[5]; i++)
        {
            for(int j = 0; j < 5; j++)
            {
                spawn(prefabs[5]);
            }
        }
        yield return new WaitForSeconds(waveTime);
        StartCoroutine(spawnFishMan());
    }

    public IEnumerator spawnSmallBoi()
    {
        
        for(int i = 0; i < spawnMulti[6]; i++)
        {
            for(int j = 0; j < 5; j++)
            {
                spawn(prefabs[6]);
            }
        }
        yield return new WaitForSeconds(waveTime);
        StartCoroutine(spawnSmallBoi());
    }

    public IEnumerator spawnCentaur()
    {
        
        for(int i = 0; i < spawnMulti[7]; i++)
        {
            for(int j = 0; j < 5; j++)
            {
                spawn(prefabs[7]);
            }
        }
        yield return new WaitForSeconds(waveTime);
        StartCoroutine(spawnCentaur());
    }
}
