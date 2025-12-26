using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering;

public class CamRotate : MonoBehaviour
{
    public float amount;
    private float time;
    public float moveWithMouseMult = 1;
    void FixedUpdate()
    {
        Vector2 mousePos = (Vector2)Input.mousePosition - new Vector2(Screen.width/2, Screen.height/2);
        mousePos = mousePos/1000 * moveWithMouseMult;
        time+=Time.deltaTime;
        Vector3 targetAngles = new Vector3(-mousePos.y, mousePos.x, Mathf.Sin(time) * amount);
        transform.localEulerAngles = targetAngles;
    }
}
