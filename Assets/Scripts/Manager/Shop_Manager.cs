using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Outsider;
using UnityRandom = UnityEngine.Random;

public class Shop_Manager : MonoBehaviour
{
    public static Shop_Manager Instance;
    public enum State { Idle, AugmentPhase, UnitPhase}
    public State CurrentState;
    public float [] unitPrice = { 25, 50, 100, 200};
    public Dictionary<string, float> Price = new Dictionary<string, float>();
    public float upgradePrice = 75;
    public event EventHandler<Augment_Manager.NewAugmentShop> OnGetRandomizeShop;
    public class UnitPriceArgs : EventArgs
    {
        public Dictionary<string,float> newUnitPrice;
    }

    public event EventHandler OnOpenShop;
    public event EventHandler<UnitPriceArgs> OnOpenUnitShop;
    public event EventHandler OnCloseShop;

    public event EventHandler OnPurchaseAugment;
    public event EventHandler<UnitPriceArgs> OnPurchaseUnit;
    public event EventHandler<UnitPriceArgs> OnPurchaseUpgrade;

    private bool forceAugment = false;
    private int augmentTicket = 3;
    public int augmentTicketBackup = 0;
    private void Awake()
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

    public void Initinate()
    {
        Price.Add("Cutlass", 25);
        Price.Add("Tracer", 50);
        Price.Add("Raygunner", 100);
        Price.Add("Hyperion", 200);
        Price.Add("Upgrade", 75);

        OnGetRandomizeShop += UI_Manager.Instance.AddAugmentToShop;

        OnOpenShop += UI_Manager.Instance.EnableShopUI;
        OnOpenUnitShop += UI_Manager.Instance.EnableUnitShopUI;
        OnCloseShop += UI_Manager.Instance.DisableShopUI;

        OnPurchaseAugment += UI_Manager.Instance.ResetAugmentShop;
        OnPurchaseUnit += UI_Manager.Instance.UpdateUnitShop;
        OnPurchaseUpgrade += UI_Manager.Instance.UpdateUnitShop;
    }

    #region AUGMENT SHOP
    public void AdaptiveSkip() // when in augment phase, skip current augment ticket | if run out of ticket enter unit phase, skip entire phase 
    {
        Audio_Manager.Instance.PlayInterface(2);

        switch (CurrentState)
        {
            case State.AugmentPhase:
                //---> skip current augment <---//
                if(augmentTicket > 0 && !forceAugment)
                {
                    //---> consume augment ticket and random new augment to shop <---//
                    augmentTicket--;
                    if (augmentTicket <= 0)
                    {
                        OpenUnitShop();
                    }
                    else
                    {
                        OnPurchaseAugment?.Invoke(this, EventArgs.Empty);
                        RandomShop(Level_Manager.Instance.GetDifficultyScaling());
                    }
                }
                //---> skip augment phase <---//
                else if(augmentTicket <= 0)
                {
                    OpenUnitShop();
                }
                break;
            case State.UnitPhase:
                //---> skip unit phase and close shop <---//
                CloseShop();
                break;
        }
    }
    public void OpenUnitShop()
    {
        CurrentState = State.UnitPhase;
        Price["Cutlass"]    = Mathf.Round(20 * (Level_Manager.Instance.GetDifficultyScaling() + (1f * Level_Manager.Instance.waveCleared)));
        Price["Tracer"]     = Mathf.Round(50 * (Level_Manager.Instance.GetDifficultyScaling() + (1.2f * Level_Manager.Instance.waveCleared)));
        Price["Raygunner"]  = Mathf.Round(100 * (Level_Manager.Instance.GetDifficultyScaling() + (1.5f * Level_Manager.Instance.waveCleared)));
        Price["Hyperion"]   = Mathf.Round(200 * (Level_Manager.Instance.GetDifficultyScaling() + (2f * Level_Manager.Instance.waveCleared)));
        Price["Upgrade"] = Mathf.Round(75 * (Level_Manager.Instance.GetDifficultyScaling() + (1.7f * Level_Manager.Instance.waveCleared)));
        OnOpenUnitShop?.Invoke(this, new UnitPriceArgs { newUnitPrice = Price});
    }
    public void OpenShop()
    {
        GameManager.Instance.isPreparing = true;
        CurrentState = State.AugmentPhase;
        augmentTicket = 3 + augmentTicketBackup;
        
        if(Level_Manager.Instance.waveCleared % 3 == 0)
            RandomEnemyShop(0);
        else if (Level_Manager.Instance.waveCleared % 3 != 0)
            RandomShop(Level_Manager.Instance.GetDifficultyScaling());

        Audio_Manager.Instance.PlayInterface(0);
        OnOpenShop?.Invoke(this, EventArgs.Empty);
    }
    public void CloseShop()
    {
        GameManager.Instance.isPreparing = false;
        CurrentState = State.Idle;
        augmentTicket = 0;
        augmentTicketBackup = 0;

        Price = new Dictionary<string, float> {
            ["Cutlass"] = 20,
            ["Tracer"] = 50,
            ["Raygunner"] = 100,
            ["Hyperion"] = 200,
            ["Upgrade"] = 75};
        Audio_Manager.Instance.PlayInterface(1);
        OnCloseShop?.Invoke(this, EventArgs.Empty);
        Level_Manager.Instance.InitiateWaveCountdown();
    }

