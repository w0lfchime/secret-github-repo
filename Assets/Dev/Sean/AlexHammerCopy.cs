using UnityEngine;

public class AlexHammerCopy : MonoBehaviour
{
    public Transform hammer;
    public Transform hammerHead;
    public Rigidbody rb;
    public PlayerController pc;
    public GameObject hammerParticles;
    public AudioClip soundEffectClip;
    public float damage = 5f;
    public float speed;
    public float spinMultiplier = 1f;
    public float limit = 10;
    public float slowDownSpeed = .1f;
    public LayerMask groundLayer;
    public Material hammerMaterial;
    public float additionalExp = 0;
    private bool clicking, clickUp;
    public float rotationToHit = 10;
    private float rotationTraveled;

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
            }

            if (clickUp)
            {
                speed = angleToMouse * spinMultiplier * 30;
                clickUp = false;
            }

            speed = Mathf.Clamp(speed,  -limit, limit);
            
        }
        Color finalColor = hammerMaterial.color * Mathf.Clamp(Mathf.Abs(speed/limit)-.5f, 0, 1)*2;
        hammerMaterial.SetColor("_EmissionColor", finalColor);

        speed = speed*slowDownSpeed;

        rb.angularVelocity = Vector3.up * speed;

        rotationTraveled+=Mathf.Abs(speed)*Time.deltaTime;

        // pc.speed = Mathf.Abs(speed)+5;
    }

    public void OnTriggerEnter(Collider col) {
        if(col.gameObject.tag != "Frame" && rotationTraveled > rotationToHit)
        {
            rotationTraveled = 0;

            Debug.Log("DIDNT hit the frame");
            Debug.Log("Collision entered");
            Debug.Log(speed);
            speed = -speed*1.5f;
            spinMultiplier = -spinMultiplier;

            GameObject particleIns = Instantiate(hammerParticles, hammerHead.transform.position, Quaternion.identity);
            Vector3 lookDirection = hammerHead.transform.forward * (speed > 0 ? -1 : 1);
            particleIns.transform.forward = new Vector3(lookDirection.x, 0, lookDirection.z);
            AudioSource.PlayClipAtPoint(soundEffectClip, hammerHead.transform.position, 1.0f);


            if(col.gameObject.tag == "Enemy")
            {
                Debug.Log("Hit an enemy with velovity: " + speed);
                Hittable hit = col.gameObject.GetComponent<Hittable>();

                hit.health -= damage * Mathf.Abs(speed/limit);

                if(hit.health <= 0)
                {
                    pc.exp += hit.expAmount + (int)additionalExp;
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
