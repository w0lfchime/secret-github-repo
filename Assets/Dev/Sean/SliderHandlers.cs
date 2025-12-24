using UnityEngine;
using UnityEngine.UI;

public class SliderHandlers : MonoBehaviour
{
    public PlayerController pc;
    public Slider healthBar;
    public Slider manaBar;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pc = GameObject.Find("Player").GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        healthBar.value = pc.health / pc.maxHealth;
        manaBar.value = pc.mana / pc.maxMana;
    }
}
