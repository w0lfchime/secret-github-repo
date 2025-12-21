using UnityEngine;

public class ManaShard : MonoBehaviour
{
    public float healAmount = 5;
    public float forceMult = 10;
    public GameObject particle;
    void OnTriggerStay(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            GetComponent<Rigidbody>().AddForce((other.gameObject.transform.position - transform.position) * forceMult);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            collision.gameObject.GetComponent<PlayerController>().manaHeal(healAmount);
            Instantiate(particle, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
