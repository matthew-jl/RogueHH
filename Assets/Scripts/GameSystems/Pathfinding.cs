using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    public GridManager gridManager;

    public List<Node> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Node startNode = GetNodeFromWorldPoint(startPos);
        Node targetNode = GetNodeFromWorldPoint(targetPos);

        if (!targetNode.isWalkable)
        {
            return null;
        }

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();

        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost ||
                   (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                // path found
                return RetracePath(startNode, targetNode);
            }

            foreach (Node neighbor in GetNeighbors(currentNode))
            {
                if (!neighbor.isWalkable || closedSet.Contains(neighbor))
                {
                    continue;
                }

                int newCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);

                // after checking a node, update neighbor nodes to reflect least cost (if the new cost is smaller)
                if (newCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newCostToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, targetNode);
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }
        }

        // no path found
        return null;
    }

    Node GetNodeFromWorldPoint(Vector3 worldPosition)
    {
        int x = Mathf.RoundToInt(worldPosition.x / gridManager.tileSpacing);
        int z = Mathf.RoundToInt(worldPosition.z / gridManager.tileSpacing);

        x = Mathf.Clamp(x, 0, gridManager.gridWidth - 1);
        z = Mathf.Clamp(z, 0, gridManager.gridHeight - 1);

        return gridManager.nodeGrid[x, z];
    }

    List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                if (x == 0 && z == 0)
                    continue;

                // prevent diagonal movement
                if (Mathf.Abs(x) + Mathf.Abs(z) > 1)
                    continue;

                int checkX = node.gridX + x;
                int checkZ = node.gridZ + z;

                if (checkX >= 0 && checkX < gridManager.gridWidth && checkZ >= 0 && checkZ < gridManager.gridHeight)
                {
                    neighbors.Add(gridManager.nodeGrid[checkX, checkZ]);
                }
            }
        }

        return neighbors;
    }

    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstZ = Mathf.Abs(nodeA.gridZ - nodeB.gridZ);

        // Since movement cost per tile is 10
        return (dstX + dstZ) * 10;
    }

    //int GetDistance(Node nodeA, Node nodeB)
    //{
    //    int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
    //    int dstZ = Mathf.Abs(nodeA.gridZ - nodeB.gridZ);

    //    /*
    //     * assuming each grid square is 1x1, the diagonal would be sqrt(2) = 1.4
    //     * to get nice round numbers (multiply by 10), the weight (distance) of a diagonal square is 14 and the weight of an across square (up down left right) is 10
    //     * to calculate the distance, use the formula below
    //     */
    //    if (dstX > dstZ)
    //        return 14 * dstZ + 10 * (dstX - dstZ);
    //    return 14 * dstX + 10 * (dstZ - dstX);

    //}

    List<Node> RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            //Debug.Log(currentNode.worldPosition);
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        path.Reverse(); // So the path is from start to end
        return path;
    }


}
