using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemMoney : ItemBase
{
    protected override void ActiveItem()
    {
        base.ActiveItem();
        GameManager.Instance.GainBounty(500 * Mathf.Pow(Level_Manager.Instance.difficultyScaling, 1.2f + (0.05f * Level_Manager.Instance.waveCleared)));
        Destroy(this.gameObject);
    }

}
