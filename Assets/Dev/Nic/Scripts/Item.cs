using System;
using System.Collections.Generic;
using System.Reflection;
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
    // 8: Increase max mana
    // 9: Recharge mana faster
    // 10: Hammer size
    // 11: Sledge speed limit
    // 12: Sledge spin multiplier
    // 13: hammer size
    // 14: life steal
    public int itemType;
    public float increaseBy;
    public AlexHammerCopy hc;
    public PlayerController pc;
    public CageManager cm;
    public List<float> values;
    public List<String> toChange;

    public void addEffect()
    {
        Type item = this.GetType();

        for(int i = 0; i < values.Count; i++)
        {
            var method = this.GetType().GetMethod(toChange[i]);
            var args = new object[] {values[i]};

            method.Invoke(this, args);
        }
    }

    public void coneSize(float changeBy)
    {
        var shape = GameObject.Find("FramePivot").transform.GetChild(0).GetComponent<ParticleSystem>().shape;
        shape.angle += changeBy;
    }

    public void playerSpeed(float changeBy)
    {
        hc.speedMulti += changeBy;
    }

    public void damage(float changeBy)
    {
        hc.damage += changeBy;
    }

    public void cageHealth(float changeBy)
    {
        cm.maxHealth += changeBy;
    }

    public void playerHealth(float changeBy)
    {
        pc.maxHealth += changeBy;
        pc.health += changeBy;
    }

    public void playerRegen(float changeBy)
    {
        pc.regenTime += changeBy;

        if(pc.regenTime < 0.1)
        {
            pc.regenTime = 0.1f;
        }
    }

    public void extraExp(float changeBy)
    {
        hc.additionalExp += changeBy;
    }

    public void maxMana(float changeBy)
    {
        pc.maxMana += changeBy;
        pc.mana += changeBy;
    }

    public void hammerSize(float changeBy)
    {
        hc.changeHammerScale(changeBy);
    }

    public void hammerSpeedLimit(float changeBy)
    {
        hc.limit += changeBy;
    }

    public void spinMulti(float changeBy)
    {
        hc.spinMultiplier += changeBy;
    }

    public void lifeSteal(float changeBy)
    {
        hc.lifeSteal += changeBy;
    }

    public void knockBack(float changeBy)
    {
        hc.knockback += changeBy;
    }
}
