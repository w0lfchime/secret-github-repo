using Unity.VisualScripting;
using UnityEngine;

public class CageManager : MonoBehaviour
{
    public float maxHealth = 100;
    public float cageHealth = 100;
    public PlayerController pc;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(cageHealth <= 0)
        {
            pc.gameOver();
        }
    }

    public void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.tag == "Enemy")
        {
            Hittable hit = col.GetComponent<Hittable>();

            hit.attackingCage = true;
            hit.startEnemyCageAttack();
        }
    }

    public void OnTriggerExit(Collider col)
    {
        if(col.gameObject.tag == "Enemy")
        {
            Hittable hit = col.GetComponent<Hittable>();

            hit.attackingCage = false;
        }
    }

    
}
