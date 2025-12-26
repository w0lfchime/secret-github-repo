using UnityEngine;

public class MirrowFollow : MonoBehaviour
{
    public LayerMask groundLayer;
    public Transform target;
    public float distance = 1;
    public float yOffset;

    void FixedUpdate()
    {
        if (target == null)
        {
            target = GameObject.FindGameObjectWithTag("PlayerModel").transform;
            transform.SetParent(target);
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            Vector3 directionToMouse = (hit.point - target.transform.position).normalized;
            directionToMouse = new Vector3(directionToMouse.x, 0, directionToMouse.z);
            transform.position = transform.parent.position + directionToMouse * distance+Vector3.up*yOffset;
            transform.LookAt(directionToMouse+transform.position);
        }
    }
}
