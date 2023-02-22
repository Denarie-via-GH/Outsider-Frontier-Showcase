using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Outsider;

public class AreaAttack : BaseProjectile, IProjectile
{
    public override void InitiateProjectile(Projectile.ProjectileObject Data, TeamIndex teamIndex, Vector3 origin, Vector3 direction, bool isCrit)
    {
        this.damage = Data.damage;
        this.damageMultiplier = Data.damageMultiplier;
        this.penetrationRate = Data.penetrationRate;
        this.aimDirection = direction.normalized;
        this.projectileCode = Data.projectileCode;
        this.teamIndex = teamIndex;
        this.speed = Data.speed;
        this.critMultiplier = Data.critMultiplier;
        this.isCrit = isCrit;
        this.isFromPool = Data.fromPool;
        transform.position = Data.origin;
        transform.rotation = Data.rotation;
        Invoke("DelayDestroy", Data.duration);
    }

    public void DelayDestroy()
    {
        GetComponent<AnimatorHandler>().TriggerAnimation(this, new Personal.Utils.SingleStringArgs { value = "Expire" });
    }
    public void RelayDestroy()
    {
        ProjectileExpire();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Unit"))
        {
            UnitBaseModule colliedUnit = collision.gameObject.GetComponent<UnitBaseModule>();
            if (teamIndex == TeamIndex.Ally && colliedUnit.teamIndex == TeamIndex.Enemy)
            {
                UnitBaseModule unitHit = collision.gameObject.GetComponent<UnitBaseModule>();
                unitHit.ReceiveDamage(CalculateDamage(),penetrationRate);
            }
        }
    }
}
