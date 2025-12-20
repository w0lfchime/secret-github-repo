using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Hittable : MonoBehaviour
{
    public CageManager cm;
    public Renderer rend;
    public bool attackingCage = false;
    public float health = 10;
    public float iced;
    public float damage = 5;
    public float attackSpeed = 1f;
    public int expAmount;
    public TetraShatter shatter;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cm = GameObject.Find("Cage").GetComponent<CageManager>();
    }

    // Update is called once per frame
    void Update()
    {
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();

        rend.GetPropertyBlock(mpb);
        mpb.SetFloat("_Alpha", iced);
        rend.SetPropertyBlock(mpb);
    }

    public void startEnemyCageAttack()
    {
        StartCoroutine(attackCage());
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

    void OnCollisionStay(Collision collision)
    {
        GetComponent<SplineEnemyMotor>().enabled = true;
    }
}
