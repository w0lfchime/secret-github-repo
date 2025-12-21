using System;
using UnityEngine;

public class CamRotate : MonoBehaviour
{
    public float amount;
    private float time;
    void FixedUpdate()
    {
        time+=Time.deltaTime;
        transform.eulerAngles = new Vector3(0, 0, Mathf.Sin(time) * amount);
    }
}
