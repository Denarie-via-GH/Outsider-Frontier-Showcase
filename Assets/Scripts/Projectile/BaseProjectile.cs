using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Outsider;

public class BaseProjectile : MonoBehaviour, IProjectile
{
    protected Vector3 aimDirection;
    protected string projectileCode;
    protected bool isCrit;
    protected float speed;
    protected float damage;
    protected float damageMultiplier;
    protected float critMultiplier;
    protected float penetrationRate;
    public TeamIndex teamIndex;
    public bool isFromPool;

    public virtual void InitiateProjectile(Projectile.ProjectileObject Data, TeamIndex teamIndex, Vector3 origin, Vector3 direction, bool isCrit)
    {
        this.damage = Data.damage;
        this.damageMultiplier = Data.damageMultiplier;
        this.penetrationRate = Data.penetrationRate;
        this.aimDirection = direction.normalized;
        this.projectileCode = Data.projectileCode;
        this.teamIndex = teamIndex;
        this.speed = Data.speed;
        this.critMultiplier = Data.critMultiplier;
        this.isCrit = isCrit;
        this.isFromPool = Data.fromPool;
        transform.position = Data.origin;
        transform.rotation = Data.rotation;
        Invoke("ProjectileExpire", Data.duration);
    }

    #region DISABLEING FUNCTION
    public virtual void ProjectileExpire()
    {
        if (isFromPool)
        {
            RecycleProjectile();
        }
        else if (!isFromPool)
        {
            DestroyProjectile();
        }
    }

    public void DestroyProjectile()
    {
        Destroy(this.gameObject);
    }
    public void RecycleProjectile()
    {
        Projectile_Manager.Instance.ProjectileDictionary[projectileCode].Enqueue(this.gameObject);
        this.gameObject.SetActive(false);
    }
    #endregion
    protected float CalculateDamage()
    {
        float finalDamage = (damage * damageMultiplier);
        if (isCrit)
        {
            finalDamage *= critMultiplier;
        }
        return finalDamage;
    }

    public virtual void OnEnable()
    {
        CancelInvoke();
    }
    public virtual void OnDisable()
    {
        CancelInvoke();
    }
}
