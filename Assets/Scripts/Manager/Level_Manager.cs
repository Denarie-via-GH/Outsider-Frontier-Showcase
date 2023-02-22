using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Outsider;
using UnityRandom = UnityEngine.Random;
using UnityEngine.Pool;

public class Level_Manager : MonoBehaviour
{
    public static Level_Manager Instance;
    public enum State { Standby, Active, Inactive}
    public State CurrentState = State.Standby;
    private Transform playerTransform;
    private Pathfinding pathfinding;
    private Augment_Manager Blackboard;

    [Header("Data Variable")]
    [SerializeField] private LayerMask Mask;
    [SerializeField] private List<UnitObject> unitData;
    public Dictionary<string, Queue<UnitBaseModule>> UnitRecycleDictionary;
    public List<Outsider.Pool> Pools;
    public List<String> UnitQueue = new List<string>();
    public List<GameObject> ItemPreset = new List<GameObject>();
    public Dictionary<string, UnitObject> PresetData;
    public event EventHandler<Personal.Utils.FloatArgs> OnUpdateTimer;
    public event EventHandler<Personal.Utils.SingleStringArgs> OnUpdateNotification;

    [Header("Global Variable")]
    public float difficultyScaling = 1;
    public float waveCleared = 0;
    public int maximumEnemy = 500;
    public int MinB, MaxB;

    [Header("Wave Variable")]
    [SerializeField] private float waveDelayTimer = 30f;
    [SerializeField] private float waveDifficultyScaling = 3f;
    [SerializeField] private float waveTimer = 0;
    [SerializeField] private float waveDuration = 180;
    [SerializeField] private float spawnRateScaling = 3;
    [SerializeField] private float timeDifficultyScaling = 0.01f;
    [SerializeField] private float elapsedTime = 0; 
    
