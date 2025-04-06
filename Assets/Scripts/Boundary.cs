using System;
using System.Collections.Generic;
using System.Numerics;

public class Boundary : IGridObject
{
    public UnityEngine.Vector3 coordinate { get; set; }

    public float x { get; set; }
    public float y { get; set; }
    public string Location { get; set; }
    public string TileName { get; set; }
    public List<IGridObject> Neighbors { get; set; }
    public List<Tile> NeighborTiles { get; set; }

    public bool isNavigable { get; set; }

    public float gCost { get; set; }
    public float hCost { get; set; }
    public float fCost { get; set; }

    public IGridObject cameFromNode { get; set; }

    public Boundary()
    {

    }

    public void Init(bool isOffset)
    {
        // Implement your initialization logic here
    }
}

public interface IGridObject
{
    string TileName { get; set; }
    UnityEngine.Vector3 coordinate { get; set; }
    public float x { get; set; }
    public float y { get; set; }
    public string Location { get; set; }
    public bool isNavigable { get; set; }
    void Init(bool isOffset);


    public float gCost { get; set; }
    public float hCost { get; set; }
    public float fCost { get; set; }

    public List<IGridObject> Neighbors { get; set; }

    public IGridObject cameFromNode { get; set; }
    List<Tile> NeighborTiles { get; set; }

    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }
}

