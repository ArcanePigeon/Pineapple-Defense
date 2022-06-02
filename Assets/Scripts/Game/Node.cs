using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public Tile tile;
    public Node parent;
    public int G;
    public int H;
    public int F;
    public Node(Tile tile)
    {
        this.tile = tile;
    }

    public void CalculateValues(Node parent, int gCost, Node goal)
    {
        this.parent = parent;
        this.G = parent.G + gCost;
        this.H = (int)(Mathf.Abs(tile.pos.x - goal.tile.pos.x) + Mathf.Abs(tile.pos.y - goal.tile.pos.y)) * 10;
        this.F = G + H;
    }
}
