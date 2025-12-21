using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class CageManager : MonoBehaviour
{
    public float maxHealth = 100;
    public float cageHealth = 100;
    public float depletionMultiplier = 1;
    public PlayerController pc;
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
    }

    public void OnCollisionEnter(Collision col)
    {
        if(col.gameObject.tag == "Enemy")
        {
            Hittable hit = col.gameObject.GetComponent<Hittable>();

            hit.attackingCage = true;
            hit.startEnemyCageAttack();
        }
    }

    public void OnCollisionExit(Collision col)
    {
        if(col.gameObject.tag == "Enemy")
        {
            Hittable hit = col.gameObject.GetComponent<Hittable>();

            hit.attackingCage = false;
        }
    }

    public void StartCageHeal(float amount)
    {
        cageHealth += amount * 50;
        if(cageHealth > maxHealth)
        {
            cageHealth = maxHealth;
        }
    }
}
