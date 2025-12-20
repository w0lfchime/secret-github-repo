using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    [TextArea(3, 5)]
    public string description;
    public Color itemColor = Color.white; //placeholder

    // Stuff for what stat the item edits
    // 1: Freeze speed
    // 2: Player speed
    // 3: Sledge damage
    // 4: Cage health
    // 5: Player health
    // 6: Player Regen
    // 7: Extra EXP per kill
    // 8: Recover stamina faster
    // 9: Recharge mana faster
    // 10: Increase total stamina total
    public int itemType;
    public float increaseBy;
}
