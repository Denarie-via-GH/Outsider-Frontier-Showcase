using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Outsider;

public class TracerBullet : BaseProjectile, IProjectile
{
    private bool isPiercing = false;
    void Update()
    {
        transform.position += (speed * aimDirection * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Unit"))
        {
            UnitBaseModule colliedUnit = collision.gameObject.GetComponent<UnitBaseModule>();
            if (teamIndex == TeamIndex.Ally && colliedUnit.teamIndex == TeamIndex.Enemy)
            {
                UnitBaseModule unitHit = collision.gameObject.GetComponent<UnitBaseModule>();

                #region AUGMENT PIERCING
                Augment_Manager Blackboard = Augment_Manager.Instane;
                foreach (AugmentDef augment in Blackboard.Augments)
                {
                    if(augment.augmentIndex == 15)
                    {
                        GameManager.Instance.GainBounty(unitHit.GetDrop() * 0.05f);
                    }

                    if (augment.augmentIndex == 16)
                    {
                        if (!isPiercing)
                            isPiercing = true;
                        penetrationRate += 0.025f;
                    }
                }
                #endregion

                unitHit.ReceiveDamage(CalculateDamage(), penetrationRate);
                if (!isPiercing)
                    ProjectileExpire();
            }
        }
    }

    #region ENABLE/DISABLE
    public override void OnDisable()
    {
        base.OnDisable();
        isPiercing = false;
    }
    #endregion
}
