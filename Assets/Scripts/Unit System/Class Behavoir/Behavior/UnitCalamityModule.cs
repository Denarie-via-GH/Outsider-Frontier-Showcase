using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Outsider;
using UnityRandom = UnityEngine.Random;

public class UnitCalamityModule : MonoBehaviour, IUnit
{
    private enum UnitState { Idle, Overriding, Action }
    [SerializeField] private UnitState CurrentState = UnitState.Overriding;
    private ClassIndex unitClass = ClassIndex.Calamity;
    public event EventHandler<Particle_Manager.ParticleArgs> OnOverride;
    public event EventHandler<Particle_Manager.ParticleArgs> OnCreateBlast;
    private UnitBaseModule BASE;
    private Augment_Manager Blackboard;
    protected Transform Pivot;
    protected Transform projectileTarget;

    [Header("Internal Variable")]
    [SerializeField] private float overrideTimer = 0;
    [SerializeField] private float overrideCooldown = 30;
    [SerializeField] private bool isOverrideCooldown = false;
    [SerializeField] private float attackTimer = 0;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private int attackCounter = 10;
    [SerializeField] private bool isAttackCooldown = false;
    [SerializeField] private int augmentCounter = 0;
    [SerializeField] private Queue<UnitOrbiterModule> controlledOrbiters = new Queue<UnitOrbiterModule>();

    public void Start()
    {
        BASE = GetComponent<UnitBaseModule>();
        Blackboard = Augment_Manager.Instane;
        Pivot = transform.Find("Pivot").gameObject.transform;
        projectileTarget = Pivot.Find("PSpawn").gameObject.transform;
        unitClass = ClassIndex.Calamity;
    }

    private void Update()
    {
        switch (CurrentState)
        {
            case (UnitState.Action):
                if(!isAttackCooldown && controlledOrbiters.Count > 0 && attackCounter > 0)
                {
                    FireAttack();
                }
                else if(controlledOrbiters.Count <= 0 || attackCounter <= 0)
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
                GetComponent<UnitManeuverModule>().SetDestination(UnitController.Instance.transform.position);
                break;
        }
    }