    #region PURCHASE FUNCTION
    public void PurchaseAugment(AugmentDef Augment, GameObject button)
    {
        if (Augment.augmentTarget == TeamIndex.Ally)
        {
            //---> validate if purchase succesfull <---//
            if (GameManager.Instance.CalculatePurchase(Augment.augmentPrice))
            {
                GameManager.Instance.VerifyStaticAugment();

                Audio_Manager.Instance.PlayInterface(2);
                OnPurchaseAugment?.Invoke(this, EventArgs.Empty);   // update ui element
                
                Augment_Manager.Instane.Augments.Add(Augment);      // add augment to list
                augmentTicket--;                                    // reduce augment ticket

                //---> after purchase augment with ticket remaining, random new augment <---//
                if (augmentTicket > 0)
                {
                    RandomShop(Level_Manager.Instance.GetDifficultyScaling());
                }
                //---> after purchase augment without remaining augment, open unit shop <---//
                else
                {
                    OpenUnitShop();
                }
            }
        }
        else if(Augment.augmentTarget == TeamIndex.Enemy)
        {
            Audio_Manager.Instance.PlayInterface(3);

            forceAugment = false;
            GameManager.Instance.GainBounty(Augment.augmentPrice);
            OnPurchaseAugment?.Invoke(this, EventArgs.Empty);   // update ui element
            Augment_Manager.Instane.Augments.Add(Augment);      // add augment to list
            RandomShop(Level_Manager.Instance.GetDifficultyScaling());
        }
    }
    public void PurchaseUpgrade()
    {
        if (GameManager.Instance.CalculatePurchase(Price["Upgrade"]))
        {
            Audio_Manager.Instance.PlayInterface(2);
            GameManager.Instance.upgrade++;
            GameManager.Instance.UpdateUpgradeCount();

            //---> update visual <---//
            Price["Upgrade"] = Price["Upgrade"] * 1.4f;
            OnPurchaseUpgrade?.Invoke(this, new UnitPriceArgs { newUnitPrice = Price });
        }
    }
    public void PurchaseUnit(string index)
    {   
        if (GameManager.Instance.CalculatePurchase(Price[index]))
        {
            Transform player = FindObjectOfType<UnitController>().transform; // temporary unit spawn position

            Level_Manager.Instance.UnitQueue.Add(index);
            Price["Cutlass"]    = Price["Cutlass"] * 1.5f;
            Price["Tracer"]     = Price["Tracer"] * 1.5f;
            Price["Raygunner"]  = Price["Raygunner"] * 1.5f;
            Price["Hyperion"]   = Price["Hyperion"] * 1.5f;
            
            Audio_Manager.Instance.PlayInterface(2);
            OnPurchaseUnit?.Invoke(this, new UnitPriceArgs { newUnitPrice = Price });
        }

    }
    #endregion

