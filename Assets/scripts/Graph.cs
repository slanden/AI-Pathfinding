using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Graph : MonoBehaviour
{
    public LayerMask unwalkableMask;
    public int rows, cols;
    Vector2 gridSize;
    public float nodeRadius = 0.5f;
    float nodeDiameter;
    int gridSizeX, gridSizeY;
    List<Node> path;
    List<Node> nodes = new List<Node>();
    
    public float delay = 0.08f;
    public bool newPath = false;
    public int startingPt = 0;
    public int searchMethod = 0;    // 0 = DFS, 1 = BFS, 2 = Dijkstra's, 3 = A*

    //Gizmo stuff
    public Material expMat, unexpMat, selectMat, startMat;
    public List<LineRenderer> connectLines = new List<LineRenderer>();
    public GameObject nodeObj;
    public GameObject line;    
    GameObject container;   //container for holding path linerenderers

    public struct Edge
    {
        public Node connection;
        public int cost;

        public Edge(Node _node, int _cost = 0)
        {
            connection = _node;
            cost = _cost;
        }
    }

    void AddNode(int id, GameObject r, float x = 0, float y = 0, bool isWalkable = true)
    {
        nodes.Add(new Node(id, r,new Vector3(x, y, transform.position.z), isWalkable));
    }

    void AddConnection(Node n1, Node n2)
    {
        int dist = Mathf.RoundToInt(Vector3.Distance(n1.position, n2.position));
        n1.connections.Add(new Edge(n2, dist));
        n2.connections.Add(new Edge(n1, dist));
    }

    void CreateGraph()
    {
        int iter = 0;
        for (int i = 0; i < rows; ++i)
        {
            for (int j = 0; j < cols; ++j)
            {
                //float x = (j + 1) * 0.75f / (cols + 1);
                //float y = (i + 1) * 0.75f / (rows + 1);
                Vector3 pos = new Vector3(j * 0.75f, i * 0.75f, transform.position.z);
                //float x = j * 0.75f;
                //float y = i * 0.75f;

                GameObject o = Instantiate(nodeObj, pos, Quaternion.identity) as GameObject;
                o.transform.parent = transform;
                o.name = iter.ToString();

                bool walkable = !(Physics.CheckSphere(pos, nodeRadius, unwalkableMask));

                AddNode(iter, o, pos.x, pos.y, walkable);
                //print("Node " + nodes[iter].value + " position: " + nodes[iter].position);

                //vertical
                if (i != 0)
                {
                    Node _n1 = nodes[(i - 1) * cols + j];
                    Node _n2 = nodes[i * cols + j];
                    AddConnection(nodes[(i - 1) * cols + j], nodes[i * cols + j]);

                    GameObject g = Instantiate(line);
                    LineRenderer l = g.GetComponent<LineRenderer>();
                    l.SetPosition(0, _n1.render.transform.position);
                    l.SetPosition(1, _n2.render.transform.position);
                    connectLines.Add(l);
                    connectLines[connectLines.Count - 1].transform.parent = transform;
                }

                //horizontal
                if (j != 0)
                {
                    Node _n1 = nodes[i * cols + j - 1];
                    Node _n2 = nodes[i * cols + j];
                    AddConnection(nodes[i * cols + j - 1], nodes[i * cols + j]);

                    GameObject g = Instantiate(line);
                    LineRenderer l = g.GetComponent<LineRenderer>();
                    l.SetPosition(0, _n1.render.transform.position);
                    l.SetPosition(1, _n2.render.transform.position);
                    connectLines.Add(l);
                    connectLines[connectLines.Count - 1].transform.parent = transform;
                }

                // diags right
                if (i != 0 && j != 0)
                {
                    Node _n1 = nodes[(i - 1) * cols + j - 1];
                    Node _n2 = nodes[i * cols + j];
                    AddConnection(nodes[(i - 1) * cols + j - 1], nodes[i * cols + j]);

                    GameObject g = Instantiate(line);
                    LineRenderer l = g.GetComponent<LineRenderer>();
                    l.SetPosition(0, _n1.render.transform.position);
                    l.SetPosition(1, _n2.render.transform.position);
                    connectLines.Add(l);
                    connectLines[connectLines.Count - 1].transform.parent = transform;
                }

                // diags left
                if (i != 0 && j != cols - 1)
                {
                    Node _n1 = nodes[(i - 1) * cols + j + 1];
                    Node _n2 = nodes[i * cols + j];
                    AddConnection(nodes[(i - 1) * cols + j + 1], nodes[i * cols + j]);

                    GameObject g = Instantiate(line);
                    LineRenderer l = g.GetComponent<LineRenderer>();
                    l.SetPosition(0, _n1.render.transform.position);
                    l.SetPosition(1, _n2.render.transform.position);
                    connectLines.Add(l);
                    connectLines[connectLines.Count - 1].transform.parent = transform;
                }

                iter++;
            }
        }
    }

    public Node NodeFromWorldPos(Vector3 position)
    {
        //get percentage of where it is in the grid and make sure it's clamped between 0-1
        float percentX = Mathf.Clamp01((position.x + gridSize.x / 2) / gridSize.x);
        float percentY = Mathf.Clamp01((position.y + gridSize.y / 2) / gridSize.y);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

        //return grid[x,y];
        return nodes[0];
    }

    void Start()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridSize.x / nodeDiameter);    //how many columns fit into gridSize.x (width)
        gridSizeY = Mathf.RoundToInt(gridSize.y / nodeDiameter);    //how many rows fit into gridSize.y (height)

        CreateGraph();

        container = new GameObject();
        container.name = "Path";
        container.transform.parent = transform;

        //Solver(startingPt, searchMethod);
        StartCoroutine(FindPath(nodes[0], nodes[26]));
    }

    void OnDrawGizmos()
    {
        if (nodes != null)
        {
            foreach (Node n in nodes)
            {
                Gizmos.color = (n.walkable) ? Color.white : Color.red;
                Gizmos.DrawCube(n.position, Vector3.one * (nodeDiameter - 0.1f));
            }
        }
    }

    void Solver(int start, int method = 0)
    {
        //DFS
        if(method == 0)
            StartCoroutine(DFS(start));
        if (method == 1)
            StartCoroutine(BFS(start));
        //if(method == 3)
        //    StartCoroutine(Solver.)
    }

    IEnumerator DFS(int startNode)
    {
        Stack<Node> unexplored = new Stack<Node>();
        List<Node> explored = new List<Node>();

        Node start = nodes[startNode];
        start.render.GetComponentInChildren<Renderer>().material = startMat;

        unexplored.Push(start/*startNode*//*nodes[startNode]*/);
        while (unexplored.Count != 0)
        {
            Node current = unexplored.Pop();
            print("Node " + current.id);

            explored.Add(current);
            //change current node's material to explored (if !startnode)
            if (current.id != startNode)
                current.render.GetComponentInChildren<Renderer>().material = expMat;

            yield return new WaitForSeconds(delay);

            foreach (Edge c in current.connections)
            {
                if (!explored.Contains(c.connection))
                {
                    unexplored.Push(c.connection);
                    //print("Add Connection " + c.connection.value);
                    //yield return new WaitForSeconds(0.5f);
                }
            }
            //yield return new WaitForSeconds(delay);
        }
    }

    IEnumerator BFS(int startNode)
    {
        Queue<Node> unexplored = new Queue<Node>();
        List<Node> explored = new List<Node>();

        Node start = nodes[startNode];
        start.render.GetComponentInChildren<Renderer>().material = startMat;

        unexplored.Enqueue(start/*startNode*//*nodes[startNode]*/);
        while (unexplored.Count != 0)
        {
            Node current = unexplored.Dequeue();

            explored.Add(current);
            //change current node's material to explored (if !startnode)
            if (current.id != startNode)
                current.render.GetComponentInChildren<Renderer>().material = expMat;

            yield return new WaitForSeconds(delay);

            foreach (Edge c in current.connections)
            {
                if (!explored.Contains(c.connection))
                {
                    unexplored.Enqueue(c.connection);
                    //print("Add Connection " + c.connection.value);
                    //yield return new WaitForSeconds(delay);
                }
            }
            //yield return new WaitForSeconds(delay);
        }
    }

    IEnumerator Dijkstra(int startNode,List<Node> potentialEndNodes)
    {
        List<Node> unexplored = new List<Node>();
        List<Node> explored = new List<Node>();

        Node endNode;
        Node start = nodes[startNode];
        start.render.GetComponentInChildren<Renderer>().material = startMat;

        unexplored.Add(start);
        while (unexplored.Count != 0)
        {
            Node current = unexplored[0];

            foreach(Node n in potentialEndNodes)
                if(current.id == n.id)
                {
                    endNode = current;
                    break;
                }
            unexplored.Remove(current);
            explored.Add(current);
            //change current node's material to explored (if !startnode)
            if (current.id != startNode)
                current.render.GetComponentInChildren<Renderer>().material = expMat;

            yield return new WaitForSeconds(delay);

            foreach (Edge c in current.connections)
            {
                if (!explored.Contains(c.connection))
                {
                    unexplored.Add(c.connection);
                    //print("Add Connection " + c.connection.value);
                    //yield return new WaitForSeconds(delay);
                }
            }
            //yield return new WaitForSeconds(delay);
        }
    }

    //List<Node> GetNeighbors(Node node)
    //{
    //    List<Node> neighbors = new List<Node>();

    //    for (int x = -1; x <= 1; ++x)
    //    {
    //        for (int y = -1; y <= 1; ++y)
    //        {
    //            if (x == 0 && y == 0)
    //                continue;

    //            int checkX = node.gridX + x;
    //            int checkY = node.gridY + y;

    //            if(checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
    //                //neighbors.Add()
    //        }
    //    }

    //    return neighbors;
    //}

    public IEnumerator FindPath(Node startNode, Node goalNode)
    {
        List<Node> unexplored = new List<Node>();
        List<Node> explored = new List<Node>();

        //set startNode material
        startNode.render.GetComponentInChildren<Renderer>().material = startMat;

        unexplored.Add(startNode);
        while (unexplored.Count != 0)
        {

            Node currentNode = unexplored[0];
            //sort by lowest fCost
            for(int i = 1; i < currentNode.fCost; ++i)
            {
                if (unexplored[i].fCost < currentNode.fCost || 
                    unexplored[i].fCost == currentNode.fCost && unexplored[i].hCost < currentNode.hCost)
                    currentNode = unexplored[i];
            }

            unexplored.Remove(currentNode);
            explored.Add(currentNode);

            if (currentNode == goalNode)
            {
                GetPath(startNode, goalNode);
                break;
            }

            //change current node's material to explored (if !startnode)
            if (currentNode != startNode)
                currentNode.render.GetComponentInChildren<Renderer>().material = expMat;

            yield return new WaitForSeconds(delay);

            foreach (Edge edge in currentNode.connections)
            {
                if (!edge.connection.walkable || explored.Contains(edge.connection))
                    continue;

                int newGCost = currentNode.gCost + GetDistance(currentNode, edge.connection);
                if (newGCost < edge.connection.gCost || !unexplored.Contains(edge.connection))
                {
                    edge.connection.gCost = newGCost;
                    edge.connection.hCost = 0;//GetDistance(edge.connection, goalNode);
                    //edge.connection.fCost = edge.connection.gCost + edge.connection.hCost;
                    edge.connection.parent = currentNode;

                    if (!unexplored.Contains(edge.connection))
                        unexplored.Add(edge.connection);
                }                
            }
            //yield return new WaitForSeconds(delay);
        }
        //print("Coroutine Finished 1");
        //yield return null;

        //pause before displaying path
        yield return new WaitForSeconds(delay * 3);

        if (path != null)
        {
            foreach (Transform child in container.transform)
                Destroy(child.gameObject);

            path.Insert(0, startNode);
            for (int i = 0; i < path.Count; ++i)
            {
                if (i == 0)
                    continue;

                GameObject g = Instantiate(line);
                LineRenderer l = g.GetComponent<LineRenderer>();
                l.SetColors(Color.red, Color.red);
                l.SetPosition(0, path[i - 1].render.transform.position);
                l.SetPosition(1, path[i].render.transform.position);
                connectLines.Add(l);
                connectLines[connectLines.Count - 1].transform.parent = container.transform;
            }
        }

    }

    void GetPath(Node startNode, Node endNode)
    {
        List<Node> _path = new List<Node>();
        Node currentNode = endNode;
        while (currentNode != startNode)
        {
            _path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        _path.Reverse();
        path = _path;
        
        endNode.render.GetComponentInChildren<Renderer>().material = selectMat;
    }

    int GetDistance_2DListNode(Node a, Node b)
    {
        int distX = Mathf.Abs(a.gridX - b.gridX);
        int distY = Mathf.Abs(a.gridY - b.gridY);

        if (distX > distY)
            return 14 * distY + 10 * (distX - distY);
        return 14 * distX + 10 * (distY - distX);
    }

    int GetDistance(Node a, Node b)
    {
        return Mathf.RoundToInt(Vector3.Distance(a.position, b.position));
    }
}
