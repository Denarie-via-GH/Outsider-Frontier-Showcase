using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadoutMenu : MonoBehaviour
{
    public Text loadoutDisplay;
    
    [Header("Static Setting")]
    public static int loadoutIndex = 0;

    public GameObject[] ButtonArray;
    public GameObject[] LoadoutButtonArray;
    public GameObject[] MenuPanelArray;

    public GameObject loadoutPanel;
    public Text DifficultyDisplay;

    public GameObject in_page1;
    public GameObject in_page2;

    public Sprite ButtonDefault;
    public Sprite ButtonLight;

    public Toggle musicToggle;
    public Toggle soundToggle;

    public SceneTransition Transitor;
    public void StarGame(string SN)
    {
        if (GameManager.Instance != null && GameManager.Instance.GamePaused)
            GameManager.Instance.GamePaused = false;
        Transitor.Target = SN;
        Transitor.Anim.Play("TransitionScene");
    }

    public void SelectLoadout(int index)
    {
        switch (index)
        {
            case 0: 
                loadoutDisplay.text = "RAIDEN";
                loadoutDisplay.color = new Color(1, 0, 0, 1);
                foreach (GameObject L in LoadoutButtonArray)
                {
                    L.GetComponent<Image>().color = Color.white;
                }
                LoadoutButtonArray[0].GetComponent<Image>().color = new Color(1, 0, 0, 1);

                loadoutIndex = index;
                break;

            case 1:
                loadoutDisplay.text = "RAPID FAIRY";
                loadoutDisplay.color = new Color(0.2f, 1, 0.9803922f, 1);
                foreach (GameObject L in LoadoutButtonArray)
                {
                    L.GetComponent<Image>().color = Color.white;
                }
                LoadoutButtonArray[1].GetComponent<Image>().color = new Color(0.2f, 1, 0.9803922f, 1);

                loadoutIndex = index;
                break;

            case 2:
                loadoutDisplay.text = "DEADLOCK";
                loadoutDisplay.color = new Color(0.1647059f, 0.9058824f, 0.4431373f, 1);
                foreach (GameObject L in LoadoutButtonArray)
                {
                    L.GetComponent<Image>().color = Color.white;
                }
                LoadoutButtonArray[2].GetComponent<Image>().color = new Color(0.1647059f, 0.9058824f, 0.4431373f, 1);

                loadoutIndex = index;
                break;

            case 3:
                loadoutDisplay.text = "APOLLOS";
                loadoutDisplay.color = new Color(0.4313726f, 0.08627451f, 0.427451f, 1);
                foreach (GameObject L in LoadoutButtonArray)
                {
                    L.GetComponent<Image>().color = Color.white;
                }
                LoadoutButtonArray[3].GetComponent<Image>().color = new Color(0.4313726f, 0.08627451f, 0.427451f, 1);

                loadoutIndex = index;
                break;
        }
    }

    public void OpenPanel(int panel)
    {
        loadoutPanel.SetActive(false);

        switch (panel)
        {
            case 0:
                foreach(GameObject B in ButtonArray)
                {
                    B.GetComponent<Image>().sprite = ButtonDefault;
                    B.transform.Find("Text").GetComponent<Text>().color = Color.white;
                }
                ButtonArray[0].GetComponent<Image>().sprite = ButtonLight;
                ButtonArray[0].transform.Find("Text").GetComponent<Text>().color = Color.black;

                foreach(GameObject M in MenuPanelArray)
                {
                    M.SetActive(false);
                }
                MenuPanelArray[0].SetActive(true);
                break;

            case 1:
                foreach (GameObject B in ButtonArray)
                {
                    B.GetComponent<Image>().sprite = ButtonDefault;
                    B.transform.Find("Text").GetComponent<Text>().color = Color.white;
                }
                ButtonArray[1].GetComponent<Image>().sprite = ButtonLight;
                ButtonArray[1].transform.Find("Text").GetComponent<Text>().color = Color.black;

                foreach (GameObject M in MenuPanelArray)
                {
                    M.SetActive(false);
                }
                MenuPanelArray[1].SetActive(true);
                break;

            case 2:
                foreach (GameObject B in ButtonArray)
                {
                    B.GetComponent<Image>().sprite = ButtonDefault;
                    B.transform.Find("Text").GetComponent<Text>().color = Color.white;
                }
                ButtonArray[2].GetComponent<Image>().sprite = ButtonLight;
                ButtonArray[2].transform.Find("Text").GetComponent<Text>().color = Color.black;

                foreach (GameObject M in MenuPanelArray)
                {
                    M.SetActive(false);
                }
                MenuPanelArray[2].SetActive(true);
                break;
        }
    }
    public void ClosePanel()
    {
        loadoutPanel.SetActive(true);

        foreach (GameObject B in ButtonArray)
        {
            B.GetComponent<Image>().sprite = ButtonDefault;
            B.transform.Find("Text").GetComponent<Text>().color = Color.white;
        }
        foreach (GameObject M in MenuPanelArray)
        {
            M.SetActive(false);
        }
    }

    public void SettingDifficulty(int Val)
    {
        Setting.difficultysetting = Val;
        if (Val == 0)
            DifficultyDisplay.text = "Difficulty: EASY";
        else if (Val == 1)
            DifficultyDisplay.text = "Difficulty: NORMAL";
        else if (Val == 2)
            DifficultyDisplay.text = "Difficulty: HARD";
    }
    public void PlayClickSFX()
    {
        Audio_Manager.Instance.PlayInterface(2);
    }
    public void InstructionButton(int value)
    {
        if(value == 0)
        {
            in_page1.SetActive(false);
            in_page2.SetActive(true);
        }
        else if(value == 1)
        {
            in_page1.SetActive(true);
            in_page2.SetActive(false);
        }
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    private void OnEnable()
    {
        loadoutDisplay = transform.Find("Panel").Find("ButtonPanel").Find("loadout").Find("Text").GetComponent<Text>();

        switch (loadoutIndex)
        {
            case 0:
                loadoutDisplay.text = "RAIDEN";
                loadoutDisplay.color = new Color(1, 0, 0, 1);
                foreach (GameObject L in LoadoutButtonArray)
                {
                    L.GetComponent<Image>().color = Color.white;
                }
                LoadoutButtonArray[0].GetComponent<Image>().color = new Color(1, 0, 0, 1);

                break;

            case 1:
                loadoutDisplay.text = "RAPID FAIRY";
                loadoutDisplay.color = new Color(0.2f, 1, 0.9803922f, 1);
                foreach (GameObject L in LoadoutButtonArray)
                {
                    L.GetComponent<Image>().color = Color.white;
                }
                LoadoutButtonArray[1].GetComponent<Image>().color = new Color(0.2f, 1, 0.9803922f, 1);

                break;

            case 2:
                loadoutDisplay.text = "DEADLOCK";
                loadoutDisplay.color = new Color(0.1647059f, 0.9058824f, 0.4431373f, 1);
                foreach (GameObject L in LoadoutButtonArray)
                {
                    L.GetComponent<Image>().color = Color.white;
                }
                LoadoutButtonArray[2].GetComponent<Image>().color = new Color(0.1647059f, 0.9058824f, 0.4431373f, 1);

                break;

            case 3:
                loadoutDisplay.text = "APOLLOS";
                loadoutDisplay.color = new Color(0.4313726f, 0.08627451f, 0.427451f, 1);
                foreach (GameObject L in LoadoutButtonArray)
                {
                    L.GetComponent<Image>().color = Color.white;
                }
                LoadoutButtonArray[3].GetComponent<Image>().color = new Color(0.4313726f, 0.08627451f, 0.427451f, 1);

                break;
        }
        if (Setting.difficultysetting == 0)
            DifficultyDisplay.text = "Difficulty: EASY";
        else if (Setting.difficultysetting == 1)
            DifficultyDisplay.text = "Difficulty: NORMAL";
        else if (Setting.difficultysetting == 2)
            DifficultyDisplay.text = "Difficulty: HARD";

        musicToggle.isOn = Setting.musicsetting;
        soundToggle.isOn = Setting.soundsetting;
    }
}
