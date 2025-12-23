using System.Collections;
using JetBrains.Annotations;
using NUnit.Framework;
using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float maxHealth = 10f;
    public float health = 10f;
    public float maxMana = 10f;
    public float mana = 10f;
    public float manaRegenTime = 2f;
    public float maxStamina = 10f;
    public float stamina = 10f;
    public float staminaRegenTime = 2f; 
    public float regenTime = 2f;
    public bool canMove = true;
    public int exp = 0;
    public int expToNextLevelUp = 10;
    public bool beingAttacked = false;
    public bool isDraining = false;
    public bool isDrainingStamina = false;
    public int level = 1;
    public Rigidbody rb;
    public ItemManager im;
    public GameObject itemSelection;
    public GameObject gameOverScreen;
    public GameObject pauseMenu;
    public AudioClip movementAudio;
    public AudioSource itemSound;
    public AudioSource movementAudioSource;
    public AudioSource stageMusic;
    public AudioClip endSound;
    public AudioClip scrollSound;
    public TextMeshProUGUI gameOverText;
    public bool isWalking = false;
    public bool hasLost = false;
    public SlowTime slowTime;
    
    private bool isPaused = false;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(regen());
        StartCoroutine(staminaRegen());
        // StartCoroutine(manaRegen());
    }

    // Update is called once per frame
    void Update()
    {
        if(!isWalking)
        {
            movementAudioSource.Stop();
        } else if(!movementAudioSource.isPlaying)
        {
            movementAudioSource.Play();
        }
        if(Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
        }
        
        if(canMove && !isPaused) {
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");
            
            if(horizontalInput != 0 || verticalInput != 0)
            {
                isWalking = true;
            } else
            {
                isWalking = false;
            }

            rb.linearVelocity = (new Vector3(horizontalInput, 0f, verticalInput)) * speed;
        }

        if(health <= 0)
        {
            gameOver();
        }
    }

    private void TogglePause()
    {
        isPaused = !isPaused;
        
        if(isPaused)
        {
            Time.timeScale = 0f;
            pauseMenu.SetActive(true);
        }
        else
        {
            Time.timeScale = 1f;
            pauseMenu.SetActive(false);
        }
    }

    public void levelUp()
    {
        // do something to get a new item!
        itemSound.Play();
        level++;
        im.TriggerItemSelection();
        itemSelection.SetActive(true);
        expToNextLevelUp += (int)(1.2 * expToNextLevelUp);
        Debug.Log("Leveled up");
    }

    public void gameOver()
    {
        gameOverText.SetText("Level: " + level + "\nScore: " + exp);
        gameOverScreen.SetActive(true);

        if(!hasLost)
        {
            hasLost = true;
            setEndSound();
        }
        
        slowTime.slowTime = true;
    }

    public void setEndSound()
    {
        stageMusic.clip = endSound;
        stageMusic.loop = false;
        stageMusic.Play();
    }

    public void heal(float healAmount)
    {
        health += healAmount;

        if(health > maxHealth)
        {
            health = maxHealth;
        }
    }

    public void manaHeal(float healAmount)
    {
        mana += healAmount;

        if(mana > maxMana)
        {
            mana = maxMana;
        }
    }

    public void staminaHeal(float healAmount)
    {
        stamina += healAmount;

        if(stamina > maxStamina)
        {
            stamina = maxStamina;
        }
    }

    public void drainMana()
    {
        mana -= 1;
        StartCoroutine(drainManaReal());
    }

    public void drainSingle()
    {
        mana -= 1;
    }

    public IEnumerator drainManaReal()
    {
        yield return new WaitForSeconds(1f);
        if(isDraining)
        {
            mana -= 1;
            StartCoroutine(drainManaReal());
        }
    }

    public void drainStamina()
    {
        stamina -= 1;
        StartCoroutine(drainStaminaReal());
    }

    public IEnumerator drainStaminaReal()
    {
        yield return new WaitForSeconds(1f);
        if(isDrainingStamina)
        {
            stamina -= 1;
            StartCoroutine(drainStaminaReal());
        }
    }

    public void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.tag == "Pickup")
        {
            Debug.Log("Touched Pickup");
            Pickup pickup = col.gameObject.GetComponent<Pickup>();

            if(pickup.type == 1)
            {
                AudioSource.PlayClipAtPoint(scrollSound, col.transform.position, 1.0f);
                heal(pickup.increaseBy);
            } else if(pickup.type == 2)
            {
                AudioSource.PlayClipAtPoint(scrollSound, col.transform.position, 1.0f);
                manaHeal(pickup.increaseBy);
            }
            Instantiate(pickup.particle, pickup.transform.position, Quaternion.identity);
            Destroy(col.gameObject);
        }
    }

    public void OnCollisionEnter(Collision col)
    {
        if(col.gameObject.tag == "Enemy")
        {
            Debug.Log("Got hit by enemy");
            if(!beingAttacked)
            {
                Hittable hit = col.gameObject.GetComponent<Hittable>();

                StartCoroutine(IFrames(hit.damage));
            }
        }
    }

    public void OnCollisionStay(Collision col)
    {
        if(col.gameObject.tag == "Enemy")
        {
            Debug.Log("Got hit by enemy");
            if(!beingAttacked)
            {
                Hittable hit = col.gameObject.GetComponent<Hittable>();

                StartCoroutine(IFrames(hit.damage));
            }
        }
    }

    public IEnumerator IFrames(float damage)
    {
        beingAttacked = true;
        health -= damage;
        yield return new WaitForSeconds(.5f);
        beingAttacked = false;
    }

    public IEnumerator regen()
    {
        yield return new WaitForSeconds(regenTime);
        heal(1);
        StartCoroutine(regen());
    }

    public IEnumerator manaRegen()
    {
        yield return new WaitForSeconds(manaRegenTime);
        manaHeal(1);
        StartCoroutine(manaRegen());
    }

    public IEnumerator staminaRegen()
    {
        yield return new WaitForSeconds(staminaRegenTime);
        staminaHeal(1);
        StartCoroutine(staminaRegen());
    }
}
