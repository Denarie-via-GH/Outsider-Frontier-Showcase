using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityRandom = UnityEngine.Random;
using Outsider;
public class UnitMortarModule : UnitClassBase
{
    [SerializeField] private float targetOffsetDistance = 10;
    [SerializeField] private float targetOffsetDistanceFocused = 5f;
    [SerializeField] private GameObject locationGetter;

    private Augment_Manager Blackboard;
    [SerializeField] private bool isRepairCooldown = false;
    [SerializeField] private float repairTimer = 0;

    private void Awake()
    {
        unitClass = Outsider.ClassIndex.Hyperion;
    }
    public override void Start()
    {
        base.Start();
        Blackboard = Augment_Manager.Instane;
        BASE.OnReleaseCoordinate += AttackRelease;
        BASE.OnChangeCoordinate += UpdateCoordinate;
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

        if (isRepairCooldown)
        {
            if (repairTimer > 0)
                repairTimer -= Time.deltaTime;
            else if (repairTimer <= 0)
                isRepairCooldown = false;
        }

        int check = Blackboard.Augments.FindIndex(AugmentDef => AugmentDef.augmentIndex == 22);
        if (check != -1)
        {
            if (BASE.GetHP() < (BASE.GetMaxHP() * 0.25f) && !isRepairCooldown)
            {
                RepairDamage();
            }
        }
    }
    #region ATTACK
    protected override void FireAttack(bool OVERRIDE)
    {
        if (!OVERRIDE)
        {
            if (focusedTarget == null)
                return;
            Instantiate(locationGetter, focusedTarget.transform.position, Quaternion.identity, transform.root).GetComponent<BombardAfterDestroy>().Parent = this;
        }
        else if (OVERRIDE)
        {
            if (overrideTarget == null)
                return;
            Instantiate(locationGetter, overrideTarget.transform.position, Quaternion.identity, transform.root).GetComponent<BombardAfterDestroy>().Parent = this;
        }

        isAttackCoooldown = true;
    }
    protected override void FireAltAttack(bool OVERRIDE)
    {
        if (!OVERRIDE)
        {
            Instantiate(locationGetter, focusedTarget.transform.position, Quaternion.identity, transform.root).GetComponent<BombardAfterDestroy>().Parent = this;
            for (int i = 0; i < 3; i++)
            {
                if (focusedTarget == null)
                    return;

                float RandomX = UnityRandom.Range(focusedTarget.transform.position.x - targetOffsetDistance, focusedTarget.transform.position.x + targetOffsetDistance);
                float RandomY = UnityRandom.Range(focusedTarget.transform.position.y - targetOffsetDistance, focusedTarget.transform.position.y + targetOffsetDistance);
                Instantiate(locationGetter, new Vector3(RandomX, RandomY), Quaternion.identity, transform.root).GetComponent<BombardAfterDestroy>().Parent = this;
            }
        }
        else if (OVERRIDE)
        {
            Instantiate(locationGetter, overrideTarget.transform.position, Quaternion.identity, transform.root).GetComponent<BombardAfterDestroy>().Parent = this;
            for (int i = 0; i < 5; i++)
            {
                if (overrideTarget == null)
                    return;

                float RandomX = UnityRandom.Range(overrideTarget.transform.position.x - targetOffsetDistanceFocused, overrideTarget.transform.position.x + targetOffsetDistanceFocused);
                float RandomY = UnityRandom.Range(overrideTarget.transform.position.y - targetOffsetDistanceFocused, overrideTarget.transform.position.y + targetOffsetDistanceFocused);
                Instantiate(locationGetter, new Vector3(RandomX, RandomY), Quaternion.identity, transform.root).GetComponent<BombardAfterDestroy>().Parent = this;
            }
        }

        isUltimateCooldown = true;
    }
    public void SpawnMortarAtPosition(Vector3 position)
    {
        #region New Data
        Projectile.ProjectileObject attackDATA = ScriptableObject.CreateInstance<Projectile.ProjectileObject>();
        attackDATA = Projectile_Manager.Instance.ReplicateData(attackDATA, 2);

        attackDATA.damage += BASE.GetUnitDamage();
        attackDATA.damageMultiplier += GetExtraDamage();
        attackDATA.penetrationRate += GetAdditionalPenerationRate();

        attackDATA.isCrit = false;
        attackDATA.origin = position;
        attackDATA.aimDirection = Vector3.zero;
        attackDATA.rotation = Quaternion.identity;
        #endregion

        int additionalStrike = 0;
        int check = Blackboard.Augments.FindIndex(AugmentDef => AugmentDef.augmentIndex == 21);
        if (check != -1)
        {
            foreach (AugmentDef augment in Blackboard.Augments)
            {
                if (augment.augmentIndex == 21)
                {
                    attackDATA.duration += 0.25f;
                    additionalStrike += 1;
                }
            }
        }
        
        //---> spawn base arterlliy strike <---//
        Projectile_Manager.Instance.SpawnProjectileCustomFromPool(attackDATA, BASE.teamIndex);
        Vector3 referencePOS = attackDATA.origin;
        for (int i = 0; i < additionalStrike; i++)
        {
            attackDATA.origin = referencePOS;
            float RandomX = UnityRandom.Range(attackDATA.origin.x - targetOffsetDistance / 3, attackDATA.origin.x + targetOffsetDistance / 3);
            float RandomY = UnityRandom.Range(attackDATA.origin.y - targetOffsetDistance / 3, attackDATA.origin.y + targetOffsetDistance / 3);
            attackDATA.origin = new Vector3(RandomX, RandomY, 0);
            Projectile_Manager.Instance.SpawnProjectileCustomFromPool(attackDATA, BASE.teamIndex);
        }
    }
    #endregion

    #region SPECIAL AUGMENT
    private void RepairDamage()
    {
        float restorePercent = 0.5f;
        foreach (AugmentDef augment in Blackboard.Augments)
        {
            if (augment.augmentIndex == 22)
            {
                restorePercent += 0.05f;
            }
        }
        BASE.RestoreHP(BASE.GetMaxHP() * restorePercent);

        repairTimer = GetRepairCooldown();
        isRepairCooldown = true;
    }
    private float GetRepairCooldown()
    {
        float cooldown = 180f;
        foreach (AugmentDef augment in Blackboard.Augments)
        {
            if (augment.augmentIndex == 22)
            {
                cooldown -= (cooldown * 0.1f);
            }
        }
        return cooldown;
    }
    #endregion
    protected override void UpdateTarget()
    {
        if (enemyInRadiusList.Count > 0 && enemyInRadiusList != null)
        {
            UnitBaseModule RandomTarget = null;
            RandomTarget = enemyInRadiusList[UnityRandom.Range(0, enemyInRadiusList.Count - 1)];
            if (RandomTarget != null)
            {
                focusedTarget = RandomTarget;
            }
        }
        else
        {
            focusedTarget = null;
        }
    }

    #region ENABLE/DISABE
    private void OnDisable()
    {
        BASE.OnReleaseCoordinate -= AttackRelease;
        BASE.OnChangeCoordinate -= UpdateCoordinate;
    }
    #endregion
}
