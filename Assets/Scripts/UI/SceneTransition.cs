using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneTransition : MonoBehaviour
{
    public string Target;
    public Animator Anim;

    public void TransitNewScene()
    {
        SceneManager.LoadScene(Target, LoadSceneMode.Single);
    }
}
