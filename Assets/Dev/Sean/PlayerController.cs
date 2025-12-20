using System.Collections;
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
    public Rigidbody rb;
    public ItemManager im;
    public GameObject itemSelection;
    public GameObject gameOverScreen;
    public GameObject pauseMenu;
    
    private bool isPaused = false;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // StartCoroutine(regen());
        // StartCoroutine(manaRegen());
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
        }
        
        if(canMove && !isPaused) {
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");

            rb.linearVelocity = (new Vector3(horizontalInput, 0f, verticalInput)) * speed;
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
