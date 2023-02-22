using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Personal.Utils;
using Outsider;

public class UI_Manager : MonoBehaviour
{
    public static UI_Manager Instance;

    private GameObject formationSelector;
    public GameObject augmentSelector;
    private Text goldText;
    private Text upgradeText;

    public GameObject PointerRef;
    public SceneTransition Transitor;
    private GameObject PointerDisplay;

    private Text timerText;
    private Text notificationText;
    private Text ticketText;

    private UnitBaseModule player;
    private Slider playerHP;
    private GameObject unitInformation;
    private GameObject unitPanelSelection;
    private GameObject Screen_Paused;
    private GameObject Screen_Shop;
    private GameObject unitShop;
    public GameObject controlPannel;
    private List<UnitDFSModule> unitListReference;
    private UnitBaseModule unitReference;
    private bool UpdateUnitInfo;
    private bool UpdateUnitPanel;

    #region INITIATE
    void Awake()
    {
        if(Instance != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }

    }
    private void Start()
    {
        playerHP = transform.Find("playerHP").GetComponent<Slider>();

        formationSelector = gameObject.transform.Find("FormationSelector").gameObject;
        augmentSelector = gameObject.transform.Find("AugmentSelector").gameObject;
        unitInformation = gameObject.transform.Find("UnitLayout").gameObject;
        unitPanelSelection = gameObject.transform.Find("UnityPanel").gameObject;
        Screen_Paused = gameObject.transform.Find("PauseScreen").gameObject;
        Screen_Shop = gameObject.transform.Find("ShopScreen").gameObject;
        unitShop = Screen_Shop.transform.Find("UnitShop").gameObject;
        PointerDisplay = gameObject.transform.Find("PointersDisplay").gameObject;

        Transitor = transform.Find("Transitor").GetComponent<SceneTransition>();

        ticketText = Screen_Shop.transform.Find("PurchaseTicket").GetComponent<Text>();
        goldText = gameObject.transform.Find("StarriumDisplay").Find("Text").GetComponent<Text>();
        upgradeText = gameObject.transform.Find("UpgradeDisplay").Find("Text").GetComponent<Text>();
        timerText = gameObject.transform.Find("Timer").Find("timer_info").GetComponent<Text>();
        notificationText = gameObject.transform.Find("Timer").Find("notification").GetComponent<Text>();
    }
    public void Initinate()
    {
        player = UnitController.Instance.GetComponent<UnitBaseModule>();
    }
    #endregion

    #region Formation UI
    public void SignalFormationDisplay(object sender, Personal.Utils.SingleBoolArgs e)
    {
        if(e.value == true)
        {
            EnableFormationSelector();
        }
        else
        {
            DisableFormationSelector();
        }
    }
    public void EnableFormationSelector()
    {
        formationSelector.SetActive(true);
        formationSelector.transform.position = Camera.main.WorldToScreenPoint(Utility.GetMouseWorldPOS());
    }
    public void DisableFormationSelector()
    {
        formationSelector.SetActive(false);
    }
    #endregion

    #region UNIT UI
    public void SignalUnitInfo(object sender, UnitMaster_Manager.SelectTargetArgs e)
    {
        if(e.selectedUnit.Count == 0)
        {
            DisableUnitInfo();
            DisableUnitPanel();
        }
        else if(e.selectedUnit.Count == 1)
        {
            EnableUnitInfo(e.selectedUnit[0].GetComponent<UnitBaseModule>());
        }
        else if(e.selectedUnit.Count > 1)
        {
            EnableUnitPanel(e.selectedUnit);
        }
    }
    public void RequestEvolveUnit()
    {
        GameManager.Instance.VerifyUpgrade(unitReference);
    }
    public void EnableUnitInfo(UnitBaseModule unit)
    {
        UpdateUnitInfo = true;
        unitReference = unit;
        unitInformation.SetActive(true);
    }
    public void DisableUnitInfo()
    {
        UpdateUnitInfo = false;
        unitReference = null;
        unitInformation.SetActive(false);
    }
    public void EnableUnitPanel(List<UnitDFSModule> unitArray)
    {
        UpdateUnitPanel = true;
        unitListReference = unitArray;
        unitPanelSelection.SetActive(true);
    }
    public void DisableUnitPanel()
    {
        UpdateUnitPanel = false;
        unitListReference = null;
        unitPanelSelection.SetActive(false);
    }
    #endregion

