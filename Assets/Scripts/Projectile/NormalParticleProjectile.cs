using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Outsider;

public class NormalParticleProjectile : NormalProjectile, IProjectile
{
    public event EventHandler<Particle_Manager.ParticleArgs> OnExpire;
    public int particle_index;

    public override void ProjectileExpire()
    {
        OnExpire?.Invoke(this, new Particle_Manager.ParticleArgs { index = particle_index, parent = null, position = transform.position });
        base.ProjectileExpire();
    }

    public override void OnEnable()
    {
        base.OnEnable();
        OnExpire += Particle_Manager.Instance.CreateParticle;
    }
    public override void OnDisable()
    {
        base.OnDisable();
        OnExpire -= Particle_Manager.Instance.CreateParticle;
    }
}
