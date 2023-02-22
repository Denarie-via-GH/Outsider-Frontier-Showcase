using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Outsider;
using UnityRandom = UnityEngine.Random;

public class UnitOrbiterModule : MonoBehaviour, IUnit
{
    [Header("Component")]
    private ClassIndex unitClass = ClassIndex.Orbiter;
    private UnitBaseModule BASE;
    private UnitManeuverModule Maneuver;
    
    //public event EventHandler OnUnitDead;

    [Header("Internal Variable")]
    public bool OVERRIDE = false;
    [SerializeField] private bool readyExplode = false;
    [SerializeField] private bool isUpgraded = false;
    private Transform playerTransform;
    private void Awake()
    {
        BASE = GetComponent<UnitBaseModule>();
        Maneuver = GetComponent<UnitManeuverModule>();
        playerTransform = UnitController.Instance.transform;
    }
    void Update()
    {
        if(!OVERRIDE && playerTransform != null)
            Maneuver.SetDestination(playerTransform.position);
    }

    #region BEHAVIOR
    public void UpgradeElite()
    {
        if (!isUpgraded)
        {
            isUpgraded = true;
            BASE.InitiateUnit(Level_Manager.Instance.PresetData["OrbiterElite"], transform.position, TeamIndex.Enemy);
            BASE.unitCode = "Orbiter";
            transform.localScale = new Vector3(1.5f, 1.5f, 1);

            GetComponent<AnimatorHandler>().TriggerAnimation(this, new Personal.Utils.SingleStringArgs { value = "elite_idle" });
            GetComponent<AnimatorHandler>().SetBoolParameter(this, new Personal.Utils.BoolAndStringArgs { string_value = "isElite", bool_value = true });
        }
    }
    private void DeadAugment(object sender, EventArgs e)
    {
        if(readyExplode)
        {
            Augment_Manager Blackboard = Augment_Manager.Instane;
            int check = Blackboard.Augments.FindIndex(AugmentDef => AugmentDef.augmentIndex == 23);
            if (check != -1)
            {
                float chance = 0.8f;

                foreach (AugmentDef augment in Blackboard.Augments)
                {
                    if (augment.augmentIndex == 23)
                    {
                        chance -= 0.05f;
                    }
                }

                chance = Mathf.Clamp(chance, 0.2f, 1);
                float randomChance = UnityRandom.Range(0f, 1f); 
                if(randomChance > chance)
                {
                    #region New Data
                    Projectile.ProjectileObject attackDATA = ScriptableObject.CreateInstance<Projectile.ProjectileObject>();
                    attackDATA = Projectile_Manager.Instance.ReplicateData(attackDATA, 11);
                    attackDATA.damage = BASE.GetUnitDamage() * 0.5f;
                    attackDATA.origin = transform.position;
                    #endregion
                    Projectile_Manager.Instance.CreateProjectile(attackDATA, BASE.teamIndex);
                }
            }
        }
    }
    #endregion

    #region COLLISION
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Unit"))
        {
            UnitBaseModule HitCheck = collision.gameObject.GetComponent<UnitBaseModule>();
            if (HitCheck.teamIndex == TeamIndex.Ally)
            {
                HitCheck.ReceiveDamage(BASE.GetUnitDamage(),0);
            }
        }
    }
    #endregion

    public ClassIndex GetUnitClass()
    {
        return unitClass;
    }

    #region ENABLE/DISABLE
    private void OnEnable()
    {
        BASE.OnUnitDead += DeadAugment;

        OVERRIDE = false;
        isUpgraded = false;
        readyExplode = true;
    }
    private void OnDisable()
    {
        BASE.OnUnitDead -= DeadAugment;

        OVERRIDE = false;
        isUpgraded = false;
        readyExplode = false;
    }
    #endregion
}
