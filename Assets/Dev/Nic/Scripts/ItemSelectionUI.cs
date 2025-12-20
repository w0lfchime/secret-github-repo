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
    
    private Action<Item> onItemSelectedCallback;
    
    void Start()
    {
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
