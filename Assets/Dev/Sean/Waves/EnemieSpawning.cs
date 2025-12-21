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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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

    public IEnumerator waitToStartSpawning()
    {
        StartCoroutine(spawnGreenSlimes());
        yield return new WaitForSeconds(30f);
        Debug.Log("Started yellow slimes");
        StartCoroutine(spawnYellowSlimes());
        yield return new WaitForSeconds(60f);
        Debug.Log("Started red slimes");
        StartCoroutine(spawnRedSlimes());
    }

    public IEnumerator spawnGreenSlimes()
    {
       
        for(int i = 0; i < spawnMulti[0]; i++)
        {
            for(int j = 0; j < 15; j++)
            {
                spawn(prefabs[0]);
            }
        }
        yield return new WaitForSeconds(30f);
        StartCoroutine(spawnGreenSlimes());
    }

    

    public IEnumerator spawnYellowSlimes()
    {
        
        for(int i = 0; i < spawnMulti[1]; i++)
        {
            for(int j = 0; j < 10; j++)
            {
                spawn(prefabs[1]);
            }
        }
        yield return new WaitForSeconds(30f);
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
        yield return new WaitForSeconds(30f);
        StartCoroutine(spawnRedSlimes());
    }

    public IEnumerator spawnSnakes()
    {
        
        for(int i = 0; i < spawnMulti[3]; i++)
        {
            for(int j = 0; j < 10; j++)
            {
                spawn(prefabs[3]);
            }
        }
        yield return new WaitForSeconds(60f);
        StartCoroutine(spawnSnakes());
    }
}
