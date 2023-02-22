using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Outsider;

public class BombardAfterDestroy : MonoBehaviour
{
    public UnitMortarModule Parent;

    private void OnEnable()
    {
        Invoke("PlaySFX", 1.77f);
    }
    private void PlaySFX()
    {
        Audio_Manager.Instance.PlayClip(GetComponent<AudioSource>(), 12);
    }
    private void OnDisable()
    {
        Parent.SpawnMortarAtPosition(transform.position);
    }
}
