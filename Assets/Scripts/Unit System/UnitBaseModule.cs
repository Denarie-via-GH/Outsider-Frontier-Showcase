using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Outsider;

public class UnitBaseModule : MonoBehaviour
{
    public event EventHandler<CoordinateEventArgs> OnReleaseCoordinate;
    public event EventHandler<Personal.Utils.SingleBoolArgs> OnChangeCoordinate;
    public event EventHandler<Personal.Utils.BoolAndStringArgs> OnChangeCoordinateAnimation;

    public event EventHandler<Personal.Utils.SingleStringArgs> OnUpgradeUnitAnimation;
    public event EventHandler<Personal.Utils.SingleStringArgs> OnHitAnimation;
    
    public event EventHandler OnUnitDead; 
    
    public class CoordinateEventArgs : EventArgs
    {
        public UnitBaseModule target;
    }
    
    [Header("Condition Variable")]
    public bool isActive = false;
    public bool IsMaster = false;
    public bool isEvolved = false;
    public bool isInFormation = false;
    public bool isCoordinated = false;
    public bool isFromPool = false;

    [Header("Unit Stats")]
    [SerializeField] private float unitMaxHP = 100;
    [SerializeField] private float unitHP = 100;
    [SerializeField] private float unitDMG = 0;
    [SerializeField] private float unitDEF = 0;
    [SerializeField] private float unitCritRate;
    [SerializeField] private float unitSpeed;
    [SerializeField] private float unitRegen;
    [SerializeField] private float unitDrop;
    public string unitCode;
    public TeamIndex teamIndex = TeamIndex.Neutral;
    
    [Header("Iframe Variable")]
    [SerializeField] private bool isIframe = false;
    [SerializeField] private float iFrameTimer = 0;
    [SerializeField] private float IframeDuration = 0.1f;

    #region INITIATION FUNCTION
    public void InitiateUnit(UnitObject DATA, Vector3 position, TeamIndex team)
    {
        GetComponent<AnimatorHandler>().TriggerAnimation(this, new Personal.Utils.SingleStringArgs { value = "idle" });

        //---> set transform <---//
        transform.position = position;
        transform.localScale = new Vector3(1, 1, 1);

        //---> assign unit new datas <---//
        isFromPool = DATA.fromPool;
        unitCode = DATA.unitCode;
        unitMaxHP = DATA.unitMaxHP;
        unitHP = unitMaxHP;
        unitDMG = DATA.unitDMG;
        unitDEF = DATA.unitDEF;
        unitCritRate = DATA.unitCritRate;
        unitSpeed = DATA.unitSpeed;
        unitRegen = DATA.unitRegen;
        unitDrop = DATA.unitDrop;
        IframeDuration = DATA.unitIframeDuration;
        this.teamIndex = team;

        //---> active unit <---//
        isActive = true;
    }
    #endregion

    #region COORDINATE FUNCTION
    public void SetCoordinate(bool value)
    {
        isCoordinated = value;
        OnChangeCoordinate?.Invoke(this, new Personal.Utils.SingleBoolArgs { value = isCoordinated });
        OnChangeCoordinateAnimation?.Invoke(this, new Personal.Utils.BoolAndStringArgs { bool_value = isCoordinated, string_value = "isCoordinate" });
    }
    public void SetCoordinate()
    {
        bool newValue = !isCoordinated;
        SetCoordinate(newValue);
    }
    public void ForceUpdateCoordinateAnimation()
    {
        OnChangeCoordinateAnimation?.Invoke(this, new Personal.Utils.BoolAndStringArgs { bool_value = false, string_value = "isCoordinate" });
    }
    public void ReleaseCoordinate(UnitBaseModule targetUnit)
    {
        OnReleaseCoordinate?.Invoke(this, new CoordinateEventArgs { target = targetUnit});
    }
    #endregion

    #region MISC FUNCTION
    public void SetMaster(bool value)
    {
        IsMaster = value;
    }
    public void EvolveUnit()
    {
        if (!isEvolved)
        {
            Audio_Manager.Instance.PlayGlobal(15);
            GetComponent<AnimatorHandler>().SetBoolParameter(this, new Personal.Utils.BoolAndStringArgs { string_value = "isEvolved", bool_value = true });
            OnUpgradeUnitAnimation?.Invoke(this, new Personal.Utils.SingleStringArgs { value = "evolve_idle" });
            
            isEvolved = true;
            UnitClassBase getClass = GetComponent<UnitClassBase>();
            getClass.RelayUpdateStat();
        }
    }
    #endregion

    #region INTERNAL FUNCTION
    public void FixedUpdate()
    {
        if(GetHP() < unitMaxHP)
            RestoreHP(GetUnitRegeneration() * Time.deltaTime);

        if(iFrameTimer > 0)
        {
            iFrameTimer -= Time.deltaTime;
            if(iFrameTimer <= 0)
            {
                isIframe = false;
            }
        }
    }
    public void RestoreHP(float value)
    {
        if (unitHP < unitMaxHP)
        {
            unitHP += value;
            if(unitHP > unitMaxHP)
            {
                unitHP = unitMaxHP;
            }
        }
    }
    public void ReceiveDamage(float damage, float penetration)
    {
        if (!isIframe && unitHP > 0 && damage > 0)
        {
            float damageReduction = 100 / (100 + (GetUnitDefense() * (1 - penetration)));
            float receivedDamage = damage * damageReduction;
            unitHP -= receivedDamage;
            OnHitAnimation?.Invoke(this, new Personal.Utils.SingleStringArgs { value = "flash" });

            if (unitHP <= 0)
                UnitDead();
            if(!isIframe && IframeDuration > 0)
                TriggerIframe();

        }
        else if(unitHP <= 0)
        {
            UnitDead();
        }

    }
    #endregion

