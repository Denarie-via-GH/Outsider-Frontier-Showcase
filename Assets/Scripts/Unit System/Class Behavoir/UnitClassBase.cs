using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityRandom = UnityEngine.Random;
using Outsider;
public class UnitClassBase : MonoBehaviour, IUnit
{
    protected enum AttackState { Action, Standby, Locked }
    protected AttackState CurrentState = AttackState.Standby;
    public ClassIndex unitClass = ClassIndex.None;

    [SerializeField] protected List<UnitBaseModule> enemyInRadiusList = new List<UnitBaseModule>();
    [SerializeField] protected UnitBaseModule focusedTarget = null;
    protected Transform Pivot;
    protected Transform arrowTarget;
    protected Transform projectileTarget;
    protected UnitBaseModule BASE;

    [SerializeField] protected UnitBaseModule overrideTarget = null;
    [SerializeField] protected float overrideAttackRange = 0;
    public bool isTargetOverride = false;

    [SerializeField] protected float critRate = 0.1f;
    [SerializeField] protected float attackRange = 10;
    [SerializeField] protected float evolvedAttackRange = 0;
    [SerializeField] protected float aimSpeed = 10;
    [SerializeField] protected float attackTimer = 0;
    [SerializeField] protected float maxAttackDelay = 0.25f;

    [SerializeField] protected float altAttackTimer = 0;
    [SerializeField] protected float maxAltAttackDelay = 5f;

    [SerializeField] protected bool isAttackCoooldown = false;
    [SerializeField] protected bool isUltimateCooldown = true;

    public virtual void Start()
    {
        BASE = GetComponent<UnitBaseModule>();
        Pivot = transform.Find("Pivot").gameObject.transform;
        arrowTarget = Pivot.Find("Arrow").gameObject.transform;
        projectileTarget = Pivot.Find("PSpawn").gameObject.transform;
    }

    public virtual void OnTimerFinish()
    {
        attackTimer = GetAttackCooldown();
        isAttackCoooldown = false;
    }
    public virtual void OnAltTimerFinish()
    {
        altAttackTimer = GetUltimateCooldown();
        isUltimateCooldown = false;
    }

    protected virtual void FireAttack(bool OVERRIDE)
    {
    }
    protected virtual void FireAltAttack(bool OVERRIDE)
    {
    }

    #region COORDINATE FUNCTION
    protected virtual void AttackRelease(object sender, UnitBaseModule.CoordinateEventArgs e)
    {
        if (!isTargetOverride)
        {
            isTargetOverride = true;
            overrideTarget = e.target;
        }

        CurrentState = AttackState.Standby;
    }

    public void UpdateCoordinate(object sender, Personal.Utils.SingleBoolArgs e)
    {
        if (e.value == true)
        {
            if (!isTargetOverride)
                CurrentState = AttackState.Locked;
        }
        else if (e.value == false)
        {
            ExitCoordinateMode();
        }
    }
    protected virtual void ExitCoordinateMode()
    {
        isTargetOverride = false;
        overrideTarget = null;

        BASE.isCoordinated = false;
        BASE.ForceUpdateCoordinateAnimation();
        if (CurrentState == AttackState.Locked)
            CurrentState = AttackState.Standby;
    }
    #endregion

    #region UPDATE FUNCTION
    public void RelayUpdateStat()
    {
        if (BASE != null)
        {
            if (BASE.isEvolved) 
            {
                string target = unitClass.ToString() + "Elite";
                BASE.InitiateUnit(Level_Manager.Instance.PresetData[target], transform.position, TeamIndex.Ally);
                BASE.unitCode = unitClass.ToString();
            }
            else
                BASE.InitiateUnit(Level_Manager.Instance.PresetData[unitClass.ToString()], transform.position, TeamIndex.Ally);
        }
    }
    protected virtual void UpdateTimer()
    {
        if (isAttackCoooldown)
        {
            if(attackTimer > 0)
                attackTimer -= Time.deltaTime;
            else if (attackTimer <= 0)
                OnTimerFinish();
        }
        
        if (isUltimateCooldown) 
        {
            if(altAttackTimer > 0)
                altAttackTimer -= Time.deltaTime;
            else if (altAttackTimer <= 0)
                OnAltTimerFinish(); 
        }
    }
    protected virtual void UpdateTarget()
    {

    }
    protected virtual void UpdateDetection()
    {
        if (overrideTarget != null)
            if (!overrideTarget.gameObject.activeInHierarchy)
            {
                isTargetOverride = false;
                overrideTarget = null;
            }

        enemyInRadiusList.RemoveAll(delegate (UnitBaseModule o) { return o == null; });
        enemyInRadiusList.RemoveAll(delegate (UnitBaseModule o) { return o.gameObject.activeInHierarchy == false; });
    }
    protected virtual bool InRange(bool OVERRIDE)
    {
        bool isInRange = false;
        float DistanceThreshold = 0;

        if (!OVERRIDE)
        {
            if (focusedTarget == null)
                return false;
            DistanceThreshold = Vector3.Distance(focusedTarget.transform.position, transform.position);

            if(DistanceThreshold <= GetRange())
                isInRange = true;
        }
        else if (OVERRIDE)
        {
            if (overrideTarget == null)
                return false;
            DistanceThreshold = Vector3.Distance(overrideTarget.transform.position, transform.position);

            if (DistanceThreshold <= GetRange())
                isInRange = true;
        }

        return isInRange;
    }
    #endregion

