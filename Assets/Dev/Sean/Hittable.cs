using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Hittable : MonoBehaviour
{
    public CageManager cm;
    public bool attackingCage = false;
    public float health = 10;
    public float damage = 5;
    public float attackSpeed = 1f;
    public int expAmount;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cm = GameObject.Find("Cage").GetComponent<CageManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision col)
    {
        if(col.gameObject.tag == "Cage")
        {
            attackingCage = true;
            StartCoroutine(attackCage());
        }
    }

    void OnCollisionExit(Collision col)
    {
        if(col.gameObject.tag == "Cage")
        {
            attackingCage = false;
            StopAllCoroutines();
        }
    }

    public IEnumerator attackCage()
    {
        yield return new WaitForSeconds(attackSpeed);
        if(attackingCage)
        {
            cm.cageHealth -= damage;
            StartCoroutine(attackCage());
        }
    } 
}
