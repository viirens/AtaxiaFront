using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
//using Newtonsoft.Json.Linq;

public class Pathfinding
{

    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;

    private GridManager grid;
    private List<IGridObject> openList;
    private List<IGridObject> closedList;

    public Pathfinding()
    {
        grid = GridManager.Instance;
    }

    public List<IGridObject> FindPath(int startX, int startY, int endX, int endY)
    {


        IGridObject startNode = grid.GetNodeAtPosition(startX, startY);
        IGridObject endNode = grid.GetNodeAtPosition(endX, endY);
        openList = new List<IGridObject> { startNode };
        closedList = new List<IGridObject>();

        for (float x = -.5f; x < grid.GetWidth(); x += .5f)
        {
            for (float y = -.5f; y < grid.GetHeight(); y += .5f)
            {
                IGridObject pathNode = grid.GetNodeAtPosition(x, y);
                pathNode.gCost = int.MaxValue;
                pathNode.CalculateFCost();
                pathNode.cameFromNode = null;
            }
        }

        startNode.gCost = 0;
        startNode.hCost = CalculateDistanceCost(startNode, endNode);
        startNode.CalculateFCost();

        while (openList.Count > 0)
        {
            IGridObject currentNode = GetLowestFCostNode(openList);
            //Debug.Log(currentNode.TileName);

            if (currentNode == endNode)
            {
                // reached final node
                return CalculatePath(endNode);
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            for (int i = 0; i < currentNode.Neighbors.Count; i++)
            {
                IGridObject neighbourNode = currentNode.Neighbors[i];
                if (closedList.Contains(neighbourNode) || neighbourNode == null || neighbourNode.isNavigable == false) continue;

                float tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighbourNode);

                if (tentativeGCost < neighbourNode.gCost)
                {
                    neighbourNode.cameFromNode = currentNode;
                    neighbourNode.gCost = tentativeGCost;
                    neighbourNode.hCost = CalculateDistanceCost(neighbourNode, endNode);
                    neighbourNode.CalculateFCost();
                    //Debug.Log(neighbourNode.TileName + " : " + neighbourNode.fCost.ToString());

                    if (!openList.Contains(neighbourNode))
                    {
                        openList.Add(neighbourNode);
                    }
                }
            }

        }

        // Out of nodes on the openList
        return null;
    }

    public List<IGridObject> GetReachableTiles(int startX, int startY, int movementDistance)
    {
        Vector2 startPosition = new Vector2(startX, startY);
        if (!GridManager.Instance._tiles.TryGetValue(startPosition, out IGridObject startNode))
        {
            // Handle the case where the start position is not in the grid
            return new List<IGridObject>();
        }

        List<IGridObject> reachableTiles = new List<IGridObject>();
        Queue<IGridObject> queue = new Queue<IGridObject>();
        Dictionary<IGridObject, int> distance = new Dictionary<IGridObject, int>();

        foreach (var tile in GridManager.Instance._tiles.Values)
        {
            distance[tile] = int.MaxValue;
        }

        distance[startNode] = 0;
        queue.Enqueue(startNode);

        while (queue.Count > 0)
        {
            IGridObject currentNode = queue.Dequeue();

            foreach (IGridObject neighbor in currentNode.Neighbors)
            {
                if (!GridManager.Instance._tiles.ContainsValue(neighbor) || !neighbor.isNavigable)
                {
                    continue;
                }

                int tentativeDistance = distance[currentNode] + 1; // Assuming uniform cost for simplicity

                if (tentativeDistance <= movementDistance && tentativeDistance < distance[neighbor])
                {
                    distance[neighbor] = tentativeDistance;
                    queue.Enqueue(neighbor);
                    reachableTiles.Add(neighbor);
                }
            }
        }

        return reachableTiles;
    }



    private List<IGridObject> CalculatePath(IGridObject endNode)
    {
        List<IGridObject> path = new List<IGridObject>
        {
            endNode
        };
        IGridObject currentNode = endNode;
        while (currentNode.cameFromNode != null)
        {
            path.Add(currentNode.cameFromNode);
            currentNode = currentNode.cameFromNode;
        }
        path.Reverse();
        return path;
    }

    private float CalculateDistanceCost(IGridObject a, IGridObject b)
    {
        float xDistance = Mathf.Abs(a.x - b.x);
        float yDistance = Mathf.Abs(a.y - b.y);
        float remaining = Mathf.Abs(xDistance - yDistance);

        return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
    }

