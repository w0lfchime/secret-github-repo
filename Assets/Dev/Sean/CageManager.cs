using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CageManager : MonoBehaviour
{
    public float maxHealth = 100;
    public float cageHealth = 100;
    public float depletionMultiplier = 1;
    public PlayerController pc;
    public List<Renderer> renderers = new List<Renderer>();
    public Transform icePart;
    public float enemyDamageScale = 1;
    public GameObject heatParticle;
    public List<GameObject> heatParticles = new List<GameObject>();
    public List<GameObject> enemiesTouching = new List<GameObject>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        cageHealth-=Time.deltaTime * depletionMultiplier;
        if(cageHealth <= 0)
        {
            pc.gameOver();
        }

        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        foreach(Renderer rend in renderers){
            rend.GetPropertyBlock(mpb);
            mpb.SetFloat("_Alpha", Mathf.Pow(cageHealth/maxHealth, 3));
            rend.SetPropertyBlock(mpb);
        }

        icePart.transform.localScale = new Vector3(
            .08f,
            Mathf.Lerp(0, .16f, cageHealth/maxHealth),
            .08f
        );
    }

    void FixedUpdate()
    {
        UpdateParticles();
    }

    void UpdateParticles()
    {
        enemiesTouching.RemoveAll(item => item == null);
        while(enemiesTouching.Count>heatParticles.Count)
        {
            GameObject particleIns = Instantiate(heatParticle, transform.position, Quaternion.identity);
            heatParticles.Add(particleIns);
        }
        while(enemiesTouching.Count<heatParticles.Count)
        {
            Destroy(heatParticles[0]);
            heatParticles.RemoveAt(0);
        }
    }

    public void OnCollisionStay(Collision col)
    {
        if(col.gameObject.tag == "Enemy")
        {
            if(!enemiesTouching.Contains(col.gameObject)) enemiesTouching.Add(col.gameObject);
            UpdateParticles();
            for(int i = 0; i < enemiesTouching.Count; i++)
            {
                heatParticles[i].transform.position = enemiesTouching[i].transform.position;
            }
            cageHealth-=Time.deltaTime * enemyDamageScale;
        }
    }

    void OnCollisionExit(Collision col)
    {
        if(col.gameObject.tag == "Enemy")
        {
            if(enemiesTouching.Contains(col.gameObject)) enemiesTouching.Remove(col.gameObject);
        }
    }

    public void StartCageHeal(float amount)
    {
        cageHealth += amount * 25;
        if(cageHealth > maxHealth)
        {
            cageHealth = maxHealth;
        }
    }
}
