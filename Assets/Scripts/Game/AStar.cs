using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class AStar
{
    public static Dictionary<Vector2, Node> nodes;
    public static Dictionary<Node, Stack<Node>> enemyPaths;
    private static GameScript main;
    public static void Init(GameScript gs)
    {
        main = gs;
        ResetEnemyPaths();
    }
    public static void ResetEnemyPaths()
    {
        enemyPaths = new Dictionary<Node, Stack<Node>>();
    }
    public static Stack<Node> CheckForEnemyPath(Node node)
    {
        if (enemyPaths.ContainsKey(node))
        {
            var foundPath = enemyPaths[node];
            if (foundPath.Count != 0)
            {
                return new Stack<Node>(foundPath.Reverse());
            }
            else
            {
                enemyPaths.Remove(node);
            }
        }
        var newPath = GetPath(node.tile.pos);
        enemyPaths.Add(node, newPath);
        return newPath;
    }
    public static void CreateNodes(Dictionary<Vector2, Tile> tiles)
    {
        nodes = new Dictionary<Vector2, Node>();
        foreach (KeyValuePair<Vector2, Tile> entry in tiles)
        {
            nodes.Add(entry.Key, new Node(entry.Value));
        }
    }
    public static Stack<Node> GetPath(Vector2 start)
    {

        var startNode = nodes[start];
        var goalNode = nodes[new Vector2(7, 7)];

        HashSet<Node> openList = new HashSet<Node>();
        HashSet<Node> closedList = new HashSet<Node>();
        Stack<Node> finalPath = new Stack<Node>();
        var currentNode = nodes[start];
        openList.Add(currentNode);
        while (openList.Count > 0)
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    var neighborPos = new Vector2(currentNode.tile.pos.x - x, currentNode.tile.pos.y - y);
                    if (!nodes.ContainsKey(neighborPos) || neighborPos == currentNode.tile.pos)
                    {
                        continue;
                    }
                    var neighbor = nodes[neighborPos];
                    if (neighbor.tile.isOccupied)
                    {
                        continue;
                    }
                    int gCost = 0;
                    if (Mathf.Abs(x - y) == 1)
                    {
                        gCost = 10;
                    }
                    else
                    {
                        var direction = neighbor.tile.pos - currentNode.tile.pos;
                        var firstPos = new Vector2(neighborPos.x - direction.x, neighborPos.y);
                        var secondPos = new Vector2(neighborPos.x, neighborPos.y - direction.y);
                        if (nodes.ContainsKey(firstPos) && nodes.ContainsKey(secondPos) && nodes[firstPos].tile.isOccupied && nodes[secondPos].tile.isOccupied)
                        {
                            continue;
                        }
                        gCost = 14;
                    }

                    if (openList.Contains(neighbor))
                    {
                        if (currentNode.G + gCost < neighbor.G)
                        {
                            neighbor.CalculateValues(currentNode, gCost, goalNode);
                        }
                    }
                    else if (!closedList.Contains(neighbor))
                    {
                        openList.Add(neighbor);
                        neighbor.CalculateValues(currentNode, gCost, goalNode);
                    }
                }
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);
            if (openList.Count > 0)
            {
                currentNode = openList.OrderBy(n => n.F).First();
            }
            if (currentNode == goalNode)
            {
                while (currentNode != startNode)
                {
                    finalPath.Push(currentNode);
                    currentNode = currentNode.parent;
                }
                finalPath.Push(currentNode);
                break;
            }
        }
        return finalPath;
        //DEBUG------------------
        //DebugPath(openList, closedList, finalPath, startNode, goalNode);
        //-----------------------
    }

    public static void DebugPath(HashSet<Node> openList, HashSet<Node> closedList, Stack<Node> finalPath, Node start, Node goal)
    {
        foreach (Node node in openList)
        {
            if (node == start)
            {
                node.tile.spriteRenderer.color = Color.green;
            }
            else if (node == goal)
            {
                node.tile.spriteRenderer.color = Color.red;
            }
            else
            {
                node.tile.spriteRenderer.color = Color.cyan;
            }
        }
        foreach (Node node in closedList)
        {
            if (node == start)
            {
                node.tile.spriteRenderer.color = Color.green;
            }
            else if (node == goal)
            {
                node.tile.spriteRenderer.color = Color.red;
            }
            else
            {
                node.tile.spriteRenderer.color = Color.blue;
            }
        }
        foreach (Node node in finalPath)
        {
            node.tile.spriteRenderer.color = Color.green;
        }
    }

}
