using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyMovement : MonoBehaviour
{
    public float targetSpeed = 5;
    private float tempTargetSpeed;
    public float acceleration = 1;
    public float chaseRadius = 10;
    public LayerMask playerMask;
    public Rigidbody rb;
    public AnimationCurve movementCurve;
    public AnimationCurve heightCurve;
    public float curveTime = 1;
    private float tempCurveTime;
    public Transform visual;
    private Transform target;
    private Transform player, cage;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        cage = GameObject.FindGameObjectWithTag("Cage").transform;
    }
    void FixedUpdate()
    {
        tempCurveTime+=Time.deltaTime/curveTime;
        if(tempCurveTime > 1) tempCurveTime = 0;
        tempTargetSpeed = movementCurve.Evaluate(tempCurveTime) * targetSpeed;
        visual.transform.localPosition = Vector3.up * heightCurve.Evaluate(tempCurveTime);

        if (Vector3.Distance(player.position, transform.position) < chaseRadius)
        {
            target = player.transform;
        }
        else
        {
            target = cage;
        }
        if(target!= null){
            Vector3 direction = (target.position - transform.position).normalized;
            transform.right += (-new Vector3(direction.x, 0, direction.z)-transform.right) * .05f;
            rb.linearVelocity = Vector3.MoveTowards(rb.linearVelocity, direction*tempTargetSpeed, acceleration * Time.fixedDeltaTime);
        }
    }
}
