using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroScript : MonoBehaviour
{
    public string Target;
    public static bool watchedIntro = false;

    public void Start()
    {
        if (watchedIntro)
            Continue();
    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Continue();
        }
    }
    public void Continue()
    {
        if(!watchedIntro)
            watchedIntro = true;
        SceneManager.LoadScene(Target, LoadSceneMode.Single);
    }
}