    #region INITIATION FUNCTION
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
    public void Initiate()
    {
        OnUpdateTimer += UI_Manager.Instance.UpdateTimer;
        OnUpdateNotification += UI_Manager.Instance.UpdateNotification;

        UnitRecycleDictionary = new Dictionary<string, Queue<UnitBaseModule>>();
        PresetData = new Dictionary<string, UnitObject>();
        Blackboard = Augment_Manager.Instane;
        pathfinding = new Pathfinding(100, 100);
        ReadyUnitDataPreset();
        SpawnStarterUnit();
        ReadyUnitPool();

        if (Setting.difficultysetting == 0)
        {
            waveDifficultyScaling = 1.5f;
            timeDifficultyScaling = 0.008f;
        }
        else if (Setting.difficultysetting == 1)
        {
            waveDifficultyScaling = 3f;
            timeDifficultyScaling = 0.01f;
        }
        else if (Setting.difficultysetting == 2)
        {
            waveDifficultyScaling = 6f;
            timeDifficultyScaling = 0.02f;
        }

        //---> start game <---//
        bool isDebug = false;
        if (!isDebug)
            InitiateWaveCountdown();
        else
            CurrentState = State.Inactive;
    }
    public void SpawnStarterUnit() // special function for first wave only
    {
        Vector3 startOrigin = new Vector3(250 + pathfinding.GetGrid().GetCellSize() / 2, 250 + pathfinding.GetGrid().GetCellSize() / 2);
        CreateUnit("Player", startOrigin, TeamIndex.Ally);
        playerTransform = UnitController.Instance.gameObject.transform;

        switch (LoadoutMenu.loadoutIndex)
        {
            case 0:
                UnitController.Instance.GetComponent<Animator>().runtimeAnimatorController = Resources.Load("Controller/Player/L1/L1") as RuntimeAnimatorController;
                for(int c = 0; c < 6; c++)
                {
                    Vector3 offset = new Vector3(UnityRandom.Range(playerTransform.position.x-5, playerTransform.position.x+5), UnityRandom.Range(playerTransform.position.y - 5, playerTransform.position.y + 5));
                    CreateUnit("Cutlass", offset, TeamIndex.Ally);
                }
                for(int t = 0; t < 3; t++)
                {
                    Vector3 offset = new Vector3(UnityRandom.Range(playerTransform.position.x - 5, playerTransform.position.x + 5), UnityRandom.Range(playerTransform.position.y - 5, playerTransform.position.y + 5));
                    CreateUnit("Tracer", offset, TeamIndex.Ally);
                }
                for (int r = 0; r < 1; r++)
                {
                    Vector3 offset = new Vector3(UnityRandom.Range(playerTransform.position.x - 5, playerTransform.position.x + 5), UnityRandom.Range(playerTransform.position.y - 5, playerTransform.position.y + 5));
                    CreateUnit("Raygunner", offset, TeamIndex.Ally);
                }
                break;
            case 1:
                UnitController.Instance.GetComponent<Animator>().runtimeAnimatorController = Resources.Load("Controller/Player/L2/L2") as RuntimeAnimatorController;
                for (int c = 0; c < 3; c++)
                {
                    Vector3 offset = new Vector3(UnityRandom.Range(playerTransform.position.x - 5, playerTransform.position.x + 5), UnityRandom.Range(playerTransform.position.y - 5, playerTransform.position.y + 5));
                    CreateUnit("Cutlass", offset, TeamIndex.Ally);
                }
                for (int t = 0; t < 5; t++)
                {
                    Vector3 offset = new Vector3(UnityRandom.Range(playerTransform.position.x - 5, playerTransform.position.x + 5), UnityRandom.Range(playerTransform.position.y - 5, playerTransform.position.y + 5));
                    CreateUnit("Tracer", offset, TeamIndex.Ally);
                }
                for (int r = 0; r < 1; r++)
                {
                    Vector3 offset = new Vector3(UnityRandom.Range(playerTransform.position.x - 5, playerTransform.position.x + 5), UnityRandom.Range(playerTransform.position.y - 5, playerTransform.position.y + 5));
                    CreateUnit("Raygunner", offset, TeamIndex.Ally);
                }
                for (int h = 0; h < 1; h++)
                {
                    Vector3 offset = new Vector3(UnityRandom.Range(playerTransform.position.x - 5, playerTransform.position.x + 5), UnityRandom.Range(playerTransform.position.y - 5, playerTransform.position.y + 5));
                    CreateUnit("Hyperion", offset, TeamIndex.Ally);
                }
                break;
            case 2:
                UnitController.Instance.GetComponent<Animator>().runtimeAnimatorController = Resources.Load("Controller/Player/L3/L3") as RuntimeAnimatorController;
                for (int c = 0; c < 2; c++)
                {
                    Vector3 offset = new Vector3(UnityRandom.Range(playerTransform.position.x - 5, playerTransform.position.x + 5), UnityRandom.Range(playerTransform.position.y - 5, playerTransform.position.y + 5));
                    CreateUnit("Cutlass", offset, TeamIndex.Ally);
                }
                for (int t = 0; t < 3; t++)
                {
                    Vector3 offset = new Vector3(UnityRandom.Range(playerTransform.position.x - 5, playerTransform.position.x + 5), UnityRandom.Range(playerTransform.position.y - 5, playerTransform.position.y + 5));
                    CreateUnit("Tracer", offset, TeamIndex.Ally);
                }
                for (int r = 0; r < 4; r++)
                {
                    Vector3 offset = new Vector3(UnityRandom.Range(playerTransform.position.x - 5, playerTransform.position.x + 5), UnityRandom.Range(playerTransform.position.y - 5, playerTransform.position.y + 5));
                    CreateUnit("Raygunner", offset, TeamIndex.Ally);
                }
                for (int h = 0; h < 1; h++)
                {
                    Vector3 offset = new Vector3(UnityRandom.Range(playerTransform.position.x - 5, playerTransform.position.x + 5), UnityRandom.Range(playerTransform.position.y - 5, playerTransform.position.y + 5));
                    CreateUnit("Hyperion", offset, TeamIndex.Ally);
                }
                break;
            case 3:
                UnitController.Instance.GetComponent<Animator>().runtimeAnimatorController = Resources.Load("Controller/Player/L4/L4") as RuntimeAnimatorController;
                for (int c = 0; c < 3; c++)
                {
                    Vector3 offset = new Vector3(UnityRandom.Range(playerTransform.position.x - 5, playerTransform.position.x + 5), UnityRandom.Range(playerTransform.position.y - 5, playerTransform.position.y + 5));
                    CreateUnit("Cutlass", offset, TeamIndex.Ally);
                }
                for (int t = 0; t < 2; t++)
                {
                    Vector3 offset = new Vector3(UnityRandom.Range(playerTransform.position.x - 5, playerTransform.position.x + 5), UnityRandom.Range(playerTransform.position.y - 5, playerTransform.position.y + 5));
                    CreateUnit("Tracer", offset, TeamIndex.Ally);
                }
                for (int r = 0; r < 2; r++)
                {
                    Vector3 offset = new Vector3(UnityRandom.Range(playerTransform.position.x - 5, playerTransform.position.x + 5), UnityRandom.Range(playerTransform.position.y - 5, playerTransform.position.y + 5));
                    CreateUnit("Raygunner", offset, TeamIndex.Ally);
                }
                for (int h = 0; h < 3; h++)
                {
                    Vector3 offset = new Vector3(UnityRandom.Range(playerTransform.position.x - 5, playerTransform.position.x + 5), UnityRandom.Range(playerTransform.position.y - 5, playerTransform.position.y + 5));
                    CreateUnit("Hyperion", offset, TeamIndex.Ally);
                }
                break;
        }
    }
    #endregion

