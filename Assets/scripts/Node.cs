using UnityEngine;
using System.Collections.Generic;

public class Node
{
    public bool walkable;
    public Vector3 position;
    public Node parent;
    public GameObject render;

    public List<Graph.Edge> connections;
    public int id;

    public int gridX;
    public int gridY;

    public int gCost;
    public int hCost;

    public int fCost
    {
        get { return gCost + hCost; }
    }

    public Node(int _id, GameObject r, Vector3 _pos, bool _walkable)
    {
        render = r;
        walkable = _walkable;
        position = _pos;
        id = _id;
        gCost = 0;
        hCost = 0;
        connections = new List<Graph.Edge>();
    }
}
//struct Node
//{
//    public List<Edge> connections;
//    public GameObject render;

//    public Node(GameObject r, float x = 0, float y = 0, int val = 0, int score = 0)
//    {
//        render = r;
//        value = val;
//        gscore = score;
//        position = new Vector2(x, y);
//        connections = new List<Edge>();
//    }
//}