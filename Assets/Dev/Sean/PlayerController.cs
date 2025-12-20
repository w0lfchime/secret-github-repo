using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float maxHealth = 10f;
    public float health = 10f;
    public float regenTime = 2f;
    public bool canMove = true;
    public int exp = 0;
    public int expToNextLevelUp = 10;
    public Rigidbody rb;
    public ItemManager im;
    public GameObject itemSelection;
    public GameObject gameOverScreen;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(canMove) {
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");

            rb.linearVelocity = (new Vector3(horizontalInput, 0f, verticalInput)) * speed;
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
}