    #region UPDATE FUNCTION
    private void Update()
    {
        if (!GameManager.Instance.GamePaused)
        {
            if (Input.GetKeyDown(KeyCode.F) && CurrentState == State.Standby && waveDelayTimer >= 0)
            {
                StartNewWave();
            }
        }
    }
    private void FixedUpdate()
    {
        if (!GameManager.Instance.GamePaused)
        {
            switch (CurrentState)
            {
                case State.Active:
                    //---> update scaling factors <---//
                    elapsedTime += Time.deltaTime;
                    difficultyScaling = (waveCleared * waveDifficultyScaling) + (elapsedTime * timeDifficultyScaling);
                    spawnRateScaling = 1 + Mathf.Floor((0.25f * (Mathf.Pow(difficultyScaling, 1.25f))));
                    OnUpdateTimer?.Invoke(this, new Personal.Utils.FloatArgs { value = waveTimer });
                    GameManager.Instance.VerifyStaticTime(elapsedTime);

                    //---> update survive time <---//
                    if (waveTimer < waveDuration)
                    {
                        waveTimer += Time.deltaTime;
                        UpdateSpawn();
                    }
                    //---> if survive the wave and clear out all enemies, proceed to finish this wave <---//
                    else if (waveTimer >= waveDuration)
                    {
                        OnUpdateTimer?.Invoke(this, new Personal.Utils.FloatArgs { value = -1 });
                        CurrentState = State.Inactive;
                    }
                    break;
                case State.Standby:
                    //---> after finish augment shop, start count down next wave <---//
                    if (waveDelayTimer > 0 && !GameManager.Instance.isPreparing)
                    {
                        OnUpdateTimer?.Invoke(this, new Personal.Utils.FloatArgs { value = waveDelayTimer });
                        waveDelayTimer -= Time.deltaTime;
                        if (waveDelayTimer <= 0)
                        {
                            StartNewWave(); // start new wave
                        }
                    }
                    break;
                case State.Inactive:
                    OnUpdateNotification?.Invoke(this, new Personal.Utils.SingleStringArgs { value = "Remaining Enemy: " + UpdateEnemyRemain() });
                    if (UpdateEnemyRemain() == 0)
                    {
                        FinishWave();
                    }
                    break;
            }
        }
    }
    #endregion

    #region GET/SET FUNCTION
    private int UpdateEnemyRemain() // retrive remaining enemy count 
    {
        int enemyLimit = 500;
        Collider2D[] detectedArray = new Collider2D[enemyLimit];
        int numColliders = Physics2D.OverlapCircleNonAlloc(playerTransform.position, 500, detectedArray, Mask);

        return numColliders;
    }
    public float GetDifficultyScaling() // get current difficulty scaling 
    {
        return difficultyScaling;
    }
    public float GetSpawnRate()
    {
        return spawnRateScaling;
    }
    public void CreateUnit(string index, Vector3 position, TeamIndex teamIndex) // create new unit from template (modifiable) 
    {
        UnitObject modifiedDATA = ScriptableObject.CreateInstance<UnitObject>();
        modifiedDATA = PresetData[index];//ReplicateData(modifiedDATA, index);
        modifiedDATA.origin = position;
        modifiedDATA.teamIndex = teamIndex;

        UnitBaseModule createdUnit = Instantiate(modifiedDATA.unitPrefab, position, Quaternion.identity, transform.root).GetComponent<UnitBaseModule>();
        createdUnit.InitiateUnit(modifiedDATA, position, teamIndex);

        GameManager.Instance.VerifyStaticUnit();
    }
    public int CheckPlayerStress()
    {
        int stress = 0;
        int detectLimit = stressMaximum;
        Collider2D[] detectedArray = new Collider2D[detectLimit];
        int numColliders = Physics2D.OverlapCircleNonAlloc(playerTransform.position, 50, detectedArray, Mask);

        if (numColliders >= stressMaximum)
            stress = 1;
        else if (numColliders <= stressMinimum)
            stress = -1;

        return stress;
    }
    #endregion

    #region WAVE PROGRESSION
    public void InitiateWaveCountdown() // resume from shop and initiate next wave countdown process 
    {
        if (CurrentState == State.Standby)
        {
            OnUpdateNotification?.Invoke(this, new Personal.Utils.SingleStringArgs { value = "Standby Phase (F to skip)" });

            GameManager.Instance.isPreparing = false;
            GameManager.Instance.GamePaused = false;

            //---> reset all item in the map <---//
            GameObject[] items = GameObject.FindGameObjectsWithTag("Item");
            foreach (GameObject i in items)
            {
                GameObject.Destroy(i);
            }
            //---> ready new unit data and spawn new purchased unit <---//
            ReadyUnitDataPreset();
            foreach (string newUnit in UnitQueue)
            {
                CreateUnit(newUnit, playerTransform.position, TeamIndex.Ally);
            }
            UnitQueue.Clear();
            //---> upgrade all unit stat to scaling <---//
            UnitDFSModule[] allUnit = FindObjectsOfType<UnitDFSModule>();
            foreach (UnitDFSModule unit in allUnit)
            {
                UnitClassBase getClass = unit.GetComponent<UnitClassBase>();
                getClass.RelayUpdateStat();
            }
            playerTransform.GetComponent<UnitBaseModule>().InitiateUnit(PresetData["Player"], playerTransform.position, TeamIndex.Ally);

            waveDelayTimer = 30;
        }
    }
    public void StartNewWave() // start new wave, set wave engine to active 
    {
        if (CurrentState == State.Standby)
        {
            OnUpdateNotification?.Invoke(this, new Personal.Utils.SingleStringArgs { value = "Objective: Survive" });
            Audio_Manager.Instance.PlayGlobal(0);

            waveTimer = 0;
            InitiateWaveSpawner();
            CurrentState = State.Active;
        }
    }
    private void FinishWave() // call when finished wave, perform after match calculation and initiate augment shop 
    {
        if (CurrentState == State.Inactive)
        {
            //---> pause game after end wave <---//
            GameManager.Instance.isPreparing = true;
            GameManager.Instance.GamePaused = true;

            //---> calculate after match variable <---//
            waveCleared += 1;
            GameManager.Instance.VerifyStaticWave((int)waveCleared);
            difficultyScaling = (waveCleared * waveDifficultyScaling) + (elapsedTime * timeDifficultyScaling);
            spawnRateScaling = 1 + Mathf.Floor((0.25f * (Mathf.Pow(difficultyScaling, 1.25f))));
            CurrentState = State.Standby;

            //---> initiate shopping phase <---//
            Shop_Manager.Instance.OpenShop();
        }
    }
    #endregion

