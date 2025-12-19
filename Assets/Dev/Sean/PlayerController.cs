using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float health = 10f;
    public bool canMove = true;
    public int exp = 0;
    public Rigidbody rb;
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
}
