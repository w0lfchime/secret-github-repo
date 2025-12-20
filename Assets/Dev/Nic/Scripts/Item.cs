using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    [TextArea(3, 5)]
    public string description;
    public Color itemColor = Color.white; //placeholder
}