    #region SPAWN FUNCTION
    [Header("Spawner Variable")]
    private float spawnTimer;
    private float wallSpawnTimer;
    private float breakerSpawnTimer;
    private float surpriseSpawnTimer;
    private int breakerCounter = 0;
    private int breakerLimit = 2;
    private int nextSpawnWave;
    private int calamityCounter = 0;
    private int calamityLimit = 3;
    private int sentinelLimit = 2;
    [SerializeField] private int stressMaximum;
    [SerializeField] private int stressMinimum;
    [SerializeField] private int temporarySpawnRate;
    [SerializeField] private float tideTimer = 10;
    [SerializeField] private bool tideCooldown = true;
    public enum SpawnState { Low, Normal, High };
    public SpawnState CurrentTide = SpawnState.Normal;

    private void InitiateWaveSpawner()
    {
        tideTimer = 10;
        tideCooldown = true;
        breakerCounter = 0;
        calamityCounter = 0;
        calamityLimit = 3 + Mathf.RoundToInt((difficultyScaling / 10) / 3);
        sentinelLimit = 2 + Mathf.RoundToInt((difficultyScaling / 10) / 3);
        CurrentTide = SpawnState.Low;
        stressMaximum = Mathf.RoundToInt(spawnRateScaling) * 15;
        stressMinimum = Mathf.RoundToInt(stressMaximum * 0.2f);

        ResetNormalSpawn();
        ResetWallSpawn();
        ResetBreakerSpawn();
        ResetSurpriseSpawn();

        CreateSentinelStarter();
    }

    #region SPAWN DATA FUNCTION
    private void ReadyUnitPool() // spawn new unit and add to pool for ready to reactive
    {
        //---> prepare object pool for each unit type <---//
        foreach(Pool P in Pools)
        {
            //---> create unit instance and deactivate it <---//
            Queue<UnitBaseModule> unitPool = new Queue<UnitBaseModule>();
            for(int i = 0; i < P.size; i++)
            {
                UnitBaseModule obj = Instantiate(P.prefab).GetComponent<UnitBaseModule>();
                obj.isFromPool = true;
                obj.gameObject.SetActive(false);
                unitPool.Enqueue(obj);
            }

            UnitRecycleDictionary.Add(P.tag, unitPool);
        }
    }
    public UnitBaseModule SpawnUnitFromPool(string tag, Vector3 position) // get unit from pool and initiate it
    {
        UnitBaseModule reuseUnit = UnitRecycleDictionary[tag].Dequeue();
        reuseUnit.gameObject.SetActive(true);
        reuseUnit.InitiateUnit(PresetData[tag], position, TeamIndex.Enemy);
        return reuseUnit;
    }