    private IGridObject GetLowestFCostNode(List<IGridObject> pathNodeList)
    {
        IGridObject lowestFCostNode = pathNodeList[0];
        for (int i = 1; i < pathNodeList.Count; i++)
        {
            if (pathNodeList[i].fCost < lowestFCostNode.fCost)
            {
                lowestFCostNode = pathNodeList[i];
            }
        }
        return lowestFCostNode;
    }

    private bool HasLineOfSight(IGridObject start, IGridObject end)
    {
        Vector2 startCoord = start.coordinate;
        Vector2 endCoord = end.coordinate;

        float dx = Mathf.Abs(endCoord.x - startCoord.x);
        float dy = Mathf.Abs(endCoord.y - startCoord.y);
        float x = startCoord.x;
        float y = startCoord.y;
        int n = 1 + (int)(dx + dy);
        float x_inc = (endCoord.x > startCoord.x) ? 0.5f : -0.5f;
        float y_inc = (endCoord.y > startCoord.y) ? 0.5f : -0.5f;
        float error = dx - dy;
        dx *= 2;
        dy *= 2;

        for (; n > 0; --n)
        {
            // Check for boundary at the current position
            if (GridManager.Instance._tiles.TryGetValue(new Vector2(x, y), out IGridObject currentTile))
            {
                if (currentTile is Boundary)
                {
                    return false; // Boundary encountered
                }
            }

            if (error > 0)
            {
                x += x_inc;
                error -= dy;
            }
            else
            {
                y += y_inc;
                error += dx;
            }
        }

        return true; // No boundaries encountered
    }

    public List<IGridObject> GetAttackRangeTiles(int startX, int startY, int attackRange)
    {
        Vector2 startPosition = new Vector2(startX, startY);
        if (!GridManager.Instance._tiles.TryGetValue(startPosition, out IGridObject startNode))
        {
            return new List<IGridObject>(); // Start position not in grid
        }

        List<IGridObject> reachableTiles = new List<IGridObject>();

        for (float x = -attackRange; x <= attackRange; x++)
        {
            for (float y = -attackRange; y <= attackRange; y++)
            {
                Vector2 currentPos = new Vector2(startX + x, startY + y);
                if (!GridManager.Instance._tiles.TryGetValue(currentPos, out IGridObject currentTile))
                {
                    continue; // Outside of grid, ignore this tile
                }

                // Check if the tile is navigable and within the attack range
                if (currentTile.isNavigable && ChebyshevDistance(startPosition, currentPos) <= attackRange && GridManager.Instance.CheckLineOfSightAndDrawLine(startPosition, currentTile.coordinate))
                {
                    reachableTiles.Add(currentTile);
                }

            }
        }

        return reachableTiles;
    }

    private float ChebyshevDistance(Vector2 pos1, Vector2 pos2)
    {
        return Mathf.Max(Mathf.Abs(pos1.x - pos2.x), Mathf.Abs(pos1.y - pos2.y));
    }

    private int ManhattanDistance(Vector2 pos1, Vector2 pos2)
    {
        return (int)(Mathf.Abs(pos1.x - pos2.x) + Mathf.Abs(pos1.y - pos2.y));
    }

    private bool PathCrossesBoundary(IGridObject start, IGridObject end)
    {
        // Assuming a square grid with orthogonal movement
        Vector2 startCoords = start.coordinate; // Assuming each IGridObject has a Position property
        Vector2 endCoords = end.coordinate;

        // Determine the direction of movement
        Vector2 direction = new Vector2(endCoords.x - startCoords.x, endCoords.y - startCoords.y);

        // Normalize the direction to get the step increments
        direction.Normalize();

        Vector2 currentPos = startCoords;

        while (currentPos != endCoords)
        {
            // Move to the next tile
            currentPos += direction;

            // Check if the current tile is a boundary
            if (GridManager.Instance._tiles.TryGetValue(currentPos, out IGridObject currentTile))
            {
                if (!currentTile.isNavigable)
                {
                    return true; // Path crosses a boundary
                }
            }
            else
            {
                return true; // Outside of the grid is considered a boundary
            }

            // Check if we have reached the end tile
            if (currentPos == endCoords)
            {
                break;
            }
        }

        return false; // No boundaries crossed
    }


}