    public void RandomShop(float difficultyRate)
    {
        float rarityRate = difficultyRate * 0.01f;
        float SR_RATE = Mathf.Clamp(Mathf.Abs(0.03f + rarityRate/2), 0, 0.5f);
        float R_RATE = Mathf.Clamp(Mathf.Abs(0.25f + rarityRate), 0, 0.5f);

        UI_Manager.Instance.UpdateTicket(augmentTicket);

        for (int i = 0; i < 3; i++)
        {
            float randomize = UnityRandom.Range(0f, 1f);
            if (randomize >= (1 - SR_RATE))
            {
                int randomAugment = UnityRandom.Range(11, 23);
                OnGetRandomizeShop?.Invoke(this, new Augment_Manager.NewAugmentShop { Augment = Augment_Manager.Instane.GenerateNewAugment(randomAugment, ClassIndex.None, TeamIndex.Ally) });
            }
            else
            {
                if (randomize >= 1 - R_RATE)
                {
                    int randomAugment = UnityRandom.Range(6, 11);
                    float checkSpecific = UnityRandom.Range(0f, 1f);
                    ClassIndex classTarget = ClassIndex.None;
                    if (checkSpecific > (1 - 0.25f))
                    {
                        int randomClass = UnityRandom.Range(0, 3);
                        switch (randomClass)
                        {
                            case 0:
                                classTarget = ClassIndex.Cutlass;
                                break;
                            case 1:
                                classTarget = ClassIndex.Tracer;
                                break;
                            case 2:
                                classTarget = ClassIndex.Raygunner;
                                break;
                            case 3:
                                classTarget = ClassIndex.Hyperion;
                                break;
                        }
                    }
                    OnGetRandomizeShop?.Invoke(this, new Augment_Manager.NewAugmentShop { Augment = Augment_Manager.Instane.GenerateNewAugment(randomAugment, classTarget, TeamIndex.Ally) });
                }
                else
                {
                    int randomAugment = UnityRandom.Range(0, 6);
                    OnGetRandomizeShop?.Invoke(this, new Augment_Manager.NewAugmentShop { Augment = Augment_Manager.Instane.GenerateNewAugment(randomAugment, ClassIndex.None, TeamIndex.Ally) });
                }
            }
        }
    }

    public void RandomEnemyShop(float difficultyRatae)
    {
        UI_Manager.Instance.UpdateTicket(1);

        for (int i = 0; i < 2; i++)
        {
            int randomAugment = UnityRandom.Range(23, 31);
            OnGetRandomizeShop?.Invoke(this, new Augment_Manager.NewAugmentShop { Augment = Augment_Manager.Instane.GenerateNewAugment(randomAugment, ClassIndex.None, TeamIndex.Enemy) });
        }
        forceAugment = true;
    }
    #endregion

    private void OnDisable()
    {
        OnGetRandomizeShop -= UI_Manager.Instance.AddAugmentToShop;

        OnOpenShop -= UI_Manager.Instance.EnableShopUI;
        OnOpenUnitShop -= UI_Manager.Instance.EnableUnitShopUI;
        OnCloseShop -= UI_Manager.Instance.DisableShopUI;

        OnPurchaseAugment -= UI_Manager.Instance.ResetAugmentShop;
        OnPurchaseUnit -= UI_Manager.Instance.UpdateUnitShop;
        OnPurchaseUpgrade -= UI_Manager.Instance.UpdateUnitShop;
    }
}
