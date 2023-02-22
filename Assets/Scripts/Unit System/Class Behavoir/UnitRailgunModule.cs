using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Outsider;

public class UnitRailgunModule : UnitClassBase
{
    [Header("Raycasting")]
    public LayerMask Mask;
    public RaycastHit2D Hit;
    private float adaptiveDistance = 0;
    [SerializeField] private Projectile.ProjectileObject beamData;
    [SerializeField] private GameObject trail;

    public event EventHandler<Audio_Manager.AudioArg> OnAttackSFX;

    private Augment_Manager Blackboard;
    public event EventHandler<TargetDestroyArgs> OnAttack;
    public class TargetDestroyArgs : EventArgs
    {
        public GameObject target;
        public float delay;
    }

    private void Awake()
    {
        unitClass = ClassIndex.Raygunner;
    }
    public override void Start()
    {
        base.Start();
        Blackboard = Augment_Manager.Instane;
        BASE.OnReleaseCoordinate += AttackRelease;
        BASE.OnChangeCoordinate += UpdateCoordinate;
    }
    #region UPDATE
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

    #region ATTACK
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
        Hit = Physics2D.Raycast(projectileTarget.position, DIR, Mathf.Infinity, Mask);
        if (Hit)
        {
            UnitBaseModule hitTarget = Hit.collider.gameObject.GetComponent<UnitBaseModule>();
            if (BASE.teamIndex == TeamIndex.Ally && hitTarget.teamIndex == TeamIndex.Enemy)
            {
                //---> check if have adaptive cooldown augment <---//
                int checkAdaptive = Blackboard.Augments.FindIndex(AugmentDef => AugmentDef.augmentIndex == 18);
                if(checkAdaptive != -1)
                {
                    adaptiveDistance = Mathf.Clamp(Math.Abs((hitTarget.transform.position - transform.position).magnitude), 0, GetRange());
                }
                //---> check if have execute augment <---//
                int checkEx = Blackboard.Augments.FindIndex(AugmentDef => AugmentDef.augmentIndex == 17);
                if (checkEx != -1)
                {
                    float executeThreshold = 0.15f;
                    foreach (AugmentDef augment in Blackboard.Augments)
                    {
                        if (augment.augmentIndex == 17)
                        {
                            executeThreshold += 0.015f;
                        }
                    }

                    executeThreshold = Mathf.Clamp(executeThreshold, 0, 0.5f);
                    if (hitTarget.GetHP() < hitTarget.GetMaxHP() * executeThreshold)
                    {
                        OnAttackSFX?.Invoke(this, new Audio_Manager.AudioArg { targetSource = GetComponent<AudioSource>(), targetClip = 10, isOverlap = false });

                        hitTarget.UnitDead();

                        LineRenderer SnipeRail = Instantiate(trail, projectileTarget.position, Quaternion.identity).GetComponent<LineRenderer>();
                        SnipeRail.SetPosition(0, SnipeRail.gameObject.transform.position);
                        SnipeRail.SetPosition(1, hitTarget.gameObject.transform.position);

                        OnAttack += DestroyTrail;
                        OnAttack?.Invoke(this, new TargetDestroyArgs { target = SnipeRail.gameObject, delay = 0.1f });
                    }
                    else
                    {
                        OnAttackSFX?.Invoke(this, new Audio_Manager.AudioArg { targetSource = GetComponent<AudioSource>(), targetClip = 10, isOverlap = false });

                        bool isCrit = RollCrit(BASE.GetUnitCritRate());
                        float getDamage = CalculateSnipeDamage((10 + BASE.GetUnitDamage()) * (1 + executeThreshold), 10f + GetExtraDamage(), 2.5f + GetAdditionalCritDamage(), isCrit);
                        float getPiercing = 0.25f + CalculateAdaptivePiercing(isCrit) + GetAdditionalPenerationRate();
                        hitTarget.ReceiveDamage(getDamage, getPiercing);
                        
                        LineRenderer SnipeRail = Instantiate(trail, projectileTarget.position, Quaternion.identity).GetComponent<LineRenderer>();
                        SnipeRail.SetPosition(0, SnipeRail.gameObject.transform.position);
                        SnipeRail.SetPosition(1, hitTarget.gameObject.transform.position);

                        OnAttack += DestroyTrail;
                        OnAttack?.Invoke(this, new TargetDestroyArgs { target = SnipeRail.gameObject, delay = 0.1f });
                    }
                }
                else
                {
                    OnAttackSFX?.Invoke(this, new Audio_Manager.AudioArg { targetSource = GetComponent<AudioSource>(), targetClip = 10, isOverlap = false });

                    bool isCrit = RollCrit(BASE.GetUnitCritRate());
                    float getDamage = CalculateSnipeDamage(10 + BASE.GetUnitDamage(), 10f + GetExtraDamage(), 2.5f + GetAdditionalCritDamage(), isCrit);
                    float getPiercing = 0.25f + CalculateAdaptivePiercing(isCrit) + GetAdditionalPenerationRate();
                    hitTarget.ReceiveDamage(getDamage, getPiercing);
                    
                    LineRenderer SnipeRail = Instantiate(trail, projectileTarget.position, Quaternion.identity).GetComponent<LineRenderer>();
                    SnipeRail.SetPosition(0, SnipeRail.gameObject.transform.position);
                    SnipeRail.SetPosition(1, hitTarget.gameObject.transform.position);

                    OnAttack += DestroyTrail;
                    OnAttack?.Invoke(this, new TargetDestroyArgs { target = SnipeRail.gameObject, delay = 0.1f });
                }
            }
        }