    private void Update()
    {
        if (UpdateUnitInfo)
        {
            if(unitReference == null)
            {
                DisableUnitInfo();
                return;
            }

            unitInformation.transform.Find("Title").GetComponent<Text>().text = unitReference.unitCode;
            unitInformation.transform.Find("stat_logs").Find("atk").GetComponent<Text>().text = "ATK: " + unitReference.GetUnitDamage().ToString("f0");
            unitInformation.transform.Find("stat_logs").Find("def").GetComponent<Text>().text = "DEF: " + unitReference.GetUnitDefense().ToString("f0");
            unitInformation.transform.Find("stat_logs").Find("spd").GetComponent<Text>().text = "SPD: " + unitReference.GetUnitSpeed().ToString("f0");
            unitInformation.transform.Find("stat_logs").Find("reg").GetComponent<Text>().text = "REG: " + unitReference.GetUnitRegeneration().ToString("f0");

            float hpPercent = unitReference.GetHP() / unitReference.GetMaxHP();
            unitInformation.transform.Find("hp_slider").GetComponent<Slider>().value = hpPercent;

            UnitClassBase TEMP = unitReference.gameObject.GetComponent<UnitClassBase>();
            float cdPercent = TEMP.GetUltimateTimer() / TEMP.GetUltimateCooldown();
            unitInformation.transform.Find("cd_slider").GetComponent<Slider>().value = cdPercent;

            if (unitReference.isEvolved)
            {
                unitInformation.transform.Find("upgrade").GetComponent<Button>().interactable = false;
            }
            else
            {
                unitInformation.transform.Find("upgrade").GetComponent<Button>().interactable = true;
            }
        }

        if (UpdateUnitPanel)
        {
            unitListReference.RemoveAll(delegate (UnitDFSModule x) { return x == null; });
            if(unitListReference == null || unitListReference.Count <= 0)
            {
                DisableUnitPanel();
                return;
            }

            int cutlass = 0;
            int tracer = 0;
            int raygunner = 0;
            int hyperion = 0;

            foreach (UnitDFSModule Unit in unitListReference)
            {
                ClassIndex check = Unit.gameObject.GetComponent<UnitClassBase>().GetUnitClass();
                switch (check)
                {
                    case ClassIndex.Cutlass:
                        cutlass++;
                        //CutlassArray.Add(check);
                        break;
                    case ClassIndex.Tracer:
                        tracer++;
                        //TracerArray.Add(check);
                        break;
                    case ClassIndex.Raygunner:
                        raygunner++;
                        //RaygunnerArray.Add(check);
                        break;
                    case ClassIndex.Hyperion:
                        hyperion++;
                        //HyperionArray.Add(check);
                        break;
                }
            }

            unitPanelSelection.transform.Find("margin").Find("cutlass").Find("Text").GetComponent<Text>().text = cutlass.ToString();
            unitPanelSelection.transform.Find("margin").Find("tracer").Find("Text").GetComponent<Text>().text = tracer.ToString();
            unitPanelSelection.transform.Find("margin").Find("raygunner").Find("Text").GetComponent<Text>().text = raygunner.ToString();
            unitPanelSelection.transform.Find("margin").Find("hyperion").Find("Text").GetComponent<Text>().text = hyperion.ToString();
        }

        if(player != null)
        {
            float hpPercent = player.GetHP() / player.GetMaxHP();
            playerHP.GetComponent<Slider>().value = hpPercent;
        }
    }

