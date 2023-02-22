using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Outsider;
using UnityRandom = UnityEngine.Random;

public class UnitSentinelModule : MonoBehaviour, IUnit
{
    private enum UnitState { Idle, Overriding, Action }
    [SerializeField] private UnitState CurrentState = UnitState.Overriding;
    private ClassIndex unitClass = ClassIndex.Sentinel;
    public event EventHandler<Particle_Manager.ParticleArgs> OnOverride;
    private UnitBaseModule BASE;
    protected Transform Pivot;
    protected Transform projectileTarget;
    private Augment_Manager Blackboard;

    [Header("Internal Variable")]
    [SerializeField] private float overrideTimer = 0;
    [SerializeField] private float overrideCooldown = 30;
    [SerializeField] private bool isOverrideCooldown = false;
    [SerializeField] private int attackCounter = 5;
    [SerializeField] private float attackTimer = 0;
    [SerializeField] private float attackCooldown = 3.5f;
    [SerializeField] private bool isAttackCooldown = false;
    [SerializeField] private Queue<UnitOrbiterModule> controlledOrbiters = new Queue<UnitOrbiterModule>();

    public void Start()
    {
        BASE = GetComponent<UnitBaseModule>();
        Blackboard = Augment_Manager.Instane;
        Pivot = transform.Find("Pivot").gameObject.transform;
        projectileTarget = Pivot.Find("PSpawn").gameObject.transform;
        unitClass = ClassIndex.Sentinel;
    }

    private void Update()
    {
        switch (CurrentState)
        {
            case (UnitState.Action):
                if(attackCounter > 0)
                {
                    if (!isAttackCooldown)
                    {
                        FireAttack();
                    }
                }
                else if(attackCounter <= 0)
                {
                    ResetBehavior();
                }
                break;
            case (UnitState.Overriding):
                if (!isOverrideCooldown)
                {
                    SeizeControl();
                }
                break;
            case (UnitState.Idle):
                break;
        }
    }

