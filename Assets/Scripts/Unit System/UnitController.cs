using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Personal.Utils;
using Outsider;

public class UnitController : MonoBehaviour
{
    public static UnitController Instance;
    public UnitBaseModule BASE;
    private ControlScheme PlayController;
    private Vector2 InputVector;
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
        BASE = GetComponent<UnitBaseModule>();
        BASE.SetMaster(true);
    }
    private void Start()
    {
        PlayController = new ControlScheme();
        PlayController.Enable();
        PlayController.Basic.Move.performed += ctx => InputVector = ctx.ReadValue<Vector2>();
        PlayController.Basic.Move.canceled += ctx => InputVector = Vector2.zero;
        
    }
    private void Update()
    {
        GetComponent<UnitManeuverModule>().SetVelocity(InputVector);
    }

    private void OnDisable()
    {
        PlayController.Basic.Move.performed -= ctx => InputVector = ctx.ReadValue<Vector2>();
        PlayController.Basic.Move.canceled -= ctx => InputVector = Vector2.zero;
        PlayController.Disable();

        if(!GameManager.Instance.returnMenu)
            GameManager.Instance.GameEnded = true;
        GameManager.Instance.GAMEOVER();
    }
}
