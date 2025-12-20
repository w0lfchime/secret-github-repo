using UnityEngine;

public class CageHealthBar : MonoBehaviour
{
    public CageManager cm;
    public Transform barTransform;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        barTransform.localScale = new Vector3(cm.cageHealth / cm.maxHealth, barTransform.localScale.y, barTransform.localScale.z);
    }
}