    private void ReadyUnitDataPreset()
    {
        PresetData.Clear();

        #region Player
        UnitObject playDATA = ScriptableObject.CreateInstance<UnitObject>();
        playDATA = ReplicateData(playDATA, 0);
        PresetData.Add(playDATA.unitCode, playDATA);
        #endregion

        #region Cutlass
        UnitObject cutlassDATA = ScriptableObject.CreateInstance<UnitObject>();
        cutlassDATA = ReplicateData(cutlassDATA, 1);
        PresetData.Add(cutlassDATA.unitCode, cutlassDATA);
        #endregion

        #region Tracer
        UnitObject tracerDATA = ScriptableObject.CreateInstance<UnitObject>();
        tracerDATA = ReplicateData(tracerDATA, 2);
        PresetData.Add(tracerDATA.unitCode, tracerDATA);
        #endregion

        #region Raygunner
        UnitObject raygunnerDATA = ScriptableObject.CreateInstance<UnitObject>();
        raygunnerDATA = ReplicateData(raygunnerDATA, 3);
        PresetData.Add(raygunnerDATA.unitCode, raygunnerDATA);
        #endregion

        #region Hyperion
        UnitObject hyperionDATA = ScriptableObject.CreateInstance<UnitObject>();
        hyperionDATA = ReplicateData(hyperionDATA, 4);
        PresetData.Add(hyperionDATA.unitCode, hyperionDATA);
        #endregion

        #region Orbiter
        UnitObject orbiterDATA = ScriptableObject.CreateInstance<UnitObject>();
        orbiterDATA = ReplicateData(orbiterDATA, 5);
        PresetData.Add(orbiterDATA.unitCode, orbiterDATA);
        #endregion

        #region Sentinel
        UnitObject sentinelDATA = ScriptableObject.CreateInstance<UnitObject>();
        sentinelDATA = ReplicateData(sentinelDATA, 6);
        PresetData.Add(sentinelDATA.unitCode, sentinelDATA);
        #endregion

        #region Calamity
        UnitObject calamityDATA = ScriptableObject.CreateInstance<UnitObject>();
        calamityDATA = ReplicateData(sentinelDATA, 7);
        PresetData.Add(calamityDATA.unitCode, calamityDATA);
        #endregion

        //===> ELITE DATA <===//

        #region OrbiterElite
        UnitObject orbiterEliteDATA = ScriptableObject.CreateInstance<UnitObject>();
        orbiterEliteDATA = ReplicateData(orbiterEliteDATA, 8);
        PresetData.Add(orbiterEliteDATA.unitCode, orbiterEliteDATA);
        #endregion

        #region CutlassElite
        UnitObject cutlassEliteDATA = ScriptableObject.CreateInstance<UnitObject>();
        cutlassEliteDATA = ReplicateData(cutlassEliteDATA, 9);
        PresetData.Add(cutlassEliteDATA.unitCode, cutlassEliteDATA);
        #endregion

        #region TracerElite
        UnitObject tracerEliteDATA = ScriptableObject.CreateInstance<UnitObject>();
        tracerEliteDATA = ReplicateData(tracerEliteDATA, 10);
        PresetData.Add(tracerEliteDATA.unitCode, tracerEliteDATA);
        #endregion

        #region RaygunnerElite
        UnitObject raygunnerEliteDATA = ScriptableObject.CreateInstance<UnitObject>();
        raygunnerEliteDATA = ReplicateData(raygunnerEliteDATA, 11);
        PresetData.Add(raygunnerEliteDATA.unitCode, raygunnerEliteDATA);
        #endregion

        #region HyperionElite
        UnitObject hyperionEliteDATA = ScriptableObject.CreateInstance<UnitObject>();
        hyperionEliteDATA = ReplicateData(hyperionEliteDATA, 12);
        PresetData.Add(hyperionEliteDATA.unitCode, hyperionEliteDATA);
        #endregion
    }
    private UnitObject ReplicateData(UnitObject replicator, int index)
    {
        replicator.fromPool             = unitData[index].fromPool;
        replicator.unitCode             = unitData[index].unitCode;
        replicator.unitPrefab           = unitData[index].unitPrefab;
        replicator.unitMaxHP            = unitData[index].unitMaxHP + (unitData[index].hp_scaling * (waveCleared + 1));
        replicator.unitDMG              = unitData[index].unitDMG + (unitData[index].damage_scaling * (waveCleared + 1));
        replicator.unitRegen            = unitData[index].unitRegen + (unitData[index].regen_scaling * (waveCleared + 1));
        replicator.unitDEF              = unitData[index].unitDEF;
        replicator.unitCritRate         = unitData[index].unitCritRate;
        replicator.unitSpeed            = unitData[index].unitSpeed;
        replicator.unitDrop             = unitData[index].unitDrop * ((difficultyScaling / 2) + (1 + (0.1f * waveCleared)));
        replicator.teamIndex            = unitData[index].teamIndex;
        replicator.unitIframeDuration   = unitData[index].unitIframeDuration;

        if (Setting.difficultysetting == 0 && replicator.teamIndex == TeamIndex.Enemy)
        {
            replicator.unitDrop     *= 1.5f;
            replicator.unitMaxHP    *= 0.75f;
            replicator.unitDMG      *= 0.5f;
        }
        else if (Setting.difficultysetting == 1 && replicator.teamIndex == TeamIndex.Enemy)
        {
            replicator.unitDrop     *= 0.9f;
            replicator.unitMaxHP    *= 1.2f;
            replicator.unitDMG      *= 1.1f;
        }
        else if (Setting.difficultysetting == 2 && replicator.teamIndex == TeamIndex.Enemy)
        {
            replicator.unitDrop     *= 0.75f;
            replicator.unitMaxHP    *= 1.45f;
            replicator.unitDMG      *= 1.2f;
        }

        foreach (AugmentDef augment in Blackboard.Augments)
        {
            if (augment.augmentIndex == 28 && replicator.teamIndex == TeamIndex.Enemy)
            {
                replicator.unitMaxHP += 50;
            }
        }
        foreach (AugmentDef augment in Blackboard.Augments)
        {
            if (augment.augmentIndex == 29 && replicator.teamIndex == TeamIndex.Enemy)
            {
                replicator.unitDMG += 5;
            }
        }
        foreach (AugmentDef augment in Blackboard.Augments)
        {
            if (augment.augmentIndex == 30 && replicator.teamIndex == TeamIndex.Enemy)
            {
                replicator.unitDEF += 2.5f;
            }
        }

        return replicator;
    }
    #endregion

