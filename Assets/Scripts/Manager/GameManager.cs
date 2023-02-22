using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityRandom = UnityEngine.Random;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private ControlScheme Controller;
    public event EventHandler<Personal.Utils.FloatArgs> OnGoldChange;
    public event EventHandler<Personal.Utils.FloatArgs> OnUpgradeChange;
    public event EventHandler<Personal.Utils.SingleBoolArgs> OnGamePaused;
    public event EventHandler<Particle_Manager.ParticleArgs> OnUnitDestroy;
    public event EventHandler<Particle_Manager.ParticleArgs> OnUnitEvolved;

    [Header("Game Condition/Status")]
    public bool GamePaused;
    public bool GameEnded;
    public bool inTimeSlow;
    public bool isPreparing;
    public bool returnMenu;

    [Header("System Property")]
    public bool reuseCollisionCallbacks;
    public float Default_TimeScale = 1f;
    public float Default_FixedDelta = 0.02f;
    [SerializeField] public float Slow_TimeScale;
    public float currency = 0;
    public float upgrade = 0;

    [Header("Post Processing")]
    [SerializeField] private Volume Normal_PP;
    [SerializeField] private Volume Paused_PP;

    [Header("Static")]
    public static int s_highscore;
    public static int s_score;
    public static int s_wave;
    public static int s_time;
    public static int s_money;
    public static int s_enemyKilled;
    public static int s_sentinelKilled;
    public static int s_calamityKilled;
    public static int s_unitRecruited;
    public static int s_unitDestroyed;
    public static int s_unitUpgraded;
    public static int s_augment;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }

    }
    void Start()
    {
        GamePaused = false;
        GameEnded = false;
        Physics2D.reuseCollisionCallbacks = reuseCollisionCallbacks;

        Level_Manager.Instance.Initiate();
        UnitMaster_Manager.Instance.Initinate();
        CameraFollow.Instance.Initiate();
        Projectile_Manager.Instance.Initiate();
        UI_Manager.Instance.Initinate();
        Shop_Manager.Instance.Initinate();

        OnGamePaused += UI_Manager.Instance.UpdateMenu;
        OnGoldChange += UI_Manager.Instance.UpdateGold;
        OnUpgradeChange += UI_Manager.Instance.UpdateUpgrade;
        OnUnitDestroy += Particle_Manager.Instance.CreateParticle;
        OnUnitEvolved += Particle_Manager.Instance.CreateParticle;

        if (Setting.difficultysetting == 0)
        {
            currency = 1000;
            UpdateGoldCount();
            upgrade = 3;
            UpdateUpgradeCount();
        }
        else if (Setting.difficultysetting == 1)
        {
            currency = 500;
            UpdateGoldCount();
            upgrade = 2;
            UpdateUpgradeCount();
        }
        else if (Setting.difficultysetting == 2)
        {
            currency = 200;
            UpdateGoldCount();
            upgrade = 1;
            UpdateUpgradeCount();
        }

        Parallax[] BG = FindObjectsOfType<Parallax>();
        foreach (Parallax pa in BG)
        {
            pa.Initinate();
        }
    }
    void Update()
    {
        if (GamePaused)
        {
            Time.timeScale = 0;
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                inTimeSlow = !inTimeSlow;
            }

            if (inTimeSlow)
            {
                Paused_PP.gameObject.SetActive(true);
                Time.timeScale = Slow_TimeScale;
                Time.fixedDeltaTime = Default_FixedDelta * Time.timeScale;
            }
            else if (!inTimeSlow)
            {
                Paused_PP.gameObject.SetActive(false);
                Time.timeScale = Default_TimeScale;
                Time.fixedDeltaTime = Default_FixedDelta * Time.timeScale;
            }
        }
    }

    #region VERIFY FUNCTION
    public void VerifyUnitDead(UnitBaseModule unit, float gold)
    {
        IUnit checkClass = unit.GetComponent(typeof(IUnit)) as IUnit;
        if (unit.teamIndex == Outsider.TeamIndex.Enemy)
        {
            //---> update money <---//
            currency += gold;
            UpdateGoldCount();

            float chanceItem = UnityRandom.Range(0f, 1f);
            if(chanceItem > 0.997f)
            {
                Level_Manager.Instance.SpawnItem(unit.transform.position);
            }

            s_money += (int)gold;
            s_enemyKilled += 1;
            if (checkClass.GetUnitClass() == Outsider.ClassIndex.Orbiter)
            {
                s_score += 50;
                OnUnitDestroy?.Invoke(this, new Particle_Manager.ParticleArgs { index = 5, position = unit.transform.position, parent = null });
            }
            if (checkClass.GetUnitClass() == Outsider.ClassIndex.Sentinel)
            {
                s_sentinelKilled++;
                s_score += 100;
                OnUnitDestroy?.Invoke(this, new Particle_Manager.ParticleArgs { index = 10, position = unit.transform.position, parent = null });
            }
            if (checkClass.GetUnitClass() == Outsider.ClassIndex.Calamity)
            {
                s_calamityKilled++;
                s_score += 500;
                OnUnitDestroy?.Invoke(this, new Particle_Manager.ParticleArgs { index = 10, position = unit.transform.position, parent = null });
            }
        }
        else if(unit.teamIndex == Outsider.TeamIndex.Ally)
        {
            if (unit.isActive)
            {
                s_unitDestroyed++;
            }
            OnUnitDestroy?.Invoke(this, new Particle_Manager.ParticleArgs { index = 5, position = unit.transform.position, parent = null });
        }
    }
    public void VerifyUpgrade(UnitBaseModule targetUnit)
    {
        if (!targetUnit.isEvolved && upgrade > 0)
        {
            upgrade -= 1;
            OnUpgradeChange?.Invoke(this, new Personal.Utils.FloatArgs { value = upgrade });
            int check = ((int)targetUnit.GetComponent<UnitClassBase>().GetUnitClass());
            OnUnitEvolved?.Invoke(this, new Particle_Manager.ParticleArgs { index = check, position = targetUnit.transform.position, parent = null });
            targetUnit.EvolveUnit();

            s_unitUpgraded++;
        }
    }
    public void VerifyStaticUnit()
    {
        s_unitRecruited++;
    }
    public void VerifyStaticTime(float time)
    {
        s_time = (int)time;
    }
    public void VerifyStaticWave(int wave)
    {
        s_wave = wave;
    }
    public void VerifyStaticAugment()
    {
        s_augment++;
    }
    #endregion

    #region GLOBAL FUNCTION
    public void GainBounty(float gold)
    {
        currency += gold;
        s_money += (int)gold;
        UpdateGoldCount();
    }
    public float GetCurrrentGold()
    {
        return currency;
    }

    public void UpdateGoldCount()
    {
        OnGoldChange?.Invoke(this, new Personal.Utils.FloatArgs { value = currency });
    }
    public void UpdateUpgradeCount()
    {
        OnUpgradeChange?.Invoke(this, new Personal.Utils.FloatArgs { value = upgrade });
    }
    public bool CalculatePurchase(float targetPrice)
    {
        bool purchase = false;
        if (currency > 0 && currency >= targetPrice)
        {
            currency -= targetPrice;
            OnGoldChange?.Invoke(this, new Personal.Utils.FloatArgs{ value = currency});
            purchase = true;
        }
        else
        {
            purchase = false;
        }
        return purchase;
    }
    public void GAMEOVER()
    {
        if (GamePaused)
            GamePaused = false;
        if (GameEnded && !returnMenu)
        {
            if(s_score > s_highscore)
            {
                s_highscore = s_score;
            }
            SceneManager.LoadScene("Gameover", LoadSceneMode.Single);
        }
    }
    #endregion

    #region PAUSE FUNCTION
    public void PauseGame()
    {
        if (!GameEnded)
        {
            if (GamePaused)
            {
                Resume();
            }
            else if (!GamePaused)
            {
                Pause();
            }
        }
    }
    private void Pause()
    {
        //AudioManager.Audio_M.PlayInterface(0);
        //AudioManager.Audio_M.PauseSource("internal");
        //AdaptiveCam.Camera.Pause_Volume.weight = 1;
        //UIManager.UI_M.OpenPauseMenu();
        //UI_Manager.Instance.EnablePauseMenu();

        if (isPreparing)
            return;

        GamePaused = true;
        OnGamePaused?.Invoke(this, new Personal.Utils.SingleBoolArgs { value = GamePaused });
    }
    public void Resume()
    {
        //AudioManager.Audio_M.PlayInterface(0);
        //AudioManager.Audio_M.UnpauseSource("internal");
        //AdaptiveCam.Camera.Pause_Volume.weight = 0;
        //UIManager.UI_M.ClosePauseMenu();
        //UI_Manager.Instance.DisablePauseMenu();

        if (isPreparing)
            return;

        GamePaused = false;
        OnGamePaused?.Invoke(this, new Personal.Utils.SingleBoolArgs { value = GamePaused });
    }
    #endregion

    #region ENABLE/DISABLE
    private void OnEnable()
    {
        Controller = new ControlScheme();
        Controller.Basic.Enable();
        Controller.Basic.Pause.performed += context => PauseGame();

        //s_highscore = 0;
        s_score = 0;
        s_wave = 0;
        s_time = 0;
        s_money = 0;
        s_enemyKilled = 0;
        s_sentinelKilled = 0;
        s_calamityKilled = 0;
        s_unitRecruited = 0;
        s_unitDestroyed = 0;
        s_unitUpgraded = 0;
        s_augment = 0;
    }
    private void OnDisable()
    {
        Controller.Basic.Disable();
        Controller.Basic.Pause.performed -= context => PauseGame();

        OnGamePaused    -= UI_Manager.Instance.UpdateMenu;
        OnGoldChange    -= UI_Manager.Instance.UpdateGold;
        OnUpgradeChange -= UI_Manager.Instance.UpdateUpgrade;
        OnUnitDestroy   -= Particle_Manager.Instance.CreateParticle;
        OnUnitEvolved   -= Particle_Manager.Instance.CreateParticle;
    }
    #endregion
}
