using UnityEngine;

public class ManaShard : MonoBehaviour
{
    public float healAmount = 5;
    public float forceMult = 10;
    public GameObject particle;
    void OnTriggerStay(Collider other)
    {
        if(other.gameObject.tag == "Player" ){
            var controller = other.gameObject.GetComponent<PlayerController>();
            if(controller.mana < controller.maxMana)
            {
                GetComponent<Rigidbody>().linearVelocity += (other.bounds.center - transform.position) * forceMult * Time.deltaTime;
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Player" ){
            var controller = collision.gameObject.GetComponent<PlayerController>();
            if(controller.mana < controller.maxMana)
            {
                controller.manaHeal(healAmount);
                Instantiate(particle, transform.position, Quaternion.identity);
                Destroy(gameObject);
            }
        }
    }
}