    private void UpdateSpawn()
    {
        //---> spawn normal enemies randomly in intervals <---//
        #region SPAWN NORMAL
        if (spawnTimer > 0)
        {
            spawnTimer -= Time.deltaTime;
            if(spawnTimer <= 0)
            {
                for (int i = 0; i < nextSpawnWave; i++)
                {
                    Vector3 destination = Vector3.zero;
                    int randomDirection = UnityRandom.Range(0, 4);
                    switch (randomDirection)
                    {
                        case 0: // left
                            destination = new Vector3(UnityRandom.Range(playerTransform.position.x - 70,playerTransform.position.x - 80), UnityRandom.Range(playerTransform.position.y - 50, playerTransform.position.y + 50), 0);
                            destination.x = Mathf.Clamp(destination.x, MinB, MaxB);
                            destination.y = Mathf.Clamp(destination.y, MinB, MaxB);
                            break;
                        case 1: // right
                            destination = new Vector3(UnityRandom.Range(playerTransform.position.x + 70, playerTransform.position.x + 80), UnityRandom.Range(playerTransform.position.y - 50, playerTransform.position.y + 50), 0);
                            destination.x = Mathf.Clamp(destination.x, MinB, MaxB);
                            destination.y = Mathf.Clamp(destination.y, MinB, MaxB);
                            break;
                        case 2: // top
                            destination = new Vector3(UnityRandom.Range(playerTransform.position.x - 80, playerTransform.position.x + 80), UnityRandom.Range(playerTransform.position.y + 40, playerTransform.position.y + 50), 0);
                            destination.x = Mathf.Clamp(destination.x, MinB, MaxB);
                            destination.y = Mathf.Clamp(destination.y, MinB, MaxB);
                            break;
                        case 3: // bottm
                            destination = new Vector3(UnityRandom.Range(playerTransform.position.x - 80, playerTransform.position.x + 80), UnityRandom.Range(playerTransform.position.y - 40, playerTransform.position.y - 50), 0);
                            destination.x = Mathf.Clamp(destination.x, MinB, MaxB);
                            destination.y = Mathf.Clamp(destination.y, MinB, MaxB);
                            break;
                    }
                    UnitBaseModule getUnit = SpawnUnitFromPool("Orbiter", destination);
                    RandomOrbiterElite(getUnit);
                }

                ResetNormalSpawn();
            }
        }
        #endregion

        //---> spawn enemy as wall formation <---//
        #region SPAWN WALL FORMATION
        if (wallSpawnTimer > 0)
        {
            wallSpawnTimer -= Time.deltaTime;
            if(wallSpawnTimer <= 0)
            {
                Vector3 leftWall = new Vector3(playerTransform.position.x - 80, playerTransform.position.y, 0);
                leftWall.x = Mathf.Clamp(leftWall.x, MinB, MaxB);
                leftWall.y = Mathf.Clamp(leftWall.y, MinB, MaxB);
                CreateWallOfEnemy(leftWall);
                
                Vector3 rightWall = new Vector3(playerTransform.position.x + 80, playerTransform.position.y, 0);
                rightWall.x = Mathf.Clamp(rightWall.x, MinB, MaxB);
                rightWall.y = Mathf.Clamp(rightWall.y, MinB, MaxB);
                CreateWallOfEnemy(rightWall);
                
                Vector3 topWall = new Vector3(playerTransform.position.x, playerTransform.position.y + 50, 0);
                topWall.x = Mathf.Clamp(topWall.x, MinB, MaxB);
                topWall.y = Mathf.Clamp(topWall.y, MinB, MaxB);
                CreateWallOfEnemy(topWall);
                
                Vector3 bottomWall = new Vector3(playerTransform.position.x, playerTransform.position.y - 50, 0);
                bottomWall.x = Mathf.Clamp(bottomWall.x, MinB, MaxB);
                bottomWall.y = Mathf.Clamp(bottomWall.y, MinB, MaxB);
                CreateWallOfEnemy(bottomWall);

                CreateCalamity();
                ResetWallSpawn();
            }
        }
        #endregion

        //---> Spawn enemy as attacking formation <---//
        #region SPAWN BREAK FORMATION
        if (breakerSpawnTimer > 0 && breakerCounter < breakerLimit)
        {
            breakerSpawnTimer -= Time.deltaTime;
            if(breakerSpawnTimer <= 0)
            {
                float Chance = UnityRandom.Range(0f, 1f);
                //---> randomize spawn chance at normal stress (higher chacne at low stress) <---//
                if (Chance > GetBreakerChance())
                {
                    CreateBreakerFormation();
                }
                CreateCalamity();
                ResetBreakerSpawn();
            }
        }
        #endregion

        //---> Spawn enemy close to player <---//
        #region SURPRISE SPAWN
        if (surpriseSpawnTimer > 0)
        {
            surpriseSpawnTimer -= Time.deltaTime;
            if(surpriseSpawnTimer <= 0)
            {
                float Chance = UnityRandom.Range(0f, 1f);
                float SpawnMax = (temporarySpawnRate / 2);
                if (Chance > GetSurpriseChance())
                {
                    for(int i = 0; i < SpawnMax; i++)
                    {   
                        float RandX = UnityRandom.Range(playerTransform.position.x - 50, playerTransform.position.x + 40);
                        float RandY = UnityRandom.Range(playerTransform.position.y - 50, playerTransform.position.y + 40);
                        UnitBaseModule getUnit = SpawnUnitFromPool("Orbiter", new Vector3(RandX,RandY));
                        RandomOrbiterElite(getUnit);
                    }
                }
                CreateCalamity();
                ResetSurpriseSpawn();
            }
        }
        #endregion

        //---> manage spawn tide rate depending on stress <---//
        #region TIDE COOLDOWN 
        if (tideTimer > 0)
        {
            tideTimer -= Time.deltaTime;
            if(tideTimer <= 0)
            {
                tideCooldown = false;
            }
        }
        #endregion

        #region UPDATE TIDE
        if (!tideCooldown)
        {
            int CheckStress = CheckPlayerStress();
            if (CheckStress == 1 && CurrentTide != SpawnState.Low)
                SwitchTide(SpawnState.Low);
            else if (CheckStress == 0 && CurrentTide != SpawnState.Normal)
                SwitchTide(SpawnState.Normal);
            else if (CheckStress == -1 && CurrentTide != SpawnState.High)
                SwitchTide(SpawnState.High);
        }
        #endregion
    }


