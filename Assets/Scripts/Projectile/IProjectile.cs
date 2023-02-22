using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Outsider;
public interface IProjectile
{
    void InitiateProjectile(Projectile.ProjectileObject DATA, TeamIndex teamIndex, Vector3 origin, Vector3 direction, bool isCrit);
    void ProjectileExpire();
}