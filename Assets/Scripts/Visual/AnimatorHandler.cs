using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorHandler : MonoBehaviour
{
    private Animator ANIM;
    public class TriggerAnimationArgs : EventArgs
    {
        public string anim_string;
    }

    void Awake()
    {
        ANIM = GetComponent<Animator>();
    }

    public void SetBoolParameter(object sender, Personal.Utils.BoolAndStringArgs e)
    {
        ANIM.SetBool(e.string_value, e.bool_value);
    }
    public void TriggerAnimation(object sender, Personal.Utils.SingleStringArgs e)
    {
        ANIM.Play(e.value);
    }
}