    private void SwitchTide(SpawnState newTide)
    {
        tideTimer = 10;
        tideCooldown = true;
        CurrentTide = newTide;
    }

    #region RESET FUNCTION
    private void ResetNormalSpawn()
    {
        float randomSpawnInterval = UnityRandom.Range(1.25f, 5f);
        spawnTimer = randomSpawnInterval;

        if (CurrentTide == SpawnState.Low)
        {
            temporarySpawnRate = Mathf.RoundToInt(spawnRateScaling * 0.5f);
            nextSpawnWave = Mathf.RoundToInt(temporarySpawnRate * randomSpawnInterval);
            
        }
        else if (CurrentTide == SpawnState.Normal)
        {
            temporarySpawnRate = Mathf.RoundToInt(spawnRateScaling * 1);
            nextSpawnWave = Mathf.RoundToInt(temporarySpawnRate * randomSpawnInterval);
        }
        else if(CurrentTide == SpawnState.High)
        {
            temporarySpawnRate = Mathf.RoundToInt(spawnRateScaling * 1.5f);
            nextSpawnWave = Mathf.RoundToInt(temporarySpawnRate * randomSpawnInterval);
        }

        stressMaximum = Mathf.RoundToInt(spawnRateScaling) * 15;
        stressMinimum = Mathf.RoundToInt(stressMaximum * 0.2f);
    }
    private void ResetWallSpawn()
    {
        wallSpawnTimer = 45;
    }
    private void ResetBreakerSpawn()
    {
        breakerSpawnTimer = 30;
    }
    private void ResetSurpriseSpawn()
    {
        surpriseSpawnTimer = 35;
    }

    private float GetBreakerChance()
    {
        float chance = 0.9f;
        if (CheckPlayerStress() == 0) 
        {
            chance = Mathf.Clamp(0.9f - (Mathf.Pow(difficultyScaling, 1.25f) * 0.002f), 0.1f, 1f);
        }
        else if (CheckPlayerStress() == -1)
        {
            chance = Mathf.Clamp(0.7f - (Mathf.Pow(difficultyScaling, 1.25f) * 0.003f), 0.1f, 1f);
        }
        else if(waveCleared + 1 % 10 == 0)
        {
            chance = 0f;
        }

        return chance;
    }
    private float GetSurpriseChance()
    {
        float chance = Mathf.Clamp(0.75f - (Mathf.Pow(difficultyScaling, 1.25f) * 0.0015f), 0.1f, 1f);

        #region SURPRISE AUGMENT
        foreach (AugmentDef augment in Blackboard.Augments)
        {
            if (augment.augmentIndex == 26)
            {
                chance -= 0.05f;
            }
        }
        #endregion
        return chance;
    }
    #endregion