    #region SHOP UI
    public void UpdateTicket(int c)
    {
        ticketText.text = "Purchase Remaining: " + c;
    }
    public void EnableShopUI(object sender, EventArgs e)
    {
        //---> enable shops screen <---//
        Screen_Shop.SetActive(true);
        ticketText.gameObject.SetActive(true);
        augmentSelector.SetActive(true);
    }
    public void DisableShopUI(object sender, EventArgs e)
    {
        //---> clear augment shop <---//
        foreach (Transform child in augmentSelector.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        augmentSelector.SetActive(false);

        //---> disable other shop screen <---//
        unitShop.SetActive(false);
        Screen_Shop.SetActive(false);
    }
    public void EnableUnitShopUI(object sender, Shop_Manager.UnitPriceArgs e)
    {
        augmentSelector.SetActive(false);
        ticketText.gameObject.SetActive(false);
        unitShop.SetActive(true);
        
        UpdateUnitShop(sender, e);        
    }
    public void UpdateUnitShop(object sender, Shop_Manager.UnitPriceArgs e)
    {
        Transform UnitPanels = unitShop.transform.Find("UnitPanel");
        UnitPanels.Find("unit1Purchase").Find("price").GetComponent<Text>().text = (int)e.newUnitPrice["Cutlass"] + " s.";
        UnitPanels.Find("unit2Purchase").Find("price").GetComponent<Text>().text = (int)e.newUnitPrice["Tracer"] + " s.";
        UnitPanels.Find("unit3Purchase").Find("price").GetComponent<Text>().text = (int)e.newUnitPrice["Raygunner"] + " s.";
        UnitPanels.Find("unit4Purchase").Find("price").GetComponent<Text>().text = (int)e.newUnitPrice["Hyperion"] + " s.";
        unitShop.transform.Find("UpgradePanel").Find("price").GetComponent<Text>().text = (int)e.newUnitPrice["Upgrade"] + " s.";
    }

    public void ResetAugmentShop(object sender, EventArgs e)
    {
        foreach (Transform child in augmentSelector.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }
    public void AddAugmentToShop(object sender, Augment_Manager.NewAugmentShop e)
    {
        Button AugmentButton = Instantiate(e.Augment.augmentBanner, augmentSelector.transform).GetComponent<Button>();
        AugmentButton.gameObject.transform.Find("price").GetComponent<Text>().text = Mathf.RoundToInt(e.Augment.augmentPrice) + " s.";
        AugmentButton.gameObject.transform.Find("target").GetComponent<Text>().text = "Target: " + e.Augment.augmentTarget; 
        AugmentButton.gameObject.transform.Find("class").GetComponent<Text>().text = "Specific Class: " + e.Augment.specificClass;
        AugmentButton.onClick.AddListener(delegate { Shop_Manager.Instance.PurchaseAugment(e.Augment, AugmentButton.gameObject); });
    }


    #endregion

    #region PAUSE MENU UI
    public void UpdateMenu(object sender, Personal.Utils.SingleBoolArgs e)
    {
        if (e.value == true)
        {
            EnablePauseMenu();
        }
        else
        {
            DisablePauseMenu();
        }
    }
    public void EnablePauseMenu()
    {
        Audio_Manager.Instance.PlayInterface(0);
        Screen_Paused.SetActive(true);
    }
    public void DisablePauseMenu()
    {
        Audio_Manager.Instance.PlayInterface(1);
        Screen_Paused.SetActive(false);
    }
    public void ResumeButton()
    {
        GameManager.Instance.PauseGame();
    }
    public void ReturnMenu()
    {
        GameManager.Instance.Resume();
        GameManager.Instance.returnMenu = true;
        GameManager.Instance.GameEnded = true;
        Transitor.Target = "StartScene";
        Transitor.Anim.Play("TransitionScene");
    }

    public void OpenControl()
    {
        controlPannel.SetActive(true);
    }
    public void CloseControl()
    {
        controlPannel.SetActive(false);
    }
    #endregion

    #region UPDATE UI
    public void UpdateTimer(object sender, Personal.Utils.FloatArgs e)
    {
        if (e.value != -1)
        {
            string minute = ((int)e.value / 60).ToString();
            string second = (e.value % 60).ToString("f0");
            timerText.text = minute + ":" + second;
        }
        else
        {
            timerText.text = "ELIMINATE";
        }
    }
    public void UpdateNotification(object sender, Personal.Utils.SingleStringArgs e)
    {
        notificationText.text = e.value;
    }
    public void UpdateGold(object sender, Personal.Utils.FloatArgs e)
    {
        goldText.text = Math.Round(e.value).ToString();
    }
    public void UpdateUpgrade(object sender, Personal.Utils.FloatArgs e)
    {
        upgradeText.text = e.value.ToString();
    }
    #endregion

    public void PlayClickSFX()
    {
        Audio_Manager.Instance.PlayInterface(2);
    }

    public ItemPointer SpawnItemPointer(Transform itemTrans)
    {
        ItemPointer newPoint = Instantiate(PointerRef, PointerDisplay.transform.position, Quaternion.identity, PointerDisplay.transform).GetComponent<ItemPointer>();
        return newPoint;
    }
}
