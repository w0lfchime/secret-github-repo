using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

public class ItemChoiceSlot : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;
    
    private Item currentItem;
    private Action onClickCallback;
    
    public void SetItem(Item item)
    {
        currentItem = item;
        
        if (item != null)
        {
            itemImage.color = item.itemColor;
            
            //Sets icon
            if (item.icon != null)
            {
                itemImage.sprite = item.icon;
            }
            
            //Set text
            if (itemNameText != null)
                itemNameText.text = item.itemName;
            
            if (itemDescriptionText != null)
                itemDescriptionText.text = item.description;
        }
    }
    
    public Item GetItem()
    {
        return currentItem;
    }
    
    public void SetClickCallback(Action callback)
    {
        onClickCallback = callback;
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        onClickCallback?.Invoke();
    }
}
