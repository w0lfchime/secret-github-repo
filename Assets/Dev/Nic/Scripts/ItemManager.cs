using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ItemManager : MonoBehaviour
{
    [Header("Item Pool")]
    [SerializeField] private List<Item> availableItems = new List<Item>();
    
    [Header("UI References")]
    [SerializeField] private ItemSelectionUI itemSelectionUI;
    [SerializeField] private Transform hotbarContainer;
    [SerializeField] private GameObject hotbarSlotPrefab;
    
    [Header("Settings")]
    [SerializeField] private KeyCode triggerKey = KeyCode.Space;
    [SerializeField] private int itemChoiceCount = 3;
    [SerializeField] private int maxHotbarSlots = 6;
    
    private List<HotbarSlot> hotbarSlots = new List<HotbarSlot>();
    
    void Start()
    {
        // Initialize hotbar slots
        for (int i = 0; i < maxHotbarSlots; i++)
        {
            GameObject slotObj = Instantiate(hotbarSlotPrefab, hotbarContainer);
            HotbarSlot slot = slotObj.GetComponent<HotbarSlot>();
            hotbarSlots.Add(slot);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(triggerKey))
        {
            //TriggerItemSelection();
        }
    }
    
    public void TriggerItemSelection()
    {
        if (availableItems.Count == 0)
        {
            Debug.LogWarning("No items available in the pool!");
            return;
        }
        
        // Get random items from the pool
        List<Item> randomItems = GetRandomItems(itemChoiceCount);
        
        // Show selection UI
        itemSelectionUI.ShowItemSelection(randomItems, OnItemSelected);
    }
    
    private List<Item> GetRandomItems(int count)
    {
        // Ensure we don't try to get more items than available
        count = Mathf.Min(count, availableItems.Count);
        
        // Get random items without duplicates
        List<Item> shuffled = availableItems.OrderBy(x => Random.value).ToList();
        return shuffled.Take(count).ToList();
    }
    
    private void OnItemSelected(Item selectedItem)
    {
        // Check if item already exists in hotbar
        HotbarSlot existingSlot = hotbarSlots.Find(slot => slot.HasItem() && slot.GetItem() == selectedItem);
        
        if (existingSlot != null)
        {
            // Item exists, increment stack
            existingSlot.IncrementStack();
        }
        else
        {
            // Find first empty slot and add item
            HotbarSlot emptySlot = hotbarSlots.Find(slot => !slot.HasItem());
            
            if (emptySlot != null)
            {
                emptySlot.SetItem(selectedItem);
            }
            else
            {
                Debug.LogWarning("Hotbar full");
            }
        }
    }
}
