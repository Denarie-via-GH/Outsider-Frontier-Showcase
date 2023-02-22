using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UnitManeuverModule : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    private UnitBaseModule BASE;
    private Rigidbody2D rigidBody;
    private Vector3 unitVelocity;
    private Vector3 unitDestination;
    private Vector3 pathfindingDestination;

    private bool IsFormationSnap = false;
    private bool IsPathfinding = false;

    private void Start()
    {
        BASE = GetComponent<UnitBaseModule>();
        rigidBody = GetComponent<Rigidbody2D>();
    }
    #region SET FUNCTION
    public void SetVelocity(Vector3 velocity)
    {
        this.unitVelocity = velocity;
    }
    public void SetDestination(Vector3 destination)
    {
        this.unitDestination = destination;
    }
    public void SetFormationSnap(bool value)
    {
        IsFormationSnap = value;
    }
    #endregion

    #region MOVE FUNCTION
    public void MoveUnit()
    {
        rigidBody.velocity = unitVelocity * moveSpeed;
    }
    public void MoveUnitToDestination()
    {
        Vector3 moveDir = (unitDestination - transform.position).normalized;
        if (Vector3.Distance(unitDestination, transform.position) < 1f)
        {
            moveDir = Vector3.zero;
            unitDestination = Vector3.zero;
            unitVelocity = Vector3.zero;
        }
        else
        {
            SetVelocity(moveDir);
            rigidBody.velocity = unitVelocity * moveSpeed;
        }
    }
    public void FixedUpdate()
    {
        moveSpeed = BASE.GetUnitSpeed();

        if (!IsPathfinding)
        {
            if (unitVelocity != Vector3.zero && unitDestination == Vector3.zero)
            {
                MoveUnit();
            }
            if (unitDestination != Vector3.zero)
            {
                MoveUnitToDestination();
            }
        }
        else if (IsPathfinding)
        {
            MovePath();
        }
    }
    #endregion

    #region PATHFINDING
    private int pathIndex = -1;
    private List<Vector3> pathVectorList;
    public Vector3 GetPosition()
    {
        return transform.position;
    }
    public void SetMovePosition(Vector3 targetPOS)
    {
        GetComponent<UnitManeuverModule>().SetDestination(targetPOS);
    }
    public void SetPathPosition(Vector3 targetPOS)
    {
        pathIndex = 0;
        
        pathVectorList = Pathfinding.Instance.FindPath(GetPosition(), targetPOS);

        if (pathVectorList != null && pathVectorList.Count > 1)
        {
            IsPathfinding = true;
            if (IsFormationSnap)
            {
                if(targetPOS != Vector3.zero)
                    pathfindingDestination = targetPOS;
            }
            
            pathVectorList.RemoveAt(0);
        }
    }
    public void MovePath()
    {
        if(pathVectorList != null && pathIndex != -1)
        {
            Vector3 nextPathPosition = pathVectorList[pathIndex];
            Vector3 moveVelocity = (nextPathPosition - GetPosition()).normalized;

            Vector3 DIR_VELOCITY = moveVelocity * moveSpeed;
            Vector3 STEER = DIR_VELOCITY - unitVelocity;
            unitVelocity += STEER / rigidBody.mass;
            transform.position += unitVelocity * Time.deltaTime;
            
            //transform.position = transform.position + DIR_VELOCITY * Time.deltaTime;

            float reachedPathPositionDistance = 1f;
            if(Vector2.Distance(GetPosition(), nextPathPosition) < reachedPathPositionDistance)
            {
                pathIndex++;
                if(pathIndex == pathVectorList.Count)
                {
                    pathIndex = -1;
                    if (IsFormationSnap)
                    {
                        transform.position = pathfindingDestination;

                        IsFormationSnap = false;
                        SetDestination(Vector3.zero);
                        SetVelocity(Vector3.zero); // new code
                    }
                }
            }
        }
        else
        {
            SetDestination(Vector3.zero); // new code
            SetVelocity(Vector3.zero);
            IsPathfinding = false;
        }
    }
    #endregion
}