    private void CreateWallOfEnemy(Vector3 position)
    {
        Vector3 StartPOS = position;
        Vector2 DIR = playerTransform.position - StartPOS;
        float angle = (Vector2.SignedAngle(DIR, new Vector2(0, 1)) * -1);
        List<Vector2> RequestFormation = UnitMaster_Manager.Instance.RequestLine(angle, StartPOS, 7.5f, 15, 10 + Mathf.RoundToInt(temporarySpawnRate * 0.75f));

        foreach (Vector2 target in RequestFormation)
        {
            UnitBaseModule getUnit = SpawnUnitFromPool("Orbiter", target);
            RandomOrbiterElite(getUnit);
        }
    }
    private void CreateBreakerFormation()
    {
        breakerCounter++;
        Vector3 destination = Vector3.zero;
        int randomDirection = UnityRandom.Range(0, 3);
        switch (randomDirection)
        {
            case 0: // left
                destination = new Vector3(playerTransform.position.x - 100, UnityRandom.Range(playerTransform.position.y - 50, playerTransform.position.y + 50), 0);
                destination.x = Mathf.Clamp(destination.x, MinB, MaxB);
                destination.y = Mathf.Clamp(destination.y, MinB, MaxB);
                break;
            case 1: // right
                destination = new Vector3(playerTransform.position.x + 100, UnityRandom.Range(playerTransform.position.y - 50, playerTransform.position.y + 50), 0);
                destination.x = Mathf.Clamp(destination.x, MinB, MaxB);
                destination.y = Mathf.Clamp(destination.y, MinB, MaxB);
                break;
            case 2: // top
                destination = new Vector3(UnityRandom.Range(playerTransform.position.x - 100, playerTransform.position.x + 100), playerTransform.position.y + 50, 0);
                destination.x = Mathf.Clamp(destination.x, MinB, MaxB);
                destination.y = Mathf.Clamp(destination.y, MinB, MaxB);
                break;
            case 3: // down
                destination = new Vector3(UnityRandom.Range(playerTransform.position.x - 100, playerTransform.position.x + 100), playerTransform.position.y - 50, 0);
                destination.x = Mathf.Clamp(destination.x, MinB, MaxB);
                destination.y = Mathf.Clamp(destination.y, MinB, MaxB);
                break;
        }
        Vector2 DIR = playerTransform.position - destination;
        float angle = (Vector2.SignedAngle(DIR, new Vector2(0, 1)) * -1);
        int units = 20 + Mathf.RoundToInt(temporarySpawnRate);

        #region BREAKER AUGMENT
        foreach (AugmentDef augment in Blackboard.Augments)
        {
            if (augment.augmentIndex == 27)
            {
                units += 5;
            }
        }
        #endregion

        List<Vector2> RequestFormation = UnitMaster_Manager.Instance.RequestWing(angle, destination, 2.5f, 10, units);
        foreach (Vector2 target in RequestFormation)
        {
            UnitBaseModule getUnit = SpawnUnitFromPool("Orbiter", target);
            RandomOrbiterElite(getUnit);
        }
    }
    private void CreateSentinelStarter()
    {
        if((waveCleared + 1) % 3 == 0)
        {
            for (int i = 0; i < sentinelLimit; i++)
            {
                //---> random chance to spawn sentinel <---//
                float check = UnityRandom.Range(0f, 1f);
                float chance = Mathf.Clamp(0.5f - (Mathf.Pow(difficultyScaling, 1.25f) * 0.0025f), 0.25f, 0.95f);
                //---> if chance passed then spawn sentinel <---//
                if (check >= chance)
                {
                    float RandX = UnityRandom.Range(MinB, MaxB);
                    float RandY = UnityRandom.Range(MinB, MaxB);
                    Vector3 targetPOS = new Vector3(RandX, RandY, 0);
                    for (int x = 0; x < 15 + temporarySpawnRate; x++)
                    {
                        UnitBaseModule newUnit = SpawnUnitFromPool("Orbiter", targetPOS);
                        UnitOrbiterModule getOrbiter = newUnit.GetComponent<UnitOrbiterModule>();
                        getOrbiter.UpgradeElite();
                    }
                    Audio_Manager.Instance.PlayGlobal(2);
                    SpawnUnitFromPool("Sentinel", targetPOS);
                }
            }
        }
    }
    private void CreateCalamity()
    {
        float check = UnityRandom.Range(0f, 1f);
        float chance = Mathf.Clamp(0.9f - (Mathf.Pow(difficultyScaling, 1.25f) * 0.0025f), 0.35f, 0.95f);

        if (check >= chance && calamityCounter < calamityLimit && waveCleared > 2)
        {
            float RandX = UnityRandom.Range(MinB, MaxB);
            float RandY = UnityRandom.Range(MinB, MaxB);
            Vector3 targetPOS = new Vector3(RandX, RandY, 0);
            for (int i = 0; i < 15 + temporarySpawnRate; i++)
            {
                UnitBaseModule newUnit = SpawnUnitFromPool("Orbiter", targetPOS);
                UnitOrbiterModule getOrbiter = newUnit.GetComponent<UnitOrbiterModule>();
                getOrbiter.UpgradeElite();
            }
            Audio_Manager.Instance.PlayGlobal(3);
            SpawnUnitFromPool("Calamity", targetPOS);
            calamityCounter++;
        }
    }
    #endregion

    public void RandomOrbiterElite(UnitBaseModule unit)
    {
        float check = UnityRandom.Range(0f, 1f);
        float chance = Mathf.Clamp(0.9f - (Mathf.Pow(difficultyScaling, 1.25f) * 0.005f),0.1f,1f);
        if (check >= chance)
        {
            UnitOrbiterModule getOrbiter = unit.GetComponent<UnitOrbiterModule>();
            getOrbiter.UpgradeElite();
        }
    }
    public void SpawnItem(Vector3 position)
    {
        Instantiate(ItemPreset[UnityRandom.Range(0, 5)],position,Quaternion.identity).GetComponent<ItemBase>();
    }
    public void ResetAllEnemy()
    {
        GameObject[] enemyArrays = GameObject.FindGameObjectsWithTag("Unit");
        foreach(GameObject enemy in enemyArrays)
        {
            UnitBaseModule baseUnit = enemy.GetComponent<UnitBaseModule>();
            if(baseUnit.teamIndex == TeamIndex.Enemy)
            {
                baseUnit.UnitDead();
            }
        }
    }

    private void OnDisable()
    {
        OnUpdateTimer -= UI_Manager.Instance.UpdateTimer;
        OnUpdateNotification -= UI_Manager.Instance.UpdateNotification;
    }
}