    private void FixedUpdate()
    {
        if(overrideTimer > 0 || isOverrideCooldown)
        {
            overrideTimer -= Time.deltaTime;
            if(overrideTimer <= 0)
            {
                isOverrideCooldown = false;
            }
        }

        if (attackTimer > 0 || isAttackCooldown)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0)
            {
                isAttackCooldown = false;
            }
        }
    }

    #region BEHAVIOR FUNCTION
    private void ResetBehavior()
    {
        attackCounter = 0;
        attackTimer = 0;
        isAttackCooldown = false;
        overrideTimer = 0;
        isOverrideCooldown = false;

        CurrentState = UnitState.Idle;
        StartCoroutine("StartEngine");
    }
    private IEnumerator StartEngine()
    {
        yield return new WaitForSecondsRealtime(5f);
        CurrentState = UnitState.Overriding;
    }
    private void FireAttack()
    {
        Vector3 DIR = UnitController.Instance.transform.position - transform.position;
        #region New Data
        Projectile.ProjectileObject attackDATA = ScriptableObject.CreateInstance<Projectile.ProjectileObject>();
        attackDATA = Projectile_Manager.Instance.ReplicateData(attackDATA, 6);

        attackDATA.damage += BASE.GetUnitDamage();
        attackDATA.isCrit = RollCrit(BASE.GetUnitCritRate());
        attackDATA.origin = projectileTarget.position;
        attackDATA.aimDirection = DIR;
        Quaternion Target = Quaternion.LookRotation(Vector3.forward, DIR);
        attackDATA.rotation = Target;
        #endregion
        Projectile_Manager.Instance.CreateProjectile(attackDATA, BASE.teamIndex);
        Audio_Manager.Instance.PlayClip(GetComponent<AudioSource>(), 4);

        attackCounter--;
        isAttackCooldown = true;
        attackTimer = attackCooldown;
    }
    private void SeizeControl()
    {
        #region OVERRIDE ORBITER
        controlledOrbiters.Clear();
        //---> spawn new additional units <---//
        for (int i = 0; i < 15 + Mathf.RoundToInt(Level_Manager.Instance.GetSpawnRate()); i++)
        {
            UnitBaseModule newUnit = Level_Manager.Instance.SpawnUnitFromPool("Orbiter", transform.position);
            UnitOrbiterModule getOrbiter = newUnit.GetComponent<UnitOrbiterModule>();
            controlledOrbiters.Enqueue(getOrbiter);
            getOrbiter.OVERRIDE = true;
        }
        //---> get reference to orbiter units and override them <---//
        Collider2D[] collider2DArray = Physics2D.OverlapCircleAll(transform.position, 50);
        if (collider2DArray != null || collider2DArray.Length > 0)
        {
            for (int i = 0; i < collider2DArray.Length; i++)
            {
                IUnit Unit = collider2DArray[i].GetComponent(typeof(IUnit)) as IUnit;
                if (Unit != null && Unit.GetUnitClass() == ClassIndex.Orbiter)
                {
                    UnitOrbiterModule newOrbiter = collider2DArray[i].GetComponent<UnitOrbiterModule>();
                    controlledOrbiters.Enqueue(newOrbiter);
                    newOrbiter.OVERRIDE = true;
                }
            }
        }
        List<Vector2> NewFormation = UnitMaster_Manager.Instance.RequestCircle(0, transform.position, 20, 25, controlledOrbiters.Count);
        int counter = 0;
        foreach (UnitOrbiterModule unit in controlledOrbiters)
        {
            unit.GetComponent<UnitManeuverModule>().SetDestination(NewFormation[counter]);
            counter++;
        }
        #endregion

        attackCounter = 5;
        CurrentState = UnitState.Action;
        overrideTimer = overrideCooldown;
        isOverrideCooldown = true;

        Audio_Manager.Instance.PlayClip(GetComponent<AudioSource>(), 5);
        OnOverride?.Invoke(this, new Particle_Manager.ParticleArgs { index = 6, position = transform.position, parent = null });
    }
    #endregion

    #region GET/SET FUNCTION
    protected virtual bool RollCrit(float chance)
    {
        bool Roll = false;
        float Check = UnityRandom.Range(0, 1);
        if (Check >= (1 - chance))
        {
            Roll = true;
        }
        return Roll;
    }
    public ClassIndex GetUnitClass()
    {
        return unitClass;
    }
    #endregion
   
    #region ENABLE/DISABLE
    private void OnEnable()
    {
        controlledOrbiters.Clear();
        attackCounter = 5;
        attackTimer = 0;
        isAttackCooldown = false;
        overrideTimer = 0;
        isOverrideCooldown = false;

        CurrentState = UnitState.Overriding;

        OnOverride += Particle_Manager.Instance.CreateParticle;
    }
    private void OnDisable()
    {
        //---> check for augment <---//
        if (Blackboard != null)
        {
            int check = Blackboard.Augments.FindIndex(AugmentDef => AugmentDef.augmentIndex == 25);
            if (check != -1)
            {
                //---> ready spawn unit <---//
                int spawnUnit = 20;
                foreach (AugmentDef augment in Blackboard.Augments)
                {
                    if (augment.augmentIndex == 25)
                    {
                        spawnUnit += 15;
                    }
                }

                //---> spawn unit upon dead <---//
                for (int i = 0; i < spawnUnit + Level_Manager.Instance.GetSpawnRate(); i++)
                {
                    float RandX = UnityRandom.Range(transform.position.x - 50, transform.position.x + 50);
                    float RandY = UnityRandom.Range(transform.position.y - 50, transform.position.y - 50);
                    float chance = UnityRandom.Range(0f, 1f);
                    UnitBaseModule newUnit = Level_Manager.Instance.SpawnUnitFromPool("Orbiter", new Vector3(RandX, RandY, 0));
                    if(chance > 0.5f)
                    {
                        UnitOrbiterModule getOrbiter = newUnit.GetComponent<UnitOrbiterModule>();
                        getOrbiter.UpgradeElite();
                    }
                }
            }
        }

        //---> release all controlled unit <---//
        int count = controlledOrbiters.Count;
        for (int i = 0; i < count; i++)
        {
            UnitOrbiterModule releaseUnit = controlledOrbiters.Dequeue();
            releaseUnit.OVERRIDE = false;
        }

        controlledOrbiters.Clear();
        attackCounter = 0;
        attackTimer = 0;
        isAttackCooldown = false;
        overrideTimer = 0;
        isOverrideCooldown = false;

        CurrentState = UnitState.Idle;

        OnOverride -= Particle_Manager.Instance.CreateParticle;

    }
    #endregion
}
