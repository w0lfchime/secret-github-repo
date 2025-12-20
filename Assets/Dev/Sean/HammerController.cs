using UnityEngine;

public class HammerController : MonoBehaviour
{
    public float damage = 5f;
    public float speed = 0f;
    public Camera mainCam;
    public GameObject player;
    public Rigidbody rb;
    private Vector3 mousePosition;
    private Vector3 previousDirection;
    public LayerMask groundLayer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Vector3 initialDirection = transform.position - player.transform.position;
        transform.position = player.transform.position + initialDirection.normalized * 5;
        previousDirection = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // mousePosition = Input.mousePosition;
        // mousePosition.z = player.transform.position.z - Camera.main.transform.position.z;
        // mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
        // // mousePosition.y = player.transform.position.y;
        
        // transform.position = mousePosition;
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            Vector3 mouseWorldPosition = hit.point;
            Vector3 directionToMouse = mouseWorldPosition - player.transform.position;
            directionToMouse = directionToMouse.normalized * 5;

            Vector3 movement = player.transform.position + directionToMouse;
            movement.y = player.transform.position.y;

            transform.position = movement;

            speed = Vector3.Distance(transform.position, previousDirection) / Time.deltaTime;
            previousDirection = transform.position;
        }
    }

    public void OnCollisionEnter(Collision col) {
        Debug.Log("Collision entered");

        if(col.gameObject.tag == "Enemy")
        {
            if(speed > 5)
            {
                Debug.Log("Hit an enemy with velovity: " + speed);
                Hittable hit = col.gameObject.GetComponent<Hittable>();

                hit.health -= damage;
            }
            
        }
    }
}