        isAttackCoooldown = true;
    }
    protected override void FireAltAttack(bool OVERRIDE)
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
        attackDATA = Projectile_Manager.Instance.ReplicateData(attackDATA, 7);

        attackDATA.damage += BASE.GetUnitDamage();
        attackDATA.damageMultiplier += GetExtraDamage();
        attackDATA.penetrationRate += GetAdditionalPenerationRate();
        attackDATA.critMultiplier += GetAdditionalCritDamage();
        
        attackDATA.origin = projectileTarget.position;
        attackDATA.rotation = Quaternion.Euler(0, 0, 0);
        attackDATA.isCrit = RollCrit(BASE.GetUnitCritRate());
        attackDATA.penetrationRate = CalculateAdaptivePiercing(attackDATA.isCrit);
        attackDATA.origin = projectileTarget.position;
        attackDATA.aimDirection = DIR;
        Quaternion Target = Quaternion.LookRotation(Vector3.forward, DIR);
        attackDATA.rotation = Target;
        #endregion

        Projectile_Manager.Instance.SpawnProjectileCustomFromPool(attackDATA, BASE.teamIndex);
        OnAttackSFX?.Invoke(this, new Audio_Manager.AudioArg { targetSource = GetComponent<AudioSource>(), targetClip = 11, isOverlap = false });
        
        isUltimateCooldown = true;
    }
    private void DestroyTrail(object sender, TargetDestroyArgs e)
    {
        Destroy(e.target, e.delay);
        OnAttack -= DestroyTrail;
    }
    #endregion
    public override void OnTimerFinish()
    {
        if (adaptiveDistance != 0)
        {
            float percentage = adaptiveDistance / attackRange;
            float cooldownCap = 0.1f;

            foreach (AugmentDef augment in Blackboard.Augments)
            {
                if (augment.augmentIndex == 18)
                {
                    cooldownCap += 0.025f;
                }
            }
            attackTimer = Mathf.Clamp(Mathf.Abs((GetAttackCooldown() * percentage) - cooldownCap), 0.1f, maxAttackDelay);
            adaptiveDistance = 0;
        }
        else
        {
            attackTimer = GetAttackCooldown();
        }

        isAttackCoooldown = false;
    }
    protected override void UpdateTarget()
    {
        if (enemyInRadiusList.Count > 0 && enemyInRadiusList != null)
        {
            UnitBaseModule furthestEnemyUnit = enemyInRadiusList[0];
            for (int i = 1; i < enemyInRadiusList.Count; i++)
            {
                if (enemyInRadiusList[i] != null)
                {
                    float newDistance = Vector2.Distance(enemyInRadiusList[i].transform.position, transform.position);
                    if (newDistance > Vector2.Distance(furthestEnemyUnit.transform.position, transform.position))
                    {
                        furthestEnemyUnit = enemyInRadiusList[i];
                    }
                }
            }
            if (furthestEnemyUnit != null)
            {
                focusedTarget = furthestEnemyUnit;
            }
        }
        else
        {
            focusedTarget = null;
        }
    }
    protected float CalculateSnipeDamage(float damage, float multiplier, float critMultiplier, bool isCrit)
    {
        float finalDamage = (damage * multiplier);
        if (isCrit)
        {
            finalDamage *= critMultiplier;
        }
        return finalDamage;
    }
    protected float CalculateAdaptivePiercing(bool isCrit)
    {
        float finalPiercing = 0;

        if (isCrit)
        {
            foreach (AugmentDef augment in Blackboard.Augments)
            {
                if (augment.augmentIndex == 19)
                {
                    finalPiercing += 0.25f;
                }
            }
        }

        return finalPiercing;
    }
    private void OnEnable()
    {
        OnAttackSFX += Audio_Manager.Instance.SignalSFX;
    }
    private void OnDisable()
    {
        OnAttackSFX -= Audio_Manager.Instance.SignalSFX;
        BASE.OnReleaseCoordinate -= AttackRelease;
        BASE.OnChangeCoordinate -= UpdateCoordinate;
    }
}
