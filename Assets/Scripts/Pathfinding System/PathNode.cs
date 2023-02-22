using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Grid Object --> PathNode
 */
public class PathNode
{
    private GridMap<PathNode> grid;
    public int x;
    public int y;

    public int gCost;
    public int hCost;
    public int fCost;

    public bool isWalkable;
    public PathNode previousNode;

    public PathNode(GridMap<PathNode> grid, int x, int y)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
        isWalkable = true;
    }

    public override string ToString()
    {
        return x + "," + y;
    }

    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }
    public void SetIsWalkable(bool canWalk)
    {
        this.isWalkable = canWalk;
        grid.TriggerGridObjectChanged(x,y);
    }
}
