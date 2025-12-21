using System.Collections.Generic;
using UnityEngine;

public class SpawnCrystal : MonoBehaviour
{
    public List<Mesh> crystals = new List<Mesh>();
    public float chance;
    public GameObject manaShard;
    public Vector2 spawnRange;
    void Start()
    {
        float number = Random.Range(0f, 100f);
        if(chance >= number){
            GetComponent<MeshFilter>().mesh = crystals[Random.Range(0, crystals.Count-1)];
        }else Destroy(gameObject);
    }

    public void DestroyCrystal(Vector3 direction)
    {
        float amount = Random.Range(spawnRange.x, spawnRange.y);
        for(int i = 0; i < amount; i++)
        {
            Vector3 offset = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * .1f;
            GameObject crystalIns = Instantiate(manaShard, transform.position, Quaternion.identity);
            crystalIns.GetComponent<Rigidbody>().AddForce(direction * 3000);
        }
    }
}
