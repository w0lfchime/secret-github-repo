using System;
using System.Collections.Generic;
using TMPro;
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
    public AudioSource woosh;
    public AudioClip hitClip, breakClip;
    public List<AudioClip> lowSpdHits;
    public List<AudioClip> highSpdHits;
    public float damage = 5f;
    public float speed;
    public float speedMulti = 1;
    public float spinMultiplier = 1f;
    private float direction = 1;
    public float limit = 10;
    public float slowDownSpeed = .1f;
    public float breakSpeedSlowDown = .5f;
    public LayerMask groundLayer;
    public float additionalExp = 0;
    private bool clicking, clickUp;
    public float rotationToHit = 10;
    private float rotationTraveled;
    public float bodyLeanAmount = 30;
    public float CameraShakeAmount, CameraShakeTime;
    public float knockback = 1000;
    public float lifeSteal = 0;
    public Vector3 hammerScale = new Vector3(1, 1, 1);
    public GameObject DamageText;

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            clicking = true;
        }else clicking = false;
        if (Input.GetMouseButtonUp(0)){
            //clickUp = true;
        }

        woosh.pitch = Math.Abs(speed) / limit;
        
        if(!woosh.isPlaying)
        {
            woosh.pitch = Math.Abs(speed) / limit;
            woosh.Play();
        }
    }
    void FixedUpdate()
    {

        speed += spinMultiplier * direction;
        speed = Mathf.Clamp(speed, -limit, limit);
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

        pc.speed = Mathf.Lerp(3, 10, Mathf.Abs(speed * speedMulti)/limit);
    }

    public void OnTriggerEnter(Collider col) {
        if(col.gameObject.tag != "Frame" && rotationTraveled > rotationToHit)
        {
            rotationTraveled = 0;

            Debug.Log("Collision entered: " + col.gameObject.name);
            speed = -speed*1.5f;
            direction = -direction;

            GameObject particleIns = Instantiate(hammerParticles, hammerHead.transform.position, Quaternion.identity);
            Vector3 lookDirection = hammerHead.transform.forward * (speed > 0 ? -1 : 1);
            particleIns.transform.forward = new Vector3(lookDirection.x, 0, lookDirection.z);

            int randAudio = UnityEngine.Random.Range(0, 5);
            if(speed >= 6)
            {
                AudioSource.PlayClipAtPoint(highSpdHits[randAudio], hammerHead.transform.position, 1.0f);
            } else
            {
                AudioSource.PlayClipAtPoint(lowSpdHits[randAudio], hammerHead.transform.position, 1.0f);
            }
            


            if(col.gameObject.tag == "Enemy" || col.gameObject.tag == "Crystal")
            {
                Hittable hit = col.gameObject.GetComponent<Hittable>();
                float totalDamage = damage * Mathf.Abs(speed/limit) * (hit.iced+.5f);

                GameObject damageTextIns = Instantiate(DamageText, -lookDirection/2+hammerHead.transform.position, Quaternion.identity);
                Destroy(damageTextIns, .5f);
                damageTextIns.transform.GetChild(0).GetComponent<TMP_Text>().text = (Mathf.Round(totalDamage*10)/10).ToString();
                damageTextIns.transform.GetChild(0).GetComponent<TMP_Text>().color = Color.Lerp(Color.red, Color.blue, hit.iced);

                hit.health -= totalDamage;

                if(col.gameObject.GetComponent<Rigidbody>()) col.gameObject.GetComponent<Rigidbody>().AddForce(-new Vector3(lookDirection.x, 1, lookDirection.z) * Mathf.Abs(speed) * knockback);

                if(hit.health <= 0)
                {
                    AudioSource.PlayClipAtPoint(breakClip, hammerHead.transform.position, 1.0f);
                    rotationTraveled+=rotationToHit;

                    speed = -speed*breakSpeedSlowDown;
                    direction = -direction;

                    col.gameObject.GetComponent<Hittable>().shatter.Shatter(-new Vector3(lookDirection.x, 0, lookDirection.z));
                    pc.exp += hit.expAmount + (int)additionalExp;
                    
                    if (col.gameObject.GetComponent<SpawnCrystal>())
                    {
                        col.gameObject.GetComponent<SpawnCrystal>().DestroyCrystal(-new Vector3(lookDirection.x, 0, lookDirection.z));
                    }else Destroy(col.gameObject);

                    if(pc.exp >= pc.expToNextLevelUp)
                    {
                        pc.levelUp();
                    }

                    StartCoroutine(Camera.main.GetComponent<CameraShake>().Shake(CameraShakeTime, CameraShakeAmount));
                }

                if(col.tag == "Enemy")
                {
                    pc.heal(lifeSteal);
                }
            }
        }
        
    }

    public void changeHammerScale(float changeBy)
    {
        hammer.localScale += new Vector3(changeBy, changeBy, changeBy);
    }

}
