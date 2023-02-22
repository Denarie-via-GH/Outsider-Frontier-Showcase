using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditRelay : MonoBehaviour
{
    public void Continue()
    {
        GameoverScript.Instance.Continue();
    }
}
