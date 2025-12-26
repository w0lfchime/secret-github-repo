using UnityEngine;

public class TransitionPosition : MonoBehaviour
{
    public bool transition;
    public Vector3 newRotation;
    public Vector3 newPosition;
    public float speed = 1;
    public void Transition()
    {
        transition = true;
    }
    void Update()
    {
        if (transition)
        {
            transform.eulerAngles += (newRotation-transform.eulerAngles) * .05f * speed;
            transform.position += (newPosition-transform.position) * .05f * speed;
        }
        else
        {
            transform.eulerAngles += (-transform.eulerAngles) * .05f * speed;
            transform.position += (-transform.position) * .05f * speed;
        }
    }
}
