using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class UI_General : MonoBehaviour
{
    public SceneTransition Transitor;

    public void ToScene(string SN)
    {
        SceneManager.LoadScene(SN, LoadSceneMode.Single);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    public void PlayClickSFX()
    {
        //AudioManager.Audio_M.PlayInterface(1);
    }

    public void ToSceneTransit(string SN)
    {
        if (GameManager.Instance != null && GameManager.Instance.GamePaused)
            GameManager.Instance.GamePaused = false;
        Transitor.Target = SN;
        Transitor.Anim.Play("TransitionScene");
    }


    public void OpenInstruction(GameObject PANEL)
    {
        PANEL.SetActive(true);
    }
    public void OpenPanel(GameObject PANEL)
    {
        PANEL.SetActive(true);
    }

    public void CloseInstruction(GameObject PANEL)
    {
        PANEL.SetActive(false);
    }
    public void ClosePanel(GameObject PANEL)
    {
        PANEL.SetActive(false);
    }
}
