using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Outsider
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/AugmentDef")]
    public class AugmentDef : ScriptableObject
    {
        public GameObject augmentBanner;
        public TeamIndex augmentTarget = TeamIndex.None;
        public ClassIndex specificClass = ClassIndex.None;
        public int augmentIndex;
        public bool isSpecific;
        public int augmentStack;
        public float augmentPrice;

        public override string ToString()
        {
            return "Index:" + augmentIndex + ", Stack: " + augmentStack + ", Target: " + augmentTarget;
        }
    }
}