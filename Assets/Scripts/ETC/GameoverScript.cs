using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class GameoverScript : MonoBehaviour
{
    public static GameoverScript Instance;
    public string Target;
    public GameObject GameoverCredit;
    public GameObject StaticDisplay;

    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(this.gameObject);
        }
        else if(Instance == null)
        {
            Instance = this;
        }
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
        GameoverCredit.SetActive(false);
        StaticDisplay.SetActive(true);
        Transform main = StaticDisplay.transform.Find("arrange");
        main.Find("highscore").GetComponent<Text>().text = "Current High Score: " + GameManager.s_highscore;
        main.Find("score").GetComponent<Text>().text = "Final Score: " + GameManager.s_score;
        main.Find("wave").GetComponent<Text>().text = "Wave Survived: " + GameManager.s_wave;
        main.Find("time").GetComponent<Text>().text = "Time Survived: " + GameManager.s_time;
        main.Find("money").GetComponent<Text>().text = "Starrium Gained: " + GameManager.s_money;
        main.Find("enemykill").GetComponent<Text>().text = "Enemy Killed: " + GameManager.s_enemyKilled;
        main.Find("sentinelkill").GetComponent<Text>().text = "Sentinel Class Killed: " + GameManager.s_sentinelKilled;
        main.Find("calamitykill").GetComponent<Text>().text = "Calamity Class Killed: " + GameManager.s_calamityKilled;
        main.Find("recruit").GetComponent<Text>().text = "Unit Recruited: " + GameManager.s_unitRecruited;
        main.Find("unitdestroy").GetComponent<Text>().text = "Unit Destroyed: " + GameManager.s_unitDestroyed;
        main.Find("unitupgrade").GetComponent<Text>().text = "Unit Upgraded: " + GameManager.s_unitUpgraded;
        main.Find("augment").GetComponent<Text>().text = "Augment Purchased: " + GameManager.s_augment;
    }
    public void ReturnMenu()
    {
        SceneManager.LoadScene(Target, LoadSceneMode.Single);
    }
}
