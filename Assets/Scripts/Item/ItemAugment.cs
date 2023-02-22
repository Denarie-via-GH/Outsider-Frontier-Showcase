using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemAugment : ItemBase
{
    protected override void ActiveItem()
    {
        base.ActiveItem();
        Shop_Manager.Instance.augmentTicketBackup += 1;
        Destroy(this.gameObject);
    }
}