    #region GET/SET FUNCTION
    public float GetHP()
    {
        return unitHP;
    }
    public float GetMaxHP()
    {
        return unitMaxHP;
    }
    public float GetDrop()
    {
        return unitDrop;
    }
    public float GetUnitRegeneration()
    {
        float finalRegeneration = unitRegen;
        Augment_Manager Blackboard = Augment_Manager.Instane;
        foreach (AugmentDef augment in Blackboard.Augments)
        {
            //---> detect augment 3: increase health regeneration 0.5 per stack <---//
            if (augment.augmentIndex == 3 && augment.augmentTarget == teamIndex)
            {
                finalRegeneration += 0.5f;
            }
        }
        return finalRegeneration;
    }
    public float GetUnitDamage()
    {
        float finalUnitDamage = unitDMG;
        Augment_Manager Blackboard = Augment_Manager.Instane;
        foreach(AugmentDef augment in Blackboard.Augments)
        {
            //---> detect augment 0: increase 5 damage per stack <---//
            if (augment.augmentIndex == 0 && augment.augmentTarget == teamIndex || augment.augmentIndex == 29 && augment.augmentTarget == teamIndex)
            {
                finalUnitDamage += 5;
            }
        }

        return finalUnitDamage;
    }
    public float GetUnitCritRate()
    {
        float finalCritRate = unitCritRate;
        Augment_Manager Blackboard = Augment_Manager.Instane;
        foreach (AugmentDef augment in Blackboard.Augments)
        {
            if (augment.augmentIndex == 2 && augment.augmentTarget == teamIndex)
            {
                finalCritRate += 0.1f;
            }
        }

        finalCritRate = Mathf.Clamp(finalCritRate, 0f, 1f);
        return finalCritRate;
    }
    public float GetUnitIframe()
    {
        float finalIframe = IframeDuration;
        Augment_Manager Blackboard = Augment_Manager.Instane;
        foreach (AugmentDef augment in Blackboard.Augments)
        {
            //---> detect augment 1: increase extra damage by 2.5 per stack (specific class) <---//
            if (augment.augmentIndex == 9 && augment.augmentTarget == teamIndex)
            {
                finalIframe += 0.1f;
            }
        }

        return finalIframe;
    }
    public float GetUnitDefense()
    {
        float finalDefense = unitDEF;
        Augment_Manager Blackboard = Augment_Manager.Instane;
        foreach (AugmentDef augment in Blackboard.Augments)
        {
            //---> detect augment 5: increase 3.5 defense per stack <---//
            if (augment.augmentIndex == 5 && augment.augmentTarget == teamIndex || augment.augmentIndex == 30 && augment.augmentTarget == teamIndex)
            {
                finalDefense += 3f;
            }
        }
        return finalDefense;
    }
    public float GetUnitSpeed()
    {
        float finalSpeed = unitSpeed;
        Augment_Manager Blackboard = Augment_Manager.Instane;
        foreach (AugmentDef augment in Blackboard.Augments)
        {
            //---> detect augment 4: increase speed 2.5 per stack <---//
            if (augment.augmentIndex == 4 && augment.augmentTarget == teamIndex)
            {
                finalSpeed += 2.5f;
            }
        }
        return finalSpeed;
    }
    #endregion

    #region IFRAME FUNCTION
    private void TriggerIframe()
    {
        iFrameTimer = GetUnitIframe();
        isIframe = true;
    }
    #endregion

    #region DEAD FUNCTION
    public void UnitDead()
    {
        OnUnitDead?.Invoke(this, EventArgs.Empty);
        
        GameManager.Instance.VerifyUnitDead(this, unitDrop);
        if (isFromPool)
        {
            RecycleUnit();
        }
        else if (!isFromPool)
        {
            DestroyUnit();
        }
    }
    public void DestroyUnit()
    {
        isActive = false;
        
        Destroy(this.gameObject);
    }
    public void RecycleUnit()
    {
        Level_Manager.Instance.UnitRecycleDictionary[unitCode].Enqueue(this);
        isActive = false;

        this.gameObject.SetActive(false);
    }
    public void DelayRecycle()
    {
        RecycleUnit();
    }
    #endregion

    #region ENABLE/DISABLE
    private void OnEnable()
    {
        if (teamIndex == TeamIndex.Ally)
        {
            OnChangeCoordinateAnimation += GetComponent<AnimatorHandler>().SetBoolParameter;
            OnUpgradeUnitAnimation      += GetComponent<AnimatorHandler>().TriggerAnimation;
        }
        OnHitAnimation += GetComponent<AnimatorHandler>().TriggerAnimation;
    }
    private void OnDestroy()
    {
        if (teamIndex == TeamIndex.Ally)
        {
            OnChangeCoordinateAnimation -= GetComponent<AnimatorHandler>().SetBoolParameter;
            OnUpgradeUnitAnimation      -= GetComponent<AnimatorHandler>().TriggerAnimation;
        }
        OnHitAnimation -= GetComponent<AnimatorHandler>().TriggerAnimation;
    }
    #endregion
}
