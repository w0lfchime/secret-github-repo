using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;
using System.Net;

public class ItemSelectionUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject selectionPanel;
    [SerializeField] private List<ItemChoiceSlot> choiceSlots = new List<ItemChoiceSlot>();
    public PlayerController pc;
    public AlexHammerCopy hc;
    public CageManager cm;
    public GameObject particleStuff;
    
    private Action<Item> onItemSelectedCallback;

    public SlowTime slow;
    
    void Start()
    {
        // pc = GameObject.Find("Player").GetComponent<PlayerController>();
        // hc = GameObject.Find("Player").GetComponent<AlexHammerCopy>();
        cm = GameObject.Find("Cage").GetComponent<CageManager>();
        // Hide selection panel at start
        selectionPanel.SetActive(false);
        
        // Setup click listeners for each choice slot
        for (int i = 0; i < choiceSlots.Count; i++)
        {
            int index = i; // Capture for closure
            choiceSlots[i].SetClickCallback(() => OnChoiceClicked(index));
        }
    }

    public void ShowItemSelection(List<Item> items, Action<Item> callback)
    {
        Debug.Log("Doin some stuff");
        onItemSelectedCallback = callback;
        
        // Show panel
        selectionPanel.SetActive(true);
        
        // Setup each choice slot with an item
        for (int i = 0; i < choiceSlots.Count; i++)
        {
            if (i < items.Count)
            {
                choiceSlots[i].SetItem(items[i]);
                choiceSlots[i].gameObject.SetActive(true);
            }
            else
            {
                choiceSlots[i].gameObject.SetActive(false);
            }
        }

        slow.slowTime = true;
    }
    
    private void OnChoiceClicked(int index)
    {
        if (index < 0 || index >= choiceSlots.Count)
            return;
        
        Item selectedItem = choiceSlots[index].GetItem();
        
        if (selectedItem != null)
        {
            // if(selectedItem.itemType == 1)
            // {
            //     var shape = particleStuff.GetComponent<ParticleSystem>().shape;
            //     shape.angle += selectedItem.increaseBy;
            // } else if (selectedItem.itemType == 2)
            // {
            //     hc.speedMulti += selectedItem.increaseBy;
            // } else if (selectedItem.itemType == 3)
            // {
            //     hc.damage += selectedItem.increaseBy;
            // } else if (selectedItem.itemType == 4)
            // {
            //     cm.maxHealth += selectedItem.increaseBy;
            // } else if (selectedItem.itemType == 5)
            // {
            //     pc.maxHealth += selectedItem.increaseBy;
            //     pc.health += selectedItem.increaseBy;
            // } else if (selectedItem.itemType == 6)
            // {
            //     pc.regenTime -= selectedItem.increaseBy;
            //     if(pc.regenTime < 0.1)
            //     {
            //         pc.regenTime = 0.1f;
            //     }
            // } else if (selectedItem.itemType == 7)
            // {
            //     hc.additionalExp += selectedItem.increaseBy;
            // } else if (selectedItem.itemType == 8)
            // {
            //     pc.maxMana += selectedItem.increaseBy;
            //     pc.mana += selectedItem.increaseBy;
            // } else if (selectedItem.itemType == 9)
            // {
            //     pc.manaRegenTime -= selectedItem.increaseBy;
            // } else if (selectedItem.itemType == 10)
            // {
                
            // } else if(selectedItem.itemType == 11)
            // {
            //     hc.limit += selectedItem.increaseBy;
            // } else if(selectedItem.itemType == 12)
            // {
            //     hc.spinMultiplier += selectedItem.increaseBy;
            // } else if(selectedItem.itemType == 13)
            // {
            //     hc.changeHammerScale(selectedItem.increaseBy);
            // } else if(selectedItem.itemType == 14)
            // {
            //     hc.lifeSteal += selectedItem.increaseBy;
            // }

            selectedItem.pc = pc;
            selectedItem.hc = hc;
            selectedItem.cm = cm;
            selectedItem.addEffect();

            // Invoke callback
            onItemSelectedCallback?.Invoke(selectedItem);

            // Hide selection panel
            HideSelection();
        }
    }
    
    private void HideSelection()
    {
        selectionPanel.SetActive(false);
        
        // Unpause game
        slow.slowTime = false;
    }
}
