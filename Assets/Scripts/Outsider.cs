using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Outsider
{
    public enum TeamIndex : sbyte
    {
        None = -1,
        Neutral,
        Player,
        Enemy,
        Ally
    }

    public enum ClassIndex : sbyte
    {
        None = -1,
        Cutlass,
        Tracer,
        Raygunner,
        Hyperion,
        Orbiter,
        Sentinel,
        Calamity
    }

    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }
}