using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Outsider;

public class SeekHoming : MonoBehaviour
{
    private Transform TargetLocked;
    private Projectile.SeekingProjectile projectile;
    public void Initiate(Projectile.SeekingProjectile Parent)
    {
        TargetLocked = null;
        projectile = Parent;
    }

    private void Update()
    {
        if (TargetLocked != null)
        {
            if (TargetLocked.gameObject.activeInHierarchy == false)
            {
                projectile.ReceiveTarget(null);
                TargetLocked = null;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Unit") && TargetLocked == null)
        {
            UnitBaseModule colliedUnit = collision.gameObject.GetComponent<UnitBaseModule>();
            if (projectile.teamIndex == TeamIndex.Ally && colliedUnit.teamIndex == TeamIndex.Enemy)
            {
                TargetLocked = colliedUnit.transform;
                projectile.ReceiveTarget(collision.transform);
            }
        }
    }

}
