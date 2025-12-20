using UnityEngine;

public class AlexHammerCopy : MonoBehaviour
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
    public Material hammerMaterial;
    private bool clicking, clickUp;

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            clicking = true;
        }else clicking = false;
        if (Input.GetMouseButtonUp(0)){
            //clickUp = true;
        }
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
                speed += spinMultiplier;
                speed = Mathf.Clamp(speed,  -limit, limit);
            }

            if (clickUp)
            {
                speed = angleToMouse * spinMultiplier * 30;
                clickUp = false;
            }
            
        }
        Color finalColor = hammerMaterial.color * Mathf.Clamp(Mathf.Abs(speed/limit)-.5f, 0, 1)*2;
        hammerMaterial.SetColor("_EmissionColor", finalColor);

        speed = speed*slowDownSpeed;

        rb.angularVelocity = Vector3.up * speed;

        // pc.speed = Mathf.Abs(speed)+5;
    }

    public void OnTriggerEnter(Collider col) {
        if(col.gameObject.tag != "Frame")
        {
            Debug.Log("DIDNT hit the frame");
            Debug.Log("Collision entered");
            Debug.Log(speed);
            speed = -speed*1.5f;
            spinMultiplier = -spinMultiplier;

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

}
