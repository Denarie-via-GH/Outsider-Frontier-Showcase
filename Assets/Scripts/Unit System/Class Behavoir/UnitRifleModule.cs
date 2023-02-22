using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Personal.Utils;
using Outsider;
public class UnitRifleModule : UnitClassBase
{
    public event EventHandler<Audio_Manager.AudioArg> OnAttackSFX;
    private Augment_Manager Blackboard;

    private void Awake()
    {
        unitClass = Outsider.ClassIndex.Tracer;
    }
    public override void Start()
    {
        base.Start();
        Blackboard = Augment_Manager.Instane;
        BASE.OnReleaseCoordinate    += AttackRelease;
        BASE.OnChangeCoordinate     += UpdateCoordinate;
    }

    #region UPDATE FUNCTION
    private void Update()
    {
        switch (CurrentState)
        {
            case (AttackState.Action):
                FireAttack(isTargetOverride);
                if (BASE.isEvolved && !isUltimateCooldown)
                    FireAltAttack(isTargetOverride);

                if (!BASE.isEvolved)
                {
                    if (isAttackCoooldown)
                        CurrentState = AttackState.Standby;
                }
                else if (BASE.isEvolved)
                {
                    if (isAttackCoooldown && isUltimateCooldown)
                        CurrentState = AttackState.Standby;
                }

                break;
            case (AttackState.Standby):
                if (!isTargetOverride)
                {
                    if (focusedTarget != null)
                    {
                        if (InRange(isTargetOverride) && !BASE.isCoordinated)
                        {
                            if (!isAttackCoooldown)
                                CurrentState = AttackState.Action;
                        }
                    }
                }
                else if (isTargetOverride)
                {
                    if (overrideTarget == null)
                    {
                        ExitCoordinateMode();
                    }
                    if (InRange(isTargetOverride))
                    {
                        if (!isAttackCoooldown)
                            CurrentState = AttackState.Action;
                    }
                }
                break;
            case (AttackState.Locked):
                break;
        }
    }
    private void FixedUpdate()
    {
        UpdateTimer();
        UpdateDetection();
        UpdateTarget();

        if (!isTargetOverride)
        {
            if (focusedTarget != null)
            {
                Vector3 DIR = focusedTarget.transform.position - transform.position;
                Quaternion Target = Quaternion.LookRotation(Vector3.forward, DIR);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Target, aimSpeed);
            }
        }
        else if (isTargetOverride)
        {
            Vector3 DIR = overrideTarget.transform.position - transform.position;
            Quaternion Target = Quaternion.LookRotation(Vector3.forward, DIR);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Target, aimSpeed);
        }
    }
    #endregion

    #region ATTACK FUNCTION
    protected override void FireAttack(bool OVERRIDE)
    {
        Vector3 DIR = Vector3.zero;
        if (!OVERRIDE)
        {
            if (focusedTarget == null)
                return;
            DIR = focusedTarget.transform.position - transform.position;
        }
        else if (OVERRIDE)
        {
            if (overrideTarget == null)
                return;
            DIR = overrideTarget.transform.position - transform.position;
        }

        #region New Data
        Projectile.ProjectileObject attackDATA = ScriptableObject.CreateInstance<Projectile.ProjectileObject>();
        attackDATA = Projectile_Manager.Instance.ReplicateData(attackDATA, 0);

        attackDATA.damage += BASE.GetUnitDamage();
        attackDATA.damageMultiplier += GetExtraDamage();
        attackDATA.critMultiplier += GetAdditionalCritDamage();
        attackDATA.penetrationRate += GetAdditionalPenerationRate();
        
        attackDATA.isCrit = RollCrit(BASE.GetUnitCritRate());
        attackDATA.origin = projectileTarget.position;
        attackDATA.aimDirection = DIR;
        Quaternion Target = Quaternion.LookRotation(Vector3.forward, DIR);
        attackDATA.rotation = Target;
        #endregion
        Projectile_Manager.Instance.SpawnProjectileCustomFromPool(attackDATA, BASE.teamIndex);
        OnAttackSFX?.Invoke(this, new Audio_Manager.AudioArg { targetSource = GetComponent<AudioSource>(), targetClip = 6, isOverlap = false});
        isAttackCoooldown = true;
    }
    protected override void FireAltAttack(bool OVERRIDE)
    {
        if (!OVERRIDE)
        {
            if (focusedTarget == null)
                return;

            int ticket = 6;
            #region AUGMENT RAPID
            foreach (AugmentDef augment in Blackboard.Augments)
            {
                if (augment.augmentIndex == 14)
                {
                    ticket += 1;
                }
            }
            #endregion
            Vector3 DIR = focusedTarget.transform.position - transform.position;
            float angle = Utility.GetAngleFromVectorFloat(DIR);
            float halfangle = angle / 2;
            for (int i = 0; i < ticket; i++)
            {
                float newAngle = ((angle / ticket) * i) + halfangle; //direction + (i * (360f / unitCount));
                Vector3 newDir = Utility.ReturnDegreeRotationToVector(new Vector2(1, 0), newAngle);
                Vector3 offset = transform.position + (newDir * 3.5f);

                #region New Data
                Projectile.ProjectileObject attackDATA = ScriptableObject.CreateInstance<Projectile.ProjectileObject>();
                attackDATA = Projectile_Manager.Instance.ReplicateData(attackDATA, 3);

                attackDATA.damage += BASE.GetUnitDamage();
                attackDATA.damageMultiplier += GetExtraDamage();
                attackDATA.critMultiplier += GetAdditionalCritDamage();
                attackDATA.penetrationRate += GetAdditionalPenerationRate();

                attackDATA.isCrit = RollCrit(BASE.GetUnitCritRate());
                attackDATA.origin = offset;
                attackDATA.aimDirection = newDir;
                Quaternion Target = Quaternion.LookRotation(Vector3.forward, newDir);
                attackDATA.rotation = Target;
                #endregion
                Projectile_Manager.Instance.SpawnProjectileCustomFromPool(attackDATA, BASE.teamIndex);
                OnAttackSFX?.Invoke(this, new Audio_Manager.AudioArg { targetSource = GetComponent<AudioSource>(), targetClip = 7, isOverlap = false});
            }
        }
        else if (OVERRIDE)
        {
            if (overrideTarget == null)
                return;

            int ticket = 6;
            #region AUGMENT RAPID
            foreach (AugmentDef augment in Blackboard.Augments)
            {
                if (augment.augmentIndex == 14)
                {
                    ticket += 1;
                }
            }
            #endregion
            Vector3 DIR = overrideTarget.transform.position - transform.position;
            float angle = Utility.GetAngleFromVectorFloat(DIR);
            float halfangle = angle / 2;
            for (int i = 0; i < ticket; i++)
            {
                float newAngle = ((angle / ticket) * i) + halfangle; //direction + (i * (360f / unitCount));
                Vector3 newDir = Utility.ReturnDegreeRotationToVector(new Vector2(1, 0), newAngle);
                Vector3 offset = transform.position + (newDir * 3.5f);

                #region New Data
                Projectile.ProjectileObject attackDATA = ScriptableObject.CreateInstance<Projectile.ProjectileObject>();
                attackDATA = Projectile_Manager.Instance.ReplicateData(attackDATA, 3);

                attackDATA.damage += BASE.GetUnitDamage();
                attackDATA.damageMultiplier += GetExtraDamage();
                attackDATA.critMultiplier += GetAdditionalCritDamage();
                attackDATA.penetrationRate += GetAdditionalPenerationRate();
                
                attackDATA.isCrit = RollCrit(BASE.GetUnitCritRate());
                attackDATA.origin = offset;
                attackDATA.aimDirection = newDir;
                Quaternion Target = Quaternion.LookRotation(Vector3.forward, newDir);
                attackDATA.rotation = Target;
                #endregion
                Projectile_Manager.Instance.SpawnProjectileCustomFromPool(attackDATA, BASE.teamIndex);
                OnAttackSFX?.Invoke(this, new Audio_Manager.AudioArg { targetSource = GetComponent<AudioSource>(), targetClip = 7, isOverlap = false});
            }
        }
        isUltimateCooldown = true;
    }
    #endregion

    protected override void UpdateTarget()
    {
        if (enemyInRadiusList.Count > 0 && enemyInRadiusList != null)
        {
            UnitBaseModule closestEnemyUnit = enemyInRadiusList[0];
            for (int i = 1; i < enemyInRadiusList.Count; i++)
            {
                if (enemyInRadiusList[i] != null)
                {

                    float newDistance = Vector2.Distance(enemyInRadiusList[i].transform.position, transform.position);
                    if (newDistance < Vector2.Distance(closestEnemyUnit.transform.position, transform.position))
                    {
                        closestEnemyUnit = enemyInRadiusList[i];
                    }
                }
            }
            if (closestEnemyUnit != null)
            {
                focusedTarget = closestEnemyUnit;
            }
        }
        else
        {
            focusedTarget = null;
        }
    }

    #region ENABLE/DISABLE
    private void OnEnable()
    {
        OnAttackSFX += Audio_Manager.Instance.SignalSFX;
    }
    private void OnDisable()
    {
        OnAttackSFX -= Audio_Manager.Instance.SignalSFX;

        BASE.OnReleaseCoordinate    -= AttackRelease;
        BASE.OnChangeCoordinate     -= UpdateCoordinate;
    }
    #endregion
}
