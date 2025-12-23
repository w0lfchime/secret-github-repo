using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    public float moveAmount;
    private float time;
    private Vector3 direction;

    void Start()
    {
        direction = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
    }
    void Update()
    {
        transform.LookAt(transform.position+Camera.main.transform.forward);
        time+=Time.deltaTime;
        transform.position+=direction * (moveAmount/time) * Time.deltaTime;
    }
}
