using UnityEngine;
using System.Collections.Generic;

public class Solver : MonoBehaviour
{
    Graph graph;
    public Material expMat, unexpMat, selectMat, startMat;
    public float delay = 0.08f;

    void Awake()
    {
        graph = GetComponent<Graph>();
    }

    //find path using positions
    void FindPath(Vector3 start, Vector3 end)
    {
        Node startNode = graph.NodeFromWorldPos(start);
        Node endNode = graph.NodeFromWorldPos(end);

        List<Node> unexplored = new List<Node>();
        List<Node> explored = new List<Node>();

        unexplored.Add(startNode);

        while(unexplored.Count != 0)
        {
            Node current = unexplored[0];

            for(int i = 1; i < unexplored.Count; ++i)
            {
                if(unexplored[i].fCost < current.fCost || 
                   unexplored[i].fCost == current.fCost && unexplored[i].hCost < current.hCost)
                {
                    current = unexplored[i];
                }
            }

            unexplored.Remove(current);
            explored.Add(current);

            if(current == endNode)
            {
                return;
            }
        }
    }

    //public IEnumerator FindPath(Node startNode, Node goalNode)
    //{
    //    Queue<Node> unexplored = new Queue<Node>();
    //    List<Node> explored = new List<Node>();
        
    //    //change startNode material
    //    startNode.render.GetComponentInChildren<Renderer>().material = startMat;

    //    unexplored.Enqueue(startNode/*startNode*//*nodes[startNode]*/);
    //    while (unexplored.Count != 0)
    //    {
    //        Node currentNode = unexplored.Dequeue();

    //        explored.Add(currentNode);
    //        //change current node's material to explored (if !startnode)
    //        if (currentNode != startNode)
    //            currentNode.render.GetComponentInChildren<Renderer>().material = expMat;

    //        if (currentNode == goalNode)
    //        {
    //            GetPath(startNode, goalNode);
    //            break;
    //        }

    //        yield return new WaitForSeconds(delay);

    //        foreach (Graph.Edge edge in currentNode.connections)
    //        {
    //            int newGCost = currentNode.gCost + edge.cost;
    //            if(edge.connection.gCost < newGCost || edge.connection.parent == null)
    //            {
    //                edge.connection.gCost = newGCost;
    //                //edge.connection.hCost = //dist from edge.connection to goalNode
    //                //edge.connection.fCost = edge.connection.gCost + edge.connection.hCost;
    //                edge.connection.parent = currentNode;
    //            }

    //            if (!explored.Contains(edge.connection))
    //            {
    //                unexplored.Enqueue(edge.connection);

    //                //print("Add Connection " + c.connection.value);
    //                //yield return new WaitForSeconds(delay);
    //            }
    //        }
    //        //yield return new WaitForSeconds(delay);
    //    }
    //}

    //void GetPath(Node startNode, Node endNode)
    //{
    //    List<Node> path = new List<Node>();
    //    Node currentNode = endNode;
    //    while(currentNode != startNode)
    //    {
    //        path.Add(currentNode);
    //        currentNode = currentNode.parent;
    //    }
    //    path.Reverse();
    //    this.path = path;
    //}
}