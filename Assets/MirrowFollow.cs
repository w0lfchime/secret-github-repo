using UnityEngine;

public class MirrowFollow : MonoBehaviour
{
    public LayerMask groundLayer;
    public Transform target;
    public float distance = 1;

    void Start()
    {
        // target = GameObject.Find("Player").transform;
        if (target == null)
        {
            target = transform.parent;
        }
    }
    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            Vector3 directionToMouse = (hit.point - target.transform.position).normalized;
            directionToMouse = new Vector3(directionToMouse.x, 0, directionToMouse.z);
            transform.position = transform.parent.position + directionToMouse * distance;
        }
    }
}
