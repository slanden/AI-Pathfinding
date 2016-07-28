using UnityEngine;
using System.Collections.Generic;

public class Node : IHeapItem<Node>
{
    public bool walkable;
    public Vector3 position;
    public Node parent;
    public GameObject render;

    public List<Graph.Edge> connections;
    public int id;
    public int heapIndex;

    //for some reason without these, using distance instead,
    //the algorithm doesn't find shortest path
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
        connections = new List<Graph.Edge>();
    }

    public int HeapIndex
    {
        get
        {
            return heapIndex;
        }
        set
        {
            heapIndex = value;
        }
    }

    public int CompareTo(Node nodeToCompare)
    {
        int compare = fCost.CompareTo(nodeToCompare.fCost);
        if(compare == 0)
        {
            compare = hCost.CompareTo(nodeToCompare.hCost);
        }
        // integer.CompareTo returns 1 if integer is higher.
        // we want to return 1 if integer is lower, so we negate.
        return -compare;
    }
}