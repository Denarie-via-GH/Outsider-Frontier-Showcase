using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Outsider;

public class CutlassProjectile : BaseProjectile, IProjectile
{
    void Update()
    {
        transform.position += (speed * aimDirection * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Unit"))
        {
            UnitBaseModule colliedUnit = collision.gameObject.GetComponent<UnitBaseModule>();
            if (teamIndex == TeamIndex.Ally && colliedUnit.teamIndex == TeamIndex.Enemy || teamIndex == TeamIndex.Enemy && colliedUnit.teamIndex == TeamIndex.Ally)
            {
                UnitBaseModule unitHit = collision.gameObject.GetComponent<UnitBaseModule>();
                if (unitHit.GetHP() > unitHit.GetMaxHP() * 0.5f)
                {
                    TriggerExtraSlash(unitHit.transform.position);
                }
                unitHit.ReceiveDamage(CalculateDamage(), penetrationRate);
                ProjectileExpire();
            }
        }
    }

    private void TriggerExtraSlash(Vector3 target)
    {
        Augment_Manager Blackboard = Augment_Manager.Instane;
        foreach (AugmentDef augment in Blackboard.Augments)
        {
            if (augment.augmentIndex == 11)
            {
                Projectile.ProjectileObject attackDATA = ScriptableObject.CreateInstance<Projectile.ProjectileObject>();
                attackDATA = Projectile_Manager.Instance.ReplicateData(attackDATA, 9);
                attackDATA.damage += CalculateDamage() * 0.3f;
                attackDATA.origin = target;
                float estimateX = Random.Range(target.x - 2.5f, target.x + 2.5f);
                float estimateY = Random.Range(target.y - 2.5f, target.y + 2.5f);
                Projectile_Manager.Instance.SpawnProjectileCustomFromPool(attackDATA, this.teamIndex);
                Particle_Manager.Instance.CreateParticle(7, new Vector3(estimateX,estimateY,0));
                Debug.Log("Phase");
                Audio_Manager.Instance.PlayAt(9, new Vector3(estimateX, estimateY, 0), 1);
            }
        }
    }
}
