using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AlexHammerCopy : MonoBehaviour
{
    public Transform hammer;
    public Transform hammerHead;
    public Transform wizardBody;
    public Rigidbody rb;
    public PlayerController pc;
    public GameObject hammerParticles;
    public AudioClip hitClip, breakClip;
    public List<AudioClip> lowSpdHits;
    public List<AudioClip> highSpdHits;
    public float damage = 5f;
    public float speed;
    public float spinMultiplier = 1f;
    public float limit = 10;
    public float slowDownSpeed = .1f;
    public LayerMask groundLayer;
    public float additionalExp = 0;
    private bool clicking, clickUp;
    public float rotationToHit = 10;
    private float rotationTraveled;
    public float bodyLeanAmount = 30;
    public float CameraShakeAmount, CameraShakeTime;
    public float knockback = 1000;

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
        //Color finalColor = hammerMaterial.color * Mathf.Clamp(Mathf.Abs(speed/limit)-.5f, 0, 1)*2;
        //hammerMaterial.SetColor("_EmissionColor", finalColor);

        speed = speed*slowDownSpeed;

        rb.angularVelocity = Vector3.up * speed;

        rotationTraveled+=Mathf.Abs(speed)*Time.deltaTime;
        wizardBody.eulerAngles = new Vector3(Mathf.Abs(speed)/limit * bodyLeanAmount, wizardBody.eulerAngles.y, wizardBody.eulerAngles.z);

        float targetField = Mathf.Lerp(60, 80, Mathf.Abs(speed)/limit);
        Camera.main.fieldOfView += (targetField-Camera.main.fieldOfView)*.05f;

        float targetParticle = Mathf.Lerp(0, 50, Mathf.Abs(speed)/limit);
        var ps = Camera.main.gameObject.GetComponent<ParticleSystem>().emission; 
        var shape = Camera.main.gameObject.GetComponent<ParticleSystem>().shape; 
        ps.rateOverTimeMultiplier += (targetParticle-ps.rateOverTimeMultiplier)*.05f;
        shape.radius = Mathf.Lerp(.5f, .75f, Mathf.Abs(speed)/limit);

        pc.speed = Mathf.Lerp(3, 10, Mathf.Abs(speed)/limit);
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

            int randAudio = Random.Range(0, 5);
            if(speed >= 6)
            {
                AudioSource.PlayClipAtPoint(highSpdHits[randAudio], hammerHead.transform.position, 1.0f);
            } else
            {
                AudioSource.PlayClipAtPoint(lowSpdHits[randAudio], hammerHead.transform.position, 1.0f);
            }
            


            if(col.gameObject.tag == "Enemy" || col.gameObject.tag == "Crystal")
            {
                Debug.Log("Hit an enemy with velovity: " + speed);
                Hittable hit = col.gameObject.GetComponent<Hittable>();

                hit.health -= damage * Mathf.Abs(speed/limit) * (hit.iced+.5f);

                if(col.gameObject.GetComponent<Rigidbody>()) col.gameObject.GetComponent<Rigidbody>().AddForce(-new Vector3(lookDirection.x, 1, lookDirection.z) * Mathf.Abs(speed) * knockback);

                if(hit.health <= 0)
                {
                    AudioSource.PlayClipAtPoint(breakClip, hammerHead.transform.position, 1.0f);

                    speed = -speed/2;
                    spinMultiplier = -spinMultiplier;

                    col.gameObject.GetComponent<Hittable>().shatter.Shatter(-new Vector3(lookDirection.x, 0, lookDirection.z));
                    pc.exp += hit.expAmount + (int)additionalExp;
                    Destroy(col.gameObject);

                    if(pc.exp >= pc.expToNextLevelUp)
                    {
                        pc.levelUp();
                    }

                    StartCoroutine(Camera.main.GetComponent<CameraShake>().Shake(CameraShakeTime, CameraShakeAmount));
                }
            }
        }
        
    }

}
