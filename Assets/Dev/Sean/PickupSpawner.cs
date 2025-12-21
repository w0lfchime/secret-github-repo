using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class PickupSpawner : MonoBehaviour
{
    public List<GameObject> prefabList;
    public GameObject spawnParticleEffect;
    public float pickupScale = 1.5f;
    public float particleHeightOffset = 0.5f;
    
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
        newPickup.transform.localScale = UnityEngine.Vector3.one * pickupScale;
        
        
        if (spawnParticleEffect != null)
        {
            UnityEngine.Vector3 particlePosition = newLocation + new UnityEngine.Vector3(0, particleHeightOffset, 0);
            GameObject part = GameObject.Instantiate(spawnParticleEffect, particlePosition, UnityEngine.Quaternion.identity);
            part.transform.SetParent(newPickup.transform);
        }
        
        StartCoroutine(spawnPickup());
    }
}
