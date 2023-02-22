using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Custom Grid System --> Pathfinding System (Using PathNode object)
 */
public class Pathfinding
{
    public static Pathfinding Instance { get; private set; }
    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONOAL_COST = 14;

    private GridMap<PathNode> grid;

    private List<PathNode> openList;
    private List<PathNode> closedList;

    public Pathfinding(int width, int height)
    {
        Instance = this;
        grid = new GridMap<PathNode>(width, height, 5f, Vector3.zero, (GridMap<PathNode> g, int x, int y) => new PathNode(g, x, y));
    }

    public List<Vector3> FindPath(Vector3 start, Vector3 end)
    {
        grid.GetGridXY(start, out int Sx, out int Sy);
        grid.GetGridXY(end, out int Ex, out int Ey);
        List<PathNode> path = FindPath(Sx, Sy, Ex, Ey);
        if (path == null)
        {
            return null;
        }
        else
        {
            List<Vector3> vectorPath = new List<Vector3>();
            foreach (PathNode P in path)
            {
                vectorPath.Add(new Vector3(P.x, P.y) * grid.GetCellSize() + Vector3.one * grid.GetCellSize() * 0.5f);
            }
            return vectorPath;
        }
    }
    public List<PathNode> FindPath(int startX, int startY, int endX, int endY)
    {
        PathNode startNode = grid.GetGridObject(startX, startY);
        PathNode endNode = grid.GetGridObject(endX, endY);

        openList = new List<PathNode>() { startNode };
        closedList = new List<PathNode>();

        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for(int y = 0; y < grid.GetHeigth(); y++)
            {
                PathNode pathNode = grid.GetGridObject(x, y);
                pathNode.gCost = int.MaxValue;
                pathNode.CalculateFCost();
                pathNode.previousNode = null;
            }
        }

        startNode.gCost = 0;
        startNode.hCost = CalculateDistanceCost(startNode, endNode);
        startNode.CalculateFCost();

        while(openList.Count > 0)
        {
            PathNode currentNode = GetLowestFCostNode(openList);
            if(currentNode == endNode)
            {
                return CalculatePath(endNode);
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            foreach(PathNode neighbourNode in GetNeighbourList(currentNode))
            {
                if (closedList.Contains(neighbourNode)) continue;
                if (!neighbourNode.isWalkable)
                {
                    closedList.Add(neighbourNode);
                    continue;
                }

                int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighbourNode);

                if(tentativeGCost < neighbourNode.gCost)
                {
                    neighbourNode.previousNode = currentNode;
                    neighbourNode.gCost = tentativeGCost;
                    neighbourNode.hCost = CalculateDistanceCost(neighbourNode, endNode);
                    neighbourNode.CalculateFCost();

                    if (!openList.Contains(neighbourNode))
                    {
                        openList.Add(neighbourNode);
                    }
                }
            }
        }

        // out of nodes on the openList
        return null;
    }
    
    private List<PathNode> GetNeighbourList(PathNode currentNode)
    {
        List<PathNode> neighbourList = new List<PathNode>();

        if(currentNode.x - 1 >= 0)
        {
            neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y));
            if (currentNode.y - 1 >= 0) neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y - 1));
            if (currentNode.y + 1 < grid.GetHeigth()) neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y + 1));
        }
        if (currentNode.x + 1 >= 0)
        {
            neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y));
            if (currentNode.y - 1 >= 0) neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y - 1));
            if (currentNode.y + 1 < grid.GetHeigth()) neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y + 1));
        }
        if (currentNode.y - 1 >= 0) neighbourList.Add(GetNode(currentNode.x, currentNode.y - 1));
        if (currentNode.y + 1 < grid.GetHeigth()) neighbourList.Add(GetNode(currentNode.x, currentNode.y + 1));

        return neighbourList;
    }

    public PathNode GetNode(int x, int y)
    {
        return grid.GetGridObject(x, y);
    }
    public GridMap<PathNode> GetGrid()
    {
        return grid;
    }
    private List<PathNode> CalculatePath(PathNode endNode)
    {
        List<PathNode> path = new List<PathNode>();
        path.Add(endNode);
        PathNode currentNode = endNode;
        while(currentNode.previousNode != null)
        {
            path.Add(currentNode.previousNode);
            currentNode = currentNode.previousNode;
        }

        path.Reverse();
        return path;
    }

    private int CalculateDistanceCost( PathNode a, PathNode b)
    {
        int xDistance = Mathf.Abs(a.x - b.x);
        int yDistance = Mathf.Abs(a.y - b.y);
        int remaining = Mathf.Abs(xDistance - yDistance);
        return MOVE_DIAGONOAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
    }

    private PathNode GetLowestFCostNode(List<PathNode> pathNodeList)
    {
        PathNode lowestFCostNode = pathNodeList[0];
        for(int i = 1; i < pathNodeList.Count; i++)
        {
            if(pathNodeList[i].fCost < lowestFCostNode.fCost)
            {
                lowestFCostNode = pathNodeList[i];
            }
        }

        return lowestFCostNode;
    }
}
