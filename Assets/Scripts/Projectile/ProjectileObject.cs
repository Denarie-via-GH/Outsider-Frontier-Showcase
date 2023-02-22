using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Projectile
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/ProjectileData")]
    public class ProjectileObject : ScriptableObject
    {
        public GameObject projectilePrefab;
        public string projectileCode;
        public bool isCrit;
        public float damage;
        public float damageMultiplier;
        public float critMultiplier;
        public float penetrationRate;
        public Vector3 origin;
        public Vector3 aimDirection;
        public Quaternion rotation;
        public float maxDistance;
        public float speed;
        public float duration;

        public bool fromPool;

        public void SetOrigin(Vector3 origin)
        {
            this.origin = origin;
        }
        public void SetDirection(Vector3 direction)
        {
            this.aimDirection = direction;
        }
    }
}
