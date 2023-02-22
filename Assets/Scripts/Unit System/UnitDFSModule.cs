using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitDFSModule : MonoBehaviour
{
    [Header("Component")]
    private UnitBaseModule BASE;
    private GameObject selectedDisplay;
    private UnitManeuverModule Maneuver;
    
    [Header("Variable")]
    public bool isSelectable = true;
 
    public void SetSelectedVisible(bool visible)
    {
        if (BASE.IsMaster)
            return;

        selectedDisplay.SetActive(visible);
    }

    public void SetPathfinding(Vector3 targetPOS, bool snap)
    {
        Maneuver.SetFormationSnap(snap);
        Maneuver.SetPathPosition(targetPOS);
    }

    public void SetCoordinateMode()
    {
        BASE.SetCoordinate();
    }
    public void TriggerCoordinate()
    {
        BASE.isCoordinated = !BASE.isCoordinated;
    }
    public void SetFormatedMode(bool mode)
    {
        BASE.isInFormation = mode;
    }

    #region ENABLE/DISABLE
    private void OnEnable()
    {
        BASE = GetComponent<UnitBaseModule>();
        Maneuver = GetComponent<UnitManeuverModule>();
        selectedDisplay = transform.Find("Selected").gameObject;
        SetSelectedVisible(false);
    }
    private void OnDisable()
    {
        SetSelectedVisible(false);
    }
    #endregion
}
