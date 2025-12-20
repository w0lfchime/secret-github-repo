using Unity.VisualScripting;
using UnityEngine;

public class CageManager : MonoBehaviour
{
    public float cageHealth = 100;
    public PlayerController pc;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(cageHealth <= 0)
        {
            pc.gameOver();
        }
    }

    
}
