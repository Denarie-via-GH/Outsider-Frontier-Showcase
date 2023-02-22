using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Outsider;

public class UnitSwordModule : UnitClassBase
{
    public event EventHandler<Audio_Manager.AudioArg> OnAttackSFX;
    public event EventHandler<Personal.Utils.SingleStringArgs> OnAttackAnimation;

    private Augment_Manager Blackboard;

    private void Awake()
    {
        unitClass = Outsider.ClassIndex.Cutlass;
    }
    public override void Start()
    {
        base.Start();
        Blackboard = Augment_Manager.Instane;
        BASE.OnReleaseCoordinate += AttackRelease;
        BASE.OnChangeCoordinate += UpdateCoordinate;
        OnAttackAnimation += arrowTarget.Find("Weapon").GetComponent<AnimatorHandler>().TriggerAnimation;
    }
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

    #region ATTACK FUNCTION
    protected override void FireAttack(bool OVERRIDE)
    {
        OnAttackAnimation?.Invoke(this, new Personal.Utils.SingleStringArgs { value = "Slash" });
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
        attackDATA = Projectile_Manager.Instance.ReplicateData(attackDATA, 1);

        attackDATA.damage           += BASE.GetUnitDamage();
        attackDATA.damageMultiplier += GetExtraDamage();
        attackDATA.critMultiplier   += GetAdditionalCritDamage();
        attackDATA.penetrationRate  += GetAdditionalPenerationRate();
        attackDATA.isCrit = RollCrit(BASE.GetUnitCritRate());

        attackDATA.origin = projectileTarget.position;
        attackDATA.aimDirection = DIR;
        Quaternion Target = Quaternion.LookRotation(Vector3.forward, DIR);
        attackDATA.rotation = Target;
        #endregion
        Projectile_Manager.Instance.SpawnProjectileCustomFromPool(attackDATA, BASE.teamIndex);
        OnAttackSFX?.Invoke(this, new Audio_Manager.AudioArg { targetSource = GetComponent<AudioSource>(), targetClip = 8, isOverlap = false });

        #region AUGMENT SWORD WAVE
        int checkSW = Blackboard.Augments.FindIndex(AugmentDef => AugmentDef.augmentIndex == 12);
        if (checkSW != -1)
        {
            TriggerSwordWave(projectileTarget.position, DIR);
        }
        #endregion
        #region AUGMENT BLOOD DRAW
        int checkBD = Blackboard.Augments.FindIndex(AugmentDef => AugmentDef.augmentIndex == 13);
        if (checkBD != -1)
        {
            TriggerBloodDraw(attackDATA.damage, attackDATA.isCrit);
        }
        #endregion

        isAttackCoooldown = true;
    }
    protected override void FireAltAttack(bool OVERRIDE)
    {
        OnAttackAnimation?.Invoke(this, new Personal.Utils.SingleStringArgs { value = "Slash_EX" });
        Vector3 DIR = Vector3.zero;
        if (!OVERRIDE)
        {
            if (focusedTarget == null)
                return;

            DIR = focusedTarget.transform.position - transform.position;
            #region New Data
            Projectile.ProjectileObject attackDATA = ScriptableObject.CreateInstance<Projectile.ProjectileObject>();
            attackDATA = Projectile_Manager.Instance.ReplicateData(attackDATA, 4);

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
            OnAttackSFX?.Invoke(this, new Audio_Manager.AudioArg { targetSource = GetComponent<AudioSource>(), targetClip = 8, isOverlap = false });

            #region AUGMENT SWORD WAVE
            int checkSW = Blackboard.Augments.FindIndex(AugmentDef => AugmentDef.augmentIndex == 12);
            if (checkSW != -1)
            {
                TriggerSwordWave(projectileTarget.position, DIR);
            }
            #endregion
            #region AUGMENT BLOOD DRAW
            int checkBD = Blackboard.Augments.FindIndex(AugmentDef => AugmentDef.augmentIndex == 13);
            if (checkBD != -1)
            {
                TriggerBloodDraw(attackDATA.damage, attackDATA.isCrit);
            }
            #endregion
        }
        else if (OVERRIDE)
        {
            if (overrideTarget == null)
                return;

            DIR = overrideTarget.transform.position - transform.position;
            #region New Data
            Projectile.ProjectileObject attackDATA = ScriptableObject.CreateInstance<Projectile.ProjectileObject>();
            attackDATA = Projectile_Manager.Instance.ReplicateData(attackDATA, 4);

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
            OnAttackSFX?.Invoke(this, new Audio_Manager.AudioArg { targetSource = GetComponent<AudioSource>(), targetClip = 8, isOverlap = false });

            #region AUGMENT SWORD WAVE
            int checkSW = Blackboard.Augments.FindIndex(AugmentDef => AugmentDef.augmentIndex == 12);
            if(checkSW != -1)
            {
                TriggerSwordWave(projectileTarget.position, DIR);
            }
            #endregion
            #region AUGMENT BLOOD DRAW
            int checkBD = Blackboard.Augments.FindIndex(AugmentDef => AugmentDef.augmentIndex == 13);
            if (checkBD != -1)
            {
                TriggerBloodDraw(attackDATA.damage, attackDATA.isCrit);
            }
            #endregion
        }

        isUltimateCooldown = true;
    }
    #endregion

    #region AUGMENT FUNCTION
    protected override float GetRange()
    {
        float finalRange = attackRange;
        foreach (AugmentDef augment in Blackboard.Augments)
        {
            if (augment.augmentIndex == 12)
            {
                finalRange += 1.25f;
            }
        }
        return finalRange;
    }
    private void TriggerSwordWave(Vector3 POS, Vector3 ROT)
    {
        Projectile.ProjectileObject waveDATA = ScriptableObject.CreateInstance<Projectile.ProjectileObject>();
        waveDATA = Projectile_Manager.Instance.ReplicateData(waveDATA, 5);
        waveDATA.origin = POS;
        waveDATA.aimDirection = ROT;
        waveDATA.isCrit = RollCrit(BASE.GetUnitCritRate());
        Quaternion TargetROT = Quaternion.LookRotation(Vector3.forward, ROT);
        waveDATA.rotation = TargetROT;

        //---> update sword wave scaling <---//
        foreach (AugmentDef augment in Blackboard.Augments)
        {
            if (augment.augmentIndex == 12)
            {
                waveDATA.damage += (waveDATA.damage * 0.05f);
                waveDATA.damageMultiplier += (waveDATA.damageMultiplier * 0.03f);     
            }
        }

        Projectile_Manager.Instance.SpawnProjectileCustomFromPool(waveDATA, BASE.teamIndex);
    }
    private void TriggerBloodDraw(float baseDamage, bool criticalStrike)
    {
        float finalRestore = baseDamage * 0.5f;
        foreach (AugmentDef augment in Blackboard.Augments)
        {
            if (augment.augmentIndex == 13)
            {
                finalRestore += (baseDamage * 0.1f);
            }
        }
        if (criticalStrike)
        {
            finalRestore *= 2;
        }

        BASE.RestoreHP(finalRestore);
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

        BASE.OnReleaseCoordinate -= AttackRelease;
        BASE.OnChangeCoordinate -= UpdateCoordinate;
        OnAttackAnimation -= arrowTarget.Find("Weapon").GetComponent<AnimatorHandler>().TriggerAnimation;
    }
    #endregion
}
