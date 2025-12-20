using UnityEngine;

public class HammerSwing : MonoBehaviour
{
    public Transform hammer;
    public PlayerController pc;
    public float damage = 5f;
    public float swingRange = 50;
    public float swingDuration = 1;
    public float chargeLimit = 3;
    public float chargeMultiplier = 1;
    public LayerMask groundLayer;
    private bool clicking, clickDown;
    public bool rightSide;
    public AnimationCurve swingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    float swingTime;
    float chargeTime;

    void Update()
    {
        if (Input.GetMouseButton(0) && swingTime>=1)
        {
            clicking = true;
        }else clicking = false;
        if (Input.GetMouseButtonDown(0))
        {
            chargeTime = 0;
        }
        if (Input.GetMouseButtonUp(0) && swingTime>=1){
            rightSide = !rightSide;
            swingTime=0;
        }
    }
    void FixedUpdate()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            Vector3 directionToMouse = (hit.point - transform.position).normalized;

            transform.right = new Vector3(directionToMouse.x, 0, directionToMouse.z);
            
        }
        if (clicking&&swingTime >= 1f)
        {
            rightSide = !rightSide;
            swingTime=0;
        }
        if (swingTime < 1f){
            swingTime += Time.deltaTime / swingDuration;

            float c = swingCurve.Evaluate(Mathf.Clamp01(swingTime));
            float targetRotation = (rightSide ? -1 : 1) * (swingRange+chargeTime) * (c-.5f)*2;

            hammer.localRotation = Quaternion.Euler(0f, targetRotation, 0f);
        }
        

    }

    public void OnTriggerEnter(Collider col) {
        Debug.Log("Collision entered");

        if(col.gameObject.tag == "Enemy")
        {
            Hittable hit = col.gameObject.GetComponent<Hittable>();

            hit.health -= damage;

            if(hit.health <= 0)
            {
                pc.exp += hit.expAmount;
                Destroy(col.gameObject);

                if(pc.exp >= pc.expToNextLevelUp)
                {
                    pc.levelUp();
                }
            }
        }
    }

}
