using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Outsider;

public class NormalProjectile : BaseProjectile, IProjectile
{
    void Update()
    {
        transform.position += (speed * aimDirection * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Unit"))
        {
            UnitBaseModule colliedUnit = collision.gameObject.GetComponent<UnitBaseModule>();
            if (teamIndex == TeamIndex.Ally && colliedUnit.teamIndex == TeamIndex.Enemy || teamIndex == TeamIndex.Enemy && colliedUnit.teamIndex == TeamIndex.Ally)
            {
                UnitBaseModule unitHit = collision.gameObject.GetComponent<UnitBaseModule>();
                unitHit.ReceiveDamage(CalculateDamage(), penetrationRate);
                ProjectileExpire();
            }
        }
    }
}
