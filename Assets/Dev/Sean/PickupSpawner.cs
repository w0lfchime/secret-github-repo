using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class PickupSpawner : MonoBehaviour
{
    public List<GameObject> prefabList;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(spawnPickup());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator spawnPickup()
    {
        yield return new WaitForSeconds(Random.Range(1, 11));
        int toSpawn = Random.Range(0, 2);

        GameObject newPickup = GameObject.Instantiate(prefabList[toSpawn]);
        UnityEngine.Vector3 newLocation = new UnityEngine.Vector3(Random.Range(-16, 1), -7, Random.Range(-13, 6));
        newPickup.transform.position = newLocation;
        
        StartCoroutine(spawnPickup());
    }
}
