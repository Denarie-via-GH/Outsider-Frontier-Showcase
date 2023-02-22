using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDestroy : ItemBase
{
    protected override void ActiveItem()
    {
        base.ActiveItem();
        Level_Manager.Instance.ResetAllEnemy();
        Destroy(this.gameObject);
    }
}
