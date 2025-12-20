using UnityEngine;

public class AlexPlayerController : MonoBehaviour
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
    public LayerMask groundLayer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            Vector3 directionToMouse = (hit.point - transform.position).normalized;
            if(canMove) {
                rb.linearVelocity = (new Vector3(directionToMouse.x, 0f, directionToMouse.z)) * speed;
            }
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
