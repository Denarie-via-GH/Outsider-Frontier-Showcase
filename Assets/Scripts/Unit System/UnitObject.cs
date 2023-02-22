using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Outsider;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/UnitData")]
public class UnitObject : ScriptableObject
{
    public GameObject unitPrefab;
    public string unitCode = "";
    public bool fromPool;
    public float unitMaxHP;
    public float unitHP;
    public float unitDMG;
    public float unitDEF;
    public float unitCritRate;
    public float unitSpeed;
    public float unitRegen;
    public float unitDrop;
    public float unitIframeDuration;
    public Vector3 origin;
    public Vector3 direction;
    public Quaternion rotation;
    public TeamIndex teamIndex;

    public float hp_scaling;
    public float regen_scaling;
    public float damage_scaling;

    //AddAdditionalModule
}
