using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public bool isWalkable;
    public Vector3 worldPosition;
    public int gridX;
    public int gridZ;

    public int gCost; // cost (distance) from start node
    public int hCost; // heuristic cost to end node
    public Node parent; // to retrace the path

    public bool hasPlayer;
    public bool hasEnemy;

    public Node(bool isWalkable, Vector3 worldPosition, int gridX, int gridZ)
    {
        this.isWalkable = isWalkable;
        this.worldPosition = worldPosition;
        this.gridX = gridX;
        this.gridZ = gridZ;

        this.hasPlayer = false;
        this.hasEnemy = false;
    }

    public int fCost
    {
        get { return gCost + hCost; }
    }

}

