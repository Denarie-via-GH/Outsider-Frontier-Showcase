using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Outsider;

public class ItemHeal : ItemBase
{
    protected override void ActiveItem()
    {
        base.ActiveItem();
        
        GameObject[] allyArrays = GameObject.FindGameObjectsWithTag("Unit");
        foreach (GameObject enemy in allyArrays)
        {
            UnitBaseModule baseUnit = enemy.GetComponent<UnitBaseModule>();
            if (baseUnit.teamIndex == TeamIndex.Ally)
            {
                baseUnit.RestoreHP(baseUnit.GetMaxHP() * 0.5f);
            }
        }

        Destroy(this.gameObject);
    }
}