    private void FixedUpdate()
    {
        if (overrideTimer > 0 || isOverrideCooldown)
        {
            overrideTimer -= Time.deltaTime;
            if (overrideTimer <= 0)
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
    private void FireAttack()
    {
        UnitOrbiterModule releaseUnit = controlledOrbiters.Dequeue(); // get reference to unit in queue

        //---> upgrade target unit into elite <---//
        if(augmentCounter > 0)
        {
            releaseUnit.UpgradeElite();
            augmentCounter -= 1;
        }
        releaseUnit.OVERRIDE = false; // remove orbiter override

        //---> spawn projectile from point of release <---//
        Vector3 DIR = UnitController.Instance.transform.position - releaseUnit.transform.position;
        #region New Data
        Projectile.ProjectileObject attackDATA = ScriptableObject.CreateInstance<Projectile.ProjectileObject>();
        attackDATA = Projectile_Manager.Instance.ReplicateData(attackDATA, 8);

        attackDATA.damage += BASE.GetUnitDamage();
        attackDATA.isCrit = RollCrit(BASE.GetUnitCritRate());
        attackDATA.origin = releaseUnit.gameObject.transform.position;
        attackDATA.aimDirection = DIR;
        Quaternion Target = Quaternion.LookRotation(Vector3.forward, DIR);
        attackDATA.rotation = Target;
        #endregion
        Projectile_Manager.Instance.CreateProjectile(attackDATA, BASE.teamIndex);
        Audio_Manager.Instance.PlayClip(GetComponent<AudioSource>(), 4);

        OnCreateBlast?.Invoke(this, new Particle_Manager.ParticleArgs { index = 9, position = attackDATA.origin, parent = null });

        attackCounter -= 1;
        isAttackCooldown = true;
        attackTimer = attackCooldown;
    }
    private void ResetBehavior()
    {
        //---> release all controlled unit <---//
        int count = controlledOrbiters.Count;
        for (int i = 0; i < count; i++)
        {
            UnitOrbiterModule releaseUnit = controlledOrbiters.Dequeue();
            releaseUnit.OVERRIDE = false;
        }

        attackCounter = 10;
        attackTimer = 0;
        isAttackCooldown = false;
        overrideTimer = 0;
        isOverrideCooldown = false;
        augmentCounter = 0;

        CurrentState = UnitState.Idle;
        StartCoroutine("StartEngine");
    }
    private IEnumerator StartEngine()
    {
        yield return new WaitForSecondsRealtime(5f);
        CurrentState = UnitState.Overriding;
    }
    private void SeizeControl()
    {
        #region READY AUGMENT
        foreach (AugmentDef augment in Blackboard.Augments)
        {
            if (augment.augmentIndex == 24)
            {
                augmentCounter += 5;
            }
        }
        #endregion

        controlledOrbiters.Clear();
        //---> spawn new additional units <---//
        for (int i = 0; i < 15 + Mathf.RoundToInt(Level_Manager.Instance.GetSpawnRate() + (augmentCounter * 2)); i++)
        {
            UnitBaseModule newUnit = Level_Manager.Instance.SpawnUnitFromPool("Orbiter", transform.position);
            UnitOrbiterModule getOrbiter = newUnit.GetComponent<UnitOrbiterModule>();
            controlledOrbiters.Enqueue(getOrbiter);
            getOrbiter.OVERRIDE = true;
        }

        //---> get reference to orbiter units and override them <---//
        Collider2D[] collider2DArray = Physics2D.OverlapCircleAll(transform.position, 70);
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

        //---> select all seized orbiter and command them into formation <---//
        Vector3 DIR = (UnitController.Instance.transform.position - transform.position).normalized;
        float angle = (Vector2.SignedAngle(DIR, new Vector2(0, 1)) * -1);
        Vector3 destination = transform.position + (DIR * 20);
        List<Vector2> NewFormation = UnitMaster_Manager.Instance.RequestWing(angle, destination, 2.5f, 20, controlledOrbiters.Count);
        int counter = 0;
        foreach(UnitOrbiterModule unit in controlledOrbiters)
        {
            unit.GetComponent<UnitManeuverModule>().SetDestination(NewFormation[counter]);
            counter++;
        }

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

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Unit"))
        {
            UnitBaseModule HitCheck = collision.gameObject.GetComponent<UnitBaseModule>();
            if (HitCheck.teamIndex == TeamIndex.Ally)
            {
                HitCheck.ReceiveDamage(BASE.GetUnitDamage(), 0);
            }
        }
    }

    #region ENABLE/DISABLE
    private void OnEnable()
    {
        controlledOrbiters.Clear();
        augmentCounter = 0;
        attackTimer = 0;
        isAttackCooldown = false;
        attackCounter = 10;
        overrideTimer = 0;
        isOverrideCooldown = false;

        CurrentState = UnitState.Overriding;

        OnOverride += Particle_Manager.Instance.CreateParticle;
        OnCreateBlast += Particle_Manager.Instance.CreateParticle;
    }
    private void OnDisable()
    {
        //---> release all controlled unit <---//
        int count = controlledOrbiters.Count;
        for (int i = 0; i < count; i++)
        {
            UnitOrbiterModule releaseUnit = controlledOrbiters.Dequeue();
            releaseUnit.OVERRIDE = false;
        }

        controlledOrbiters.Clear();
        augmentCounter = 0;
        attackTimer = 0;
        attackCounter = 0;
        isAttackCooldown = false;
        overrideTimer = 0;
        isOverrideCooldown = false;

        CurrentState = UnitState.Idle;

        OnOverride -= Particle_Manager.Instance.CreateParticle;
        OnCreateBlast -= Particle_Manager.Instance.CreateParticle;
    }
    #endregion
}
