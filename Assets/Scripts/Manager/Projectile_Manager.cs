using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Outsider;
using UnityEngine.Pool;


public class Projectile_Manager : MonoBehaviour
{
    public static Projectile_Manager Instance;
    [Header("Data Variable")]
    public List<Projectile.ProjectileObject> projectileDataList;
    public Dictionary<string, Projectile.ProjectileObject> PresetData;

    [Header("Object Pool")]
    public List<Outsider.Pool> projectilePools;
    public Dictionary<string, Queue<GameObject>> ProjectileDictionary;


    #region INITIATE FUNCTION
    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public void Initiate()
    {
        ProjectileDictionary = new Dictionary<string, Queue<GameObject>>();    
        ReadyProjectilePool();
    }
    private void ReadyProjectilePool()
    {
        foreach (Pool P in projectilePools)
        {
            Queue<GameObject> projectilePool = new Queue<GameObject>();
            for (int i = 0; i < P.size; i++)
            {
                GameObject obj = Instantiate(P.prefab);
                obj.SetActive(false);
                projectilePool.Enqueue(obj);
            }

            ProjectileDictionary.Add(P.tag, projectilePool);
        }
    }
    #endregion

    #region Projectile Spawn: Instanite GameObject
    public void CreateProjectile(Projectile.ProjectileObject projectileDATA, TeamIndex team)
    {
        IProjectile newProjectile = Instantiate(projectileDATA.projectilePrefab, projectileDATA.origin, Quaternion.identity).GetComponent<IProjectile>();
        newProjectile.InitiateProjectile(projectileDATA, team, projectileDATA.origin, projectileDATA.aimDirection, projectileDATA.isCrit);
    }
    #endregion
    
    public Projectile.ProjectileObject ReplicateData(Projectile.ProjectileObject replicator, int index)
    {
        replicator.projectilePrefab     = projectileDataList[index].projectilePrefab;
        replicator.projectileCode       = projectileDataList[index].projectileCode;
        replicator.isCrit               = projectileDataList[index].isCrit;
        replicator.damage               = projectileDataList[index].damage;
        replicator.damageMultiplier     = projectileDataList[index].damageMultiplier;
        replicator.critMultiplier       = projectileDataList[index].critMultiplier;
        replicator.penetrationRate      = projectileDataList[index].penetrationRate;
        replicator.origin               = projectileDataList[index].origin;
        replicator.aimDirection         = projectileDataList[index].aimDirection;
        replicator.rotation             = projectileDataList[index].rotation;
        replicator.maxDistance          = projectileDataList[index].maxDistance;
        replicator.speed                = projectileDataList[index].speed;
        replicator.duration             = projectileDataList[index].duration;
        replicator.fromPool             = projectileDataList[index].fromPool;
        return replicator;
    }

    #region Projectile Spawn: Reactive from object pool
    public IProjectile SpawnProjectileCustomFromPool(Projectile.ProjectileObject customDATA, TeamIndex team)
    {
        return SpawnProjectileCustomFromPool(customDATA, team, customDATA.origin, customDATA.aimDirection, customDATA.isCrit);
    }
    public IProjectile SpawnProjectileCustomFromPool(Projectile.ProjectileObject customDATA, TeamIndex team, Vector3 position, Vector3 direction, bool isCRIT)
    {
        GameObject reuseProjectile = ProjectileDictionary[customDATA.projectileCode].Dequeue();
        reuseProjectile.SetActive(true);
        IProjectile getProjectile = reuseProjectile.GetComponent(typeof(IProjectile)) as IProjectile;
        getProjectile.InitiateProjectile(customDATA, team, position, direction, isCRIT);
        return getProjectile;
    }
    #endregion
}
