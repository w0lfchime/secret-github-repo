using System.Collections.Generic;
using UnityEngine;

public class SpawnCrystal : MonoBehaviour
{
    public List<Mesh> crystals = new List<Mesh>();
    public float chance;
    void Start()
    {
        float number = Random.Range(0f, 100f);
        if(chance >= number){
            GetComponent<MeshFilter>().mesh = crystals[Random.Range(0, crystals.Count-1)];
        }else Destroy(gameObject);
        
    }
}
