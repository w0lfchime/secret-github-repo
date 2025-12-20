using UnityEngine;

public class AlexHammerController : MonoBehaviour
{
    public Transform hammer;
    public Rigidbody rb;
    public PlayerController pc;
    public float damage = 5f;
    public float speed;
    public float spinMultiplier = 1f;
    public float limit = 10;
    public float slowDownSpeed = .1f;
    public LayerMask groundLayer;
    private bool clicking;

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            clicking = true;
        }else clicking = false;
    }
    void FixedUpdate()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            Vector3 directionToMouse = (hit.point - hammer.position).normalized;
            Vector3 directionToHammer = -(hammer.transform.position - transform.position).normalized;

            float angleToMouse = Vector3.SignedAngle(-directionToHammer, directionToMouse, Vector3.up);
            if (clicking)
            {
                speed += angleToMouse * spinMultiplier;
                speed = Mathf.Clamp(speed,  -limit, limit);
            }else speed = speed*slowDownSpeed;
            
        }

        rb.angularVelocity = Vector3.up * speed;
    }

    public void OnTriggerEnter(Collider col) {
        Debug.Log("Collision entered");
        Debug.Log(speed);

        if(col.gameObject.tag == "Enemy")
        {
            Debug.Log("Hit an enemy with velovity: " + speed);
            Hittable hit = col.gameObject.GetComponent<Hittable>();

            hit.health -= damage * Mathf.Abs(speed/limit);

            if(hit.health <= 0)
            {
                pc.exp += hit.expAmount;
                Destroy(col.gameObject);

                if(pc.exp >= pc.expToNextLevelUp)
                {
                    pc.levelUp();
                }
            }
        }
    }

}
