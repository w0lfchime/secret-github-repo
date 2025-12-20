using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

public class ItemSelectionUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject selectionPanel;
    [SerializeField] private List<ItemChoiceSlot> choiceSlots = new List<ItemChoiceSlot>();
    private PlayerController pc;
    private AlexHammerCopy hc;
    private CageManager cm;
    
    private Action<Item> onItemSelectedCallback;
    
    void Start()
    {
        pc = GameObject.Find("Player").GetComponent<PlayerController>();
        hc = GameObject.Find("Player").GetComponent<AlexHammerCopy>();
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
        
        // Pause game 
        Time.timeScale = 0f;
    }
    
    private void OnChoiceClicked(int index)
    {
        if (index < 0 || index >= choiceSlots.Count)
            return;
        
        Item selectedItem = choiceSlots[index].GetItem();
        
        if (selectedItem != null)
        {
            if(selectedItem.itemType == 1)
            {
                
            } else if (selectedItem.itemType == 2)
            {
                pc.speed += selectedItem.increaseBy;
            } else if (selectedItem.itemType == 3)
            {
                hc.damage += selectedItem.increaseBy;
            } else if (selectedItem.itemType == 4)
            {
                cm.maxHealth += selectedItem.increaseBy;
            } else if (selectedItem.itemType == 5)
            {
                pc.maxHealth += selectedItem.increaseBy;
                pc.health += selectedItem.increaseBy;
            } else if (selectedItem.itemType == 6)
            {
                pc.regenTime -= selectedItem.increaseBy;
            } else if (selectedItem.itemType == 7)
            {
                hc.additionalExp += selectedItem.increaseBy;
            } else if (selectedItem.itemType == 8)
            {
                
            } else if (selectedItem.itemType == 9)
            {
                
            } else if (selectedItem.itemType == 10)
            {
                
            }
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
        Time.timeScale = 1f;
    }
}
