using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HotbarSlot : MonoBehaviour
{
    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI stackCountText;
    [SerializeField] private GameObject stackCountObject;
    
    private Item currentItem;
    private int stackCount = 1;
    
    void Start()
    {
        ClearSlot();
    }
    
    public void SetItem(Item item)
    {
        currentItem = item;
        stackCount = 1;
        
        if (item != null)
        {
            itemImage.gameObject.SetActive(true);
            itemImage.color = item.itemColor;
            
            //If no sprite
            if (item.icon != null)
            {
                itemImage.sprite = item.icon;
            }
            
            UpdateStackDisplay();
        }
    }
    
    public void IncrementStack()
    {
        stackCount++;
        UpdateStackDisplay();
    }
    
    private void UpdateStackDisplay()
    {
        if (stackCount > 1)
        {
            stackCountObject.SetActive(true);
            stackCountText.text = "+" + stackCount;
        }
        else
        {
            stackCountObject.SetActive(false);
        }
    }
    
    public bool HasItem()
    {
        return currentItem != null;
    }
    
    public Item GetItem()
    {
        return currentItem;
    }
    
    public void ClearSlot()
    {
        currentItem = null;
        stackCount = 0;
        itemImage.gameObject.SetActive(false);
        stackCountObject.SetActive(false);
    }
}
