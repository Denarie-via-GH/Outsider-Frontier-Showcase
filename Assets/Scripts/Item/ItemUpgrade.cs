using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemUpgrade : ItemBase
{
    protected override void ActiveItem()
    {
        base.ActiveItem();
        GameManager.Instance.upgrade++;
        GameManager.Instance.UpdateUpgradeCount();
        Destroy(this.gameObject);
    }
}
