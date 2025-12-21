using System.Collections;
using NUnit.Framework;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float maxHealth = 10f;
    public float health = 10f;
    public float maxMana = 10f;
    public float mana = 10f;
    public float manaRegenTime = 2f;
    public float regenTime = 2f;
    public bool canMove = true;
    public int exp = 0;
    public int expToNextLevelUp = 10;
    public bool beingAttacked = false;
    public Rigidbody rb;
    public ItemManager im;
    public GameObject itemSelection;
    public GameObject gameOverScreen;
    public GameObject pauseMenu;
    public AudioClip movementAudio;
    public AudioSource movementAudioSource;
    public bool isWalking = false;
    
    private bool isPaused = false;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(regen());
        StartCoroutine(manaRegen());
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
        im.TriggerItemSelection();
        itemSelection.SetActive(true);
        expToNextLevelUp += (int)(1.5 * expToNextLevelUp);
        Debug.Log("Leveled up");
    }

    public void gameOver()
    {
        gameOverScreen.SetActive(true);
        Time.timeScale = 0;
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
            health = maxHealth;
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
                heal(pickup.increaseBy);
            } else if(pickup.type == 2)
            {
                manaHeal(pickup.increaseBy);
            }

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
}
