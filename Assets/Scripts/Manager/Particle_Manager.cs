using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle_Manager : MonoBehaviour
{
    public static Particle_Manager Instance;
    [SerializeField] private List<GameObject> particleList;
    public class ParticleArgs : EventArgs
    {
        public int index;
        public Vector3 position;
        public Transform parent;
    }
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public void CreateParticle(object sender, ParticleArgs e)
    {
        if (e.parent == null)
            CreateParticle(e.index, e.position);
        else
            CreateParticle(e.index, e.position, e.parent);
    }
    public void CreateParticle(int particleIndex, Vector3 position)
    {
        CreateParticle(particleIndex,position,Quaternion.identity, transform.root);
    }
    public void CreateParticle(int particleIndex, Vector3 position, Transform parent)
    {
        CreateParticle(particleIndex, position, Quaternion.identity, parent);
    }
    public void CreateParticle(int particleIndex, Vector3 position, Quaternion rotation, Transform parent)
    {
        GameObject newParticle = Instantiate(particleList[particleIndex], position, rotation, parent);
    }
}