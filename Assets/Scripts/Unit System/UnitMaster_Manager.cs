using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Outsider;
using Personal.Utils;
using UnityEngine.InputSystem;
using UnityRandom = UnityEngine.Random;

public class UnitMaster_Manager : MonoBehaviour
{
    public static UnitMaster_Manager Instance;
    public enum Formation { Skirmish, Ring, Wing, Line, Box}
    public Formation SelectFormation;
    public bool isSelectingFormation;

    public event EventHandler<SelectTargetArgs> OnSelectTarget;
    public event EventHandler<Personal.Utils.SingleBoolArgs> OnFormationDisplay;
    public class SelectTargetArgs : EventArgs
    {
        public List<UnitDFSModule> selectedUnit;
    }

    public Vector3 startPOS;
    private List<UnitDFSModule> selectedUnitList;
    private UnitController playerUnit;
    private ControlScheme Controller;

    [SerializeField] public List<UnitDFSModule> Team_1;
    [SerializeField] public List<UnitDFSModule> Team_2;
    [SerializeField] public List<UnitDFSModule> Team_3;
    [SerializeField] public List<UnitDFSModule> Team_4;
    [SerializeField] public List<UnitDFSModule> Team_5;

    private Vector2 RefPoint;
    private Vector2 TargetPoint;
    public int MinB, MaxB;

    [Header("Raycasting")]
    public LayerMask Mask;
    public RaycastHit2D Hit;

    [SerializeField] private Transform SelectDisplayTransform;

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
        playerUnit = UnitController.Instance;
        SelectDisplayTransform.gameObject.SetActive(false);
        selectedUnitList = new List<UnitDFSModule>();
        Controller = new ControlScheme();

        OnFormationDisplay += UI_Manager.Instance.SignalFormationDisplay;
        OnSelectTarget += UI_Manager.Instance.SignalUnitInfo;

        Controller.Basic.Enable();

        Controller.Basic.CreateTeamOne.performed += ctx => CreateTeam(0);
        Controller.Basic.CreateTeamTwo.performed += ctx => CreateTeam(1);
        Controller.Basic.CreateTeamThree.performed += ctx => CreateTeam(2);
        Controller.Basic.CreateTeamFour.performed += ctx => CreateTeam(3);
        Controller.Basic.CreateTeamFive.performed += ctx => CreateTeam(4);

        Controller.Basic.SelectTeamOne.performed += ctx => SelectTeam(0);
        Controller.Basic.SelectTeamTwo.performed += ctx => SelectTeam(1);
        Controller.Basic.SelectTeamThree.performed += ctx => SelectTeam(2);
        Controller.Basic.SelectTeamFour.performed += ctx => SelectTeam(3);
        Controller.Basic.SelectTeamFive.performed += ctx => SelectTeam(4);

