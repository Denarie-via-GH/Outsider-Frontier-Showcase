using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Outsider;
using UnityRandom = UnityEngine.Random;

public class Augment_Manager : MonoBehaviour
{
    public static Augment_Manager Instane;

    public bool augmentPhase = false;
    public bool miscPhase = false;

    public int augmentLimitCounter = 0;
    [SerializeField] private bool isSelecting;
    public class NewAugmentShop : EventArgs
    {
        public AugmentDef Augment;
    }
    public List<AugmentDef> AugmentPreset = new List<AugmentDef>();
    public List<AugmentDef> Augments = new List<AugmentDef>();

    private void Awake()
    {
        if(Instane != null)
        {
            Destroy(this.gameObject);
        }    
        else if(Instane == null)
        {
            Instane = this;
        }
    }

    public AugmentDef GenerateNewAugment(int index, ClassIndex classIndex, TeamIndex teamIndex)
    {
        AugmentDef newAugment       = ScriptableObject.CreateInstance<AugmentDef>();
        newAugment.augmentBanner    = AugmentPreset[index].augmentBanner;
        newAugment.augmentTarget    = teamIndex;
        newAugment.augmentIndex     = AugmentPreset[index].augmentIndex;
        newAugment.isSpecific       = AugmentPreset[index].isSpecific;
        newAugment.augmentPrice     = AugmentPreset[index].augmentPrice * Mathf.Pow(Level_Manager.Instance.difficultyScaling, 1.25f + (0.07f * Level_Manager.Instance.waveCleared));
        
        if(newAugment.isSpecific && classIndex != ClassIndex.None)
        {
            newAugment.specificClass = classIndex;
        }

        if (Setting.difficultysetting == 0)
        {
            newAugment.augmentPrice *= 0.9f;
        }
        else if (Setting.difficultysetting == 1)
        {
            newAugment.augmentPrice *= 1.1f;
        }
        else if (Setting.difficultysetting == 2)
        {
            newAugment.augmentPrice *= 1.5f;
        }

        return newAugment;
    }
}
