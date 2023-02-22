using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Outsider;
using Projectile;
using System;

public class ArterillyStrike : DelayProjectile, IProjectile
{
    public event EventHandler<Audio_Manager.AudioArg> OnStrike;
    public override void InitiateProjectile(ProjectileObject Data, TeamIndex teamIndex, Vector3 origin, Vector3 direction, bool isCrit)
    {
        base.InitiateProjectile(Data, teamIndex, origin, direction, isCrit);
        OnStrike?.Invoke(this, new Audio_Manager.AudioArg { targetSource = GetComponent<AudioSource>(), targetClip = 13, isOverlap = false});
    }
    public override void RelayDestroy()
    {
        Augment_Manager Blackboard = Augment_Manager.Instane;
        int check = Blackboard.Augments.FindIndex(AugmentDef => AugmentDef.augmentIndex == 20);
        if (check != -1)
        {
            SpawnLandmine();
        }
        base.RelayDestroy();
    }
    private void SpawnLandmine()
    {
        #region New Data
        Projectile.ProjectileObject attackDATA = ScriptableObject.CreateInstance<Projectile.ProjectileObject>();
        attackDATA = Projectile_Manager.Instance.ReplicateData(attackDATA, 10);
        attackDATA.damage += this.damage * 0.75f;
        attackDATA.origin = transform.position;

        Augment_Manager Blackboard = Augment_Manager.Instane;
        foreach (AugmentDef augment in Blackboard.Augments)
        {
            if (augment.augmentIndex == 20)
            {
                attackDATA.damageMultiplier += (attackDATA.damageMultiplier * 0.25f);
            }
        }
        #endregion
        Projectile_Manager.Instance.SpawnProjectileCustomFromPool(attackDATA, teamIndex);
    }

    public override void OnEnable()
    {
        base.OnEnable();
        OnStrike += Audio_Manager.Instance.SignalSFX;
    }
    public override void OnDisable()
    {
        base.OnDisable();
        OnStrike -= Audio_Manager.Instance.SignalSFX;
    }
}
