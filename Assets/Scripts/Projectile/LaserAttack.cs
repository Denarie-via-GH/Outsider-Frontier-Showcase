using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Outsider;

public class LaserAttack : BaseProjectile, IProjectile
{
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Unit"))
        {
            UnitBaseModule colliedUnit = collision.gameObject.GetComponent<UnitBaseModule>();
            if (teamIndex == TeamIndex.Ally && colliedUnit.teamIndex == TeamIndex.Enemy)
            {
                UnitBaseModule unitHit = collision.gameObject.GetComponent<UnitBaseModule>();
                unitHit.ReceiveDamage(CalculateDamage(), penetrationRate);
            }
        }
    }
}