    #region CALCULATION FUNCTION
    protected virtual bool RollCrit(float chance)
    {
        bool Roll = false;
        float Check = UnityRandom.Range(0f, 1f);
        if (Check >= (1f - chance))
        {
            Roll = true;
        }
        return Roll;
    }
    protected virtual float GetRange()
    {
        return attackRange;
    }
    public float GetExtraDamage()
    {
        float finalExtraDamage = 0;
        Augment_Manager Blackboard = Augment_Manager.Instane;
        foreach (AugmentDef augment in Blackboard.Augments)
        {
            if (augment.augmentIndex == 1 && augment.augmentTarget == BASE.teamIndex)
            {
                if (augment.specificClass == unitClass || augment.specificClass == ClassIndex.None)
                {
                    finalExtraDamage += 0.1f;
                }
            }
        }
        return finalExtraDamage;
    }
    public float GetAdditionalPenerationRate()
    {
        float finalPeneration = 0;
        Augment_Manager Blackboard = Augment_Manager.Instane;
        foreach (AugmentDef augment in Blackboard.Augments)
        {
            if (augment.augmentIndex == 10 && augment.augmentTarget == BASE.teamIndex)
            {
                if (augment.specificClass == unitClass || augment.specificClass == ClassIndex.None)
                {
                    finalPeneration += 0.1f;
                }
            }
        }
        return finalPeneration;
    }
    public float GetUltimateTimer()
    {
        return altAttackTimer;
    }
    public float GetUltimateCooldown()
    {
        float finalCooldown = maxAltAttackDelay;
        Augment_Manager Blackboard = Augment_Manager.Instane;
        foreach (AugmentDef augment in Blackboard.Augments)
        {
            if (augment.augmentIndex == 6 && augment.augmentTarget == BASE.teamIndex)
            {
                if (augment.specificClass == unitClass || augment.specificClass == ClassIndex.None)
                {
                    finalCooldown -= (maxAltAttackDelay * 0.05f);
                }
            }
        }
        finalCooldown = Mathf.Clamp(finalCooldown, 0.1f, maxAltAttackDelay);
        return finalCooldown;
    }
    public float GetAttackCooldown()
    {
        float finalAttackCooldown = maxAttackDelay;
        Augment_Manager Blackboard = Augment_Manager.Instane;
        foreach (AugmentDef augment in Blackboard.Augments)
        {
            if (augment.augmentIndex == 8 && augment.augmentTarget == BASE.teamIndex)
            {
                if (augment.specificClass == unitClass || augment.specificClass == ClassIndex.None)
                {
                    finalAttackCooldown -= (maxAttackDelay * 0.1f);
                }
            }
        }
        finalAttackCooldown = Mathf.Clamp(finalAttackCooldown, 0.1f, maxAttackDelay);
        return finalAttackCooldown;
    }
    public float GetAdditionalCritDamage()
    {
        float finalCritDamage = 0;
        Augment_Manager Blackboard = Augment_Manager.Instane;
        foreach (AugmentDef augment in Blackboard.Augments)
        {
            if (augment.augmentIndex == 7 && augment.augmentTarget == BASE.teamIndex)
            {
                if (augment.specificClass == unitClass || augment.specificClass == ClassIndex.None)
                {
                    finalCritDamage += 0.2f;
                }
            }
        }
        return finalCritDamage;
    }
    public ClassIndex GetUnitClass()
    {
        return unitClass;
    }
    #endregion

    #region COLLISION FUNCTION
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Unit"))
        {
            UnitBaseModule detectedUnit = collision.gameObject.GetComponent<UnitBaseModule>();
            if (BASE.teamIndex == TeamIndex.Ally && detectedUnit.teamIndex == TeamIndex.Enemy)
            {
                int check = enemyInRadiusList.FindIndex(x => x == detectedUnit);
                if (check == -1)
                {
                    enemyInRadiusList.Add(detectedUnit);
                }
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Unit"))
        {
            UnitBaseModule detectedUnit = collision.gameObject.GetComponent<UnitBaseModule>();
            if (BASE.teamIndex == TeamIndex.Ally && detectedUnit.teamIndex == TeamIndex.Enemy)
            {
                int check = enemyInRadiusList.FindIndex(x => x == detectedUnit);
                if (check != -1)
                {
                    enemyInRadiusList.RemoveAt(check);
                }
            }

        }
    }
    #endregion
}