        Controller.Basic.ShowSelector.performed += ctx => DisplaySelector();
        Controller.Basic.Deselect.performed += ctx => DeselectAll();
    }
    void Update()
    {
        if (!GameManager.Instance.GamePaused)
        {
            #region LEFT MOUSE
            if (Input.GetMouseButtonDown(0))
            {
                if (isSelectingFormation)
                    return;
                SelectDisplayTransform.gameObject.SetActive(true);
                startPOS = Utility.GetMouseWorldPOS();
            }

            if (Input.GetMouseButton(0))
            {
                if (isSelectingFormation)
                    return;
                Vector3 currentMousePosition = Utility.GetMouseWorldPOS();
                Vector3 lowerLeft = new Vector3(
                    Mathf.Min(startPOS.x, currentMousePosition.x),
                    Mathf.Min(startPOS.y, currentMousePosition.y)
                    );
                Vector3 upperRight = new Vector3(
                    Mathf.Max(startPOS.x, currentMousePosition.x),
                    Mathf.Max(startPOS.y, currentMousePosition.y)
                    );

                SelectDisplayTransform.position = lowerLeft;
                SelectDisplayTransform.localScale = upperRight - lowerLeft;
            }
            if (Input.GetMouseButtonUp(0))
            {
                if (isSelectingFormation)
                    return;
                SelectDisplayTransform.gameObject.SetActive(false);

                Collider2D[] collider2DArray = Physics2D.OverlapAreaAll(startPOS, Utility.GetMouseWorldPOS());

                selectedUnitList.RemoveAll(delegate (UnitDFSModule x) { return x == null; });
                foreach (UnitDFSModule Unit in selectedUnitList)
                {
                    Unit.SetSelectedVisible(false);
                }
                selectedUnitList.Clear();

                foreach (Collider2D Col in collider2DArray)
                {
                    UnitDFSModule Unit = Col.GetComponent<UnitDFSModule>();
                    if (Unit != null)
                    {
                        Unit.SetSelectedVisible(true);
                        selectedUnitList.Add(Unit);
                    }
                }
                OnSelectTarget?.Invoke(this, new SelectTargetArgs { selectedUnit = selectedUnitList });
            }
            #endregion

            #region SELECT FORMATION
            if (Input.GetKeyDown(KeyCode.C))
            {
                SelectFormation = Formation.Ring;
            }
            if (Input.GetKeyDown(KeyCode.V))
            {
                SelectFormation = Formation.Wing;
            }
            if (Input.GetKeyDown(KeyCode.Z))
            {
                SelectFormation = Formation.Line;
            }
            if (Input.GetKeyDown(KeyCode.B))
            {
                SelectFormation = Formation.Box;
            }
            if (Input.GetKeyDown(KeyCode.X))
            {
                SelectFormation = Formation.Skirmish;
            }
            #endregion

            #region BUTTONS
            if (Input.GetKeyDown(KeyCode.Q))
            {
                if (selectedUnitList != null || selectedUnitList.Count > 0)
                {
                    foreach (UnitDFSModule Unit in selectedUnitList)
                    {
                        Unit.SetCoordinateMode();
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.U))
            {
                SelectUnitType(0);
            }
            if (Input.GetKeyDown(KeyCode.I))
            {
                SelectUnitType(1);
            }
            if (Input.GetKeyDown(KeyCode.O))
            {
                SelectUnitType(2);
            }
            if (Input.GetKeyDown(KeyCode.P))
            {
                SelectUnitType(3);
            }
            #endregion

            #region RIGHT MOUSE
            if (Input.GetMouseButtonDown(1))
            {
                TargetPoint = Utility.GetMouseWorldPOS();
                TargetPoint.x = Mathf.Clamp(TargetPoint.x, MinB, MaxB);
                TargetPoint.y = Mathf.Clamp(TargetPoint.y, MinB, MaxB);
            }
            //---> move unit to mouse position <---//
            if (Input.GetMouseButtonUp(1))
            {
                if (isSelectingFormation)
                {
                    isSelectingFormation = false;
                    OnFormationDisplay?.Invoke(this, new Personal.Utils.SingleBoolArgs { value = isSelectingFormation });
                }
                //---------> calculate wing formation direction <------------//
                RefPoint = Utility.GetMouseWorldPOS();
                Vector2 DIR = RefPoint - TargetPoint;
                float angle = (Vector2.SignedAngle(DIR, new Vector2(0, 1)) * -1);
                selectedUnitList.RemoveAll(delegate (UnitDFSModule x) { return x == null; });
                //------------------------------------------------------//

                List<Vector2> finalFormation = new List<Vector2>();
                switch (SelectFormation)
                {
                    case (Formation.Ring):
                        finalFormation = RequestCircle(angle, TargetPoint, 5, 10, selectedUnitList.Count); ;
                        break;
                    case (Formation.Wing):
                        finalFormation = RequestWing(angle,TargetPoint, 3f, 10, selectedUnitList.Count);
                        break;
                    case (Formation.Line):
                        finalFormation = RequestLine(angle, TargetPoint, 2.5f, 10, selectedUnitList.Count);
                        break;
                    case (Formation.Box):
                        finalFormation = RequestBox(TargetPoint,2.5f,5,selectedUnitList.Count);
                        break;
                    case (Formation.Skirmish):
                        finalFormation = RequestSkirmish(TargetPoint,10,10,selectedUnitList.Count);
                        break;
                }

                int targetPositionListIndex = 0;

                //---> select multiple ally unit <---//
                if (selectedUnitList.Count > 1)
                {
                    foreach (UnitDFSModule Unit in selectedUnitList)
                    {
                        Unit.SetPathfinding(finalFormation[targetPositionListIndex], true);
                        targetPositionListIndex = (targetPositionListIndex + 1) % finalFormation.Count;
                    }
                }
                //---> select single ally unit <---//
                else if (selectedUnitList.Count == 1)
                {
                    foreach (UnitDFSModule Unit in selectedUnitList)
                    {
                        //Debug.Log("Not in formation");
                        Unit.SetPathfinding(TargetPoint, false);
                    }
                }
                //---> release coordinate on focused target <---//
                else if (selectedUnitList.Count <= 0)
                {
                    Hit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition), Mathf.Infinity, Mask);
                    if (Hit.collider != null)
                    {
                        UnitBaseModule targetUnit = Hit.collider.gameObject.GetComponent<UnitBaseModule>();
                        UnitDFSModule[] CallCoordinate = FindObjectsOfType<UnitDFSModule>();
                        foreach (UnitDFSModule Unit in CallCoordinate)
                        {
                            UnitBaseModule UnitBase = Unit.GetComponent<UnitBaseModule>();
                            if (UnitBase.isCoordinated)
                            {
                                UnitBase.ReleaseCoordinate(targetUnit);
                            }
                        }
                    }
                }
            }
            #endregion
        }
    }
    #region REQUEST FORMATION
    public List<Vector2> RequestCircle(float angle, Vector2 targetPOS, float requestDistance, int requestUnit, int availableUnit)
    {
        List<Vector2> NewRingFormation = new List<Vector2>();
        // 5 -> 10 -> 11
        int RankLevel = 1;
        int UnitLevel = 0;
        int RemainingUnit = availableUnit;
        List<float> storedRank = new List<float>();
        List<int> storedUnit = new List<int>();
        for (int Z = RemainingUnit; Z >= requestUnit * RankLevel; Z -= requestUnit * UnitLevel)
        {
            storedRank.Add(requestDistance * RankLevel);
            storedUnit.Add(requestUnit * RankLevel);

            RankLevel += 1;
            UnitLevel += 1;
            RemainingUnit -= requestUnit * UnitLevel;
        }
        if (RemainingUnit >= 1)
        {
            storedRank.Add(requestDistance * RankLevel);
            storedUnit.Add(RemainingUnit);
        }
        NewRingFormation = SetRingFormation(angle, targetPOS, storedRank, storedUnit);
        return NewRingFormation;
    }
    public List<Vector2> RequestWing(float angle, Vector2 targetPOS, float requestDistance, int requestUnit, int availableUnit)
    {
        List<Vector2> NewWingFormation = new List<Vector2>();
        int W_RemainingUnit = availableUnit;
        List<float> W_storedRank = new List<float>();
        List<int> W_storedUnit = new List<int>();
        for (int W = W_RemainingUnit; W >= requestUnit; W -= requestUnit)
        {
            W_storedRank.Add(requestDistance);
            W_storedUnit.Add(requestUnit);
            W_RemainingUnit -= requestUnit;
        }
        if (W_RemainingUnit >= 1)
        {
            W_storedRank.Add(requestDistance);
            W_storedUnit.Add(W_RemainingUnit);
        }
        NewWingFormation = SetWingFormation(angle, targetPOS, W_storedRank, W_storedUnit);
        return NewWingFormation;
    }
    public List<Vector2> RequestLine(float angle, Vector2 targetPOS, float requestDistance, int requestUnit, int availableUnit)
    {
        List<Vector2> NewLineFormation = new List<Vector2>();
        int L_RemainingUnit = availableUnit;
        List<float> L_storedRank = new List<float>();
        List<int> L_storedUnit = new List<int>();
        for (int L = L_RemainingUnit; L >= requestUnit; L -= requestUnit)
        {
            L_storedRank.Add(requestDistance);
            L_storedUnit.Add(requestUnit);
            L_RemainingUnit -= requestUnit;
        }
        if (L_RemainingUnit >= 1)
        {
            L_storedRank.Add(requestDistance);
            L_storedUnit.Add(L_RemainingUnit);
        }
        NewLineFormation = SetLineFormation(angle, targetPOS, L_storedRank, L_storedUnit);
        return NewLineFormation;
    }
    public List<Vector2> RequestBox(Vector2 targetPOS, float requestDistance, int requestUnit, int availableUnit)
    {
        List<Vector2> NewBoxFormation = new List<Vector2>();
        int B_RemainingUnit = availableUnit;
        List<float> B_storedRank = new List<float>();
        List<int> B_storedUnit = new List<int>();
        for (int B = B_RemainingUnit; B >= requestUnit; B -= requestUnit)
        {
            B_storedRank.Add(requestDistance);
            B_storedUnit.Add(requestUnit);
            B_RemainingUnit -= requestUnit;
        }
        if (B_RemainingUnit >= 1)
        {
            B_storedRank.Add(requestDistance);
            B_storedUnit.Add(B_RemainingUnit);
        }
        NewBoxFormation = SetBoxFormation(targetPOS, B_storedRank, B_storedUnit);
        return NewBoxFormation;
    }
    public List<Vector2> RequestSkirmish(Vector2 targetPOS, float requestDistance, int requestUnit, int availableUnit)
    {
        List<Vector2> NewSkirmishFormation = new List<Vector2>();
        int S_RankLevel = 1;
        int S_UnitLevel = 0;
        int S_RemainingUnit = availableUnit;
        List<float> S_storedRank = new List<float>();
        List<int> S_storedUnit = new List<int>();
        for (int S = S_RemainingUnit; S >= requestUnit * S_RankLevel; S -= requestUnit * S_UnitLevel)
        {
            S_storedRank.Add(requestDistance * S_RankLevel);
            S_storedUnit.Add(requestUnit * S_RankLevel);

            S_RankLevel += 1;
            S_UnitLevel += 1;
            S_RemainingUnit -= requestUnit * S_UnitLevel;
        }
        if (S_RemainingUnit >= 1)
        {
            S_storedRank.Add(requestDistance * S_RankLevel);
            S_storedUnit.Add(S_RemainingUnit);
        }
        NewSkirmishFormation = SetSkirmishFormation(targetPOS, S_storedRank, S_storedUnit);
        return NewSkirmishFormation;
    }
    #endregion

    #region FORMATION FUCNTION
    public List<Vector2> SetBoxFormation(Vector2 basePosition, List<float> unitDistanceArray, List<int> unitPositionCountArray)
    {
        List<Vector2> targetPositionList = new List<Vector2>();
        int baseRank = 0;
        for (int i = 0; i < unitDistanceArray.Count; i++)
        {
            Vector2 newBase = basePosition + (new Vector2(0, -1) * baseRank * 2.5f);
            targetPositionList.AddRange(SetLineFormation(0, newBase, unitDistanceArray[i], unitPositionCountArray[i]));

            baseRank++;
        }
        return targetPositionList;
    }
    public List<Vector2> SetRingFormation(float direction, Vector2 basePosition, List<float> ringDistanceArray, List<int> ringPositionCountArray)
    {
        List<Vector2> targetPositionList = new List<Vector2>();
        for (int i = 0; i < ringDistanceArray.Count; i++)
        {
            targetPositionList.AddRange(SetRingFormation(direction, basePosition, ringDistanceArray[i], ringPositionCountArray[i]));
        }
        return targetPositionList;
    }
    public List<Vector2> SetRingFormation(float direction, Vector2 basePosition, float ringDistance, int unitCount)
    {
        List<Vector2> targetPositionList = new List<Vector2>();

        for(int i = 0; i < unitCount; i++)
        {
            float angle = direction + (i * (360f / unitCount));

            Vector2 dir = ReturnDegreeRotationToVector(new Vector2(1, 0), angle);
            Vector2 position = basePosition + dir * ringDistance;
            targetPositionList.Add(position);
        }

        return targetPositionList;
    }
    public List<Vector2> SetWingFormation(float direction, Vector2 basePosition, List<float> wingDistanceArray, List<int> wingPositionCountArray)
    {
        List<Vector2> targetPositionList = new List<Vector2>();
        int baseRank = 0;
        for (int i = 0; i < wingDistanceArray.Count; i++)
        {

            Vector2 dir = ReturnDegreeRotationToVector(new Vector2(0, -1), direction);
            Vector2 newBase = basePosition + (dir * 5f * baseRank);

            targetPositionList.AddRange(SetWingFormation(direction, newBase, wingDistanceArray[i], wingPositionCountArray[i]));

            baseRank++;
        }
        return targetPositionList;
    }
    public List<Vector2> SetWingFormation(float direction, Vector2 basePosition, float wingDistance, int unitCount)
    {
        List<Vector2> targetPositionList = new List<Vector2>();
        int WingCounter = 0;
        int WingRank = 1;
        targetPositionList.Add(basePosition);
        for(int i = 1; i < unitCount; i++)
        {
            if(i % 2 == 1)
            {
                float leftWing = direction + 225f;
                Vector2 dir = ReturnDegreeRotationToVector(new Vector2(1, 0), leftWing);
                Vector2 position = basePosition + dir * (wingDistance * WingRank);
                targetPositionList.Add(position);
                WingCounter++;
            }
            else
            {
                float rightWing = direction + 315f;
                Vector2 dir = ReturnDegreeRotationToVector(new Vector2(1, 0), rightWing);
                Vector2 position = basePosition + dir * (wingDistance * WingRank);
                targetPositionList.Add(position);
                WingCounter++;
            }
            if(WingCounter >= 2)
            {
                WingRank++;
                WingCounter = 0;
            }
        }
        return targetPositionList; 
    }
    public List<Vector2> SetLineFormation(float direction, Vector2 basePosition, List<float> LineDistanceArray, List<int> unitPositionCountArray)
    {
        List<Vector2> targetPositionList = new List<Vector2>();
        int baseRank = 0;
        for (int i = 0; i < LineDistanceArray.Count; i++)
        {
            Vector2 dir = ReturnDegreeRotationToVector(new Vector2(0, -1), direction);
            Vector2 newBase = basePosition + (dir * 3f * baseRank);

            targetPositionList.AddRange(SetLineFormation(direction, newBase, LineDistanceArray[i], unitPositionCountArray[i]));

            baseRank++;
        }
        return targetPositionList;
    }
    public List<Vector2> SetLineFormation(float direction, Vector2 basePosition, float unitDistance, int unitCount)
    {
        List<Vector2> targetPositionList = new List<Vector2>();
        int LineCounter = 0;
        int LineRank = 1;
        targetPositionList.Add(basePosition);
        for (int i = 1; i < unitCount; i++)
        {
            if (i % 2 == 1)
            {
                float angle = direction;
                Vector2 dir = ReturnDegreeRotationToVector(new Vector2(1, 0), angle);
                Vector2 position = basePosition + dir * (unitDistance * LineRank);
                targetPositionList.Add(position);
                LineCounter++;
            }
            else
            {
                float angle = direction;
                Vector2 dir = ReturnDegreeRotationToVector(new Vector2(1, 0), angle);
                Vector2 position = basePosition + dir * (unitDistance * LineRank * -1);
                targetPositionList.Add(position);
                LineCounter++;
            }
            if(LineCounter >= 2)
            {
                LineRank++;
                LineCounter = 0;
            }
        }
        return targetPositionList;
    }
    public List<Vector2> SetSkirmishFormation(Vector2 basePosition, List<float> unitDistanceArray, List<int> unitPositionCountArray)
    {
        List<Vector2> targetPositionList = new List<Vector2>();
        for(int i = 0; i < unitDistanceArray.Count; i++)
        {
            targetPositionList.AddRange(SetSkirmishFormation(basePosition, unitDistanceArray[i], unitPositionCountArray[i]));
        }
        return targetPositionList;
    }
    public List<Vector2> SetSkirmishFormation(Vector2 basePosition, float unitDistance, int unitCount)
    {
        List<Vector2> targetPositionList = new List<Vector2>();

        for(int i = 0; i < unitCount; i++)
        {
            float RandomX = UnityRandom.Range(basePosition.x - unitDistance, basePosition.x + unitDistance);
            float RandomY = UnityRandom.Range(basePosition.y - unitDistance, basePosition.y + unitDistance);
            Vector2 position = new Vector2(RandomX, RandomY);
            targetPositionList.Add(position);
        }

        return targetPositionList;
    }

    private Vector3 ReturnDegreeRotationToVector(Vector3 vec, float angle)
    {
        return Quaternion.Euler(0, 0, angle) * vec;
    }
    #endregion

    #region Misc. Function
    public void ChangeFormation(int F)
    {
        if (GameManager.Instance.GamePaused)
            return;

        switch (F)
        {
            case (0):
                SelectFormation = Formation.Ring;
                break;
            case (1):
                SelectFormation = Formation.Wing;
                break;
            case (2):
                SelectFormation = Formation.Line;
                break;
            case (3):
                SelectFormation = Formation.Box;
                break;
            case (4):
                SelectFormation = Formation.Skirmish;
                break;
        }
    }
    public void DeselectAll()
    {
        if (GameManager.Instance.GamePaused)
            return;

        selectedUnitList.RemoveAll(delegate (UnitDFSModule x) { return x == null; });
        if (selectedUnitList.Count > 0)
        {
            foreach (UnitDFSModule Unit in selectedUnitList)
            {
                Unit.SetSelectedVisible(false);
            }
            selectedUnitList.Clear();
        }
        else
        {
            selectedUnitList.Clear();
            UnitDFSModule [] Useable = FindObjectsOfType<UnitDFSModule>();
            foreach(UnitDFSModule Unit in Useable)
            {
                if (Unit != null)
                {
                    Unit.SetSelectedVisible(true);
                    selectedUnitList.Add(Unit);
                }
            }
            OnSelectTarget?.Invoke(this, new SelectTargetArgs { selectedUnit = selectedUnitList });
        }
    }
    public int GetSelectingUnit()
    {
        return selectedUnitList.Count;
    }
    private void DisplaySelector()
    {
        isSelectingFormation = true;
        OnFormationDisplay?.Invoke(this, new Personal.Utils.SingleBoolArgs { value = isSelectingFormation });
    }
    #endregion

    #region TEAM CREATION & SELECTION
    public void CreateTeam(int team)
    {
        if (GameManager.Instance.GamePaused)
            return;

        switch (team)
        {
            case 0:
                AutoCreateTeam(Team_1);
                break;
            case 1:
                AutoCreateTeam(Team_2);
                break;
            case 2:
                AutoCreateTeam(Team_3);
                break;
            case 3:
                AutoCreateTeam(Team_4);
                break;
            case 4:
                AutoCreateTeam(Team_5);
                break;
        }
    }
    public void SelectTeam(int team)
    {
        if (GameManager.Instance.GamePaused)
            return;

        switch (team)
        {
            case 0:
                if (Team_1 == null)
                    return;
                AutoSelectTeam(Team_1,0);
                break;
            case 1:
                if (Team_2 == null)
                    return;
                AutoSelectTeam(Team_2,1);
                break;
            case 2:
                if (Team_3 == null)
                    return;
                AutoSelectTeam(Team_3,2);
                break;
            case 3:
                if (Team_4 == null)
                    return;
                AutoSelectTeam(Team_4,3);
                break;
            case 4:
                if (Team_5 == null)
                    return;
                AutoSelectTeam(Team_5,4);
                break;
        }
    }
    public void AutoSelectTeam(List<UnitDFSModule> teamArray, int teamNumber)
    {
        SelectDisplayTransform.gameObject.SetActive(false);

        selectedUnitList.RemoveAll(delegate (UnitDFSModule x) { return x == null; });
        foreach (UnitDFSModule Unit in selectedUnitList)
        {
            Unit.SetSelectedVisible(false);
        }
        selectedUnitList.Clear();

        teamArray.RemoveAll(delegate (UnitDFSModule y) { return y == null; });
        foreach (UnitDFSModule newUnit in teamArray)
        {
            newUnit.SetSelectedVisible(true);
            selectedUnitList.Add(newUnit);
        }
        OnSelectTarget?.Invoke(this, new SelectTargetArgs{ selectedUnit = selectedUnitList});
    }
    public void AutoCreateTeam(List<UnitDFSModule> teamArray)
    {
        if (selectedUnitList != null)
        {
            teamArray.Clear();
            foreach (UnitDFSModule Unit in selectedUnitList)
            {
                RemoveSelection(Unit);
                teamArray.Add(Unit);
            }
        }
    }
    public void RemoveSelection(UnitDFSModule Unit)
    {
        if (GameManager.Instance.GamePaused)
            return;

        int CheckT1 = Team_1.FindIndex(x => x == Unit);
        if (CheckT1 != -1)
        {
            Team_1.RemoveAt(CheckT1);
            return;
        }

        int CheckT2 = Team_2.FindIndex(x => x == Unit);
        if (CheckT2 != -1)
        {
            Team_2.RemoveAt(CheckT2);
            return;
        }

        int CheckT3 = Team_3.FindIndex(x => x == Unit);
        if (CheckT3 != -1)
        {
            Team_3.RemoveAt(CheckT3);
            return;
        }

        int CheckT4 = Team_4.FindIndex(x => x == Unit);
        if (CheckT4 != -1)
        {
            Team_4.RemoveAt(CheckT4);
            return;
        }

        int CheckT5 = Team_5.FindIndex(x => x == Unit);
        if (CheckT5 != -1)
        {
            Team_5.RemoveAt(CheckT5);
            return;
        }
    }
    public void SelectUnitType(int type)
    {
        if (GameManager.Instance.GamePaused)
            return;

        foreach (UnitDFSModule Unit in selectedUnitList)
        {
            Unit.SetSelectedVisible(false);
        }
        selectedUnitList.Clear();
        SelectDisplayTransform.gameObject.SetActive(false);

        switch (type)
        {
            case 0:
                UnitSwordModule[] uT1 = FindObjectsOfType<UnitSwordModule>();
                foreach (UnitSwordModule uT in uT1)
                {
                    selectedUnitList.Add(uT.GetComponent<UnitDFSModule>());
                }
                break;
            case 1:
                UnitRifleModule[] uT2 = FindObjectsOfType<UnitRifleModule>();
                foreach (UnitRifleModule uT in uT2)
                {
                    selectedUnitList.Add(uT.GetComponent<UnitDFSModule>());
                }
                break;
            case 2:
                UnitRailgunModule[] uT3 = FindObjectsOfType<UnitRailgunModule>();
                foreach (UnitRailgunModule uT in uT3)
                {
                    selectedUnitList.Add(uT.GetComponent<UnitDFSModule>());
                }
                break;
            case 3:
                UnitMortarModule[] uT4 = FindObjectsOfType<UnitMortarModule>();
                foreach (UnitMortarModule uT in uT4)
                {
                    selectedUnitList.Add(uT.GetComponent<UnitDFSModule>());
                }
                break;
        }

        foreach (UnitDFSModule Unit in selectedUnitList)
        {
            Unit.SetSelectedVisible(true);
        }
        OnSelectTarget?.Invoke(this, new SelectTargetArgs { selectedUnit = selectedUnitList });
    }
    #endregion

    #region ENABLE/DISABLE
    private void OnDisable()
    {
        OnFormationDisplay -= UI_Manager.Instance.SignalFormationDisplay;
        OnSelectTarget -= UI_Manager.Instance.SignalUnitInfo;

        Controller.Basic.CreateTeamOne.performed -= ctx => CreateTeam(0);
        Controller.Basic.CreateTeamTwo.performed -= ctx => CreateTeam(1);
        Controller.Basic.CreateTeamThree.performed -= ctx => CreateTeam(2);
        Controller.Basic.CreateTeamFour.performed -= ctx => CreateTeam(3);
        Controller.Basic.CreateTeamFive.performed -= ctx => CreateTeam(4);

        Controller.Basic.SelectTeamOne.performed -= ctx => SelectTeam(0);
        Controller.Basic.SelectTeamTwo.performed -= ctx => SelectTeam(1);
        Controller.Basic.SelectTeamThree.performed -= ctx => SelectTeam(2);
        Controller.Basic.SelectTeamFour.performed -= ctx => SelectTeam(3);
        Controller.Basic.SelectTeamFive.performed -= ctx => SelectTeam(4);

        Controller.Basic.ShowSelector.performed -= ctx => DisplaySelector();
        Controller.Basic.Deselect.performed -= ctx => DeselectAll();
        Controller.Basic.Disable();
    }
    #endregion
}
