/*To Do: Decouple all pathfinding logic from graph logic*/

using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;

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
    bool newPath = true;
    int startPt = 0;
    int goalPt = 0;
    public int searchMethod = 3;    // 0 = DFS, 1 = BFS, 2 = Dijkstra's, 3 = A*

    public int MaxSize
    {
        get { return rows * cols; }
    }

    //Gizmo stuff
    public Material expMat, unexpMat, selectMat, startMat;
    List<LineRenderer> connectLines = new List<LineRenderer>();
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
                Vector3 pos = new Vector3(transform.position.x + j * nodeDiameter + nodeRadius,
                                          transform.position.y + i * nodeDiameter + nodeRadius, 
                                          transform.position.z + -0.7f);

                GameObject o = Instantiate(nodeObj, pos, Quaternion.identity) as GameObject;
                o.transform.parent = transform;
                o.name = iter.ToString();

                bool walkable = !(Physics.CheckSphere(pos, nodeRadius, unwalkableMask));

                AddNode(iter, o, pos.x, pos.y, walkable);
                nodes[iter].gridX = iter % cols;//(iter + 1) % cols;
                nodes[iter].gridY = iter / cols;//(iter + 1) / cols;

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

        nodeObj.transform.localScale = new Vector3(nodeRadius, nodeRadius, nodeRadius);

        CreateGraph();

        container = new GameObject();
        container.name = "Path";
        container.transform.parent = transform;
    }

    void Update()
    {
        if(newPath == true)
        {
            newPath = false;
            startPt = Random.Range(0, MaxSize);
            goalPt = Random.Range(0, MaxSize);
            if (goalPt == startPt)
                goalPt = Random.Range(0, MaxSize);

            foreach(Node n in nodes)
            {
                n.render.GetComponentInChildren<Renderer>().material = unexpMat;
            }
            Solver(searchMethod);
                
        }
    }

    void Solver(int method = 3)
    {
        if (method == 3)
            StartAStar(nodes[startPt], nodes[goalPt]);
        if (method == 2)
            StartDijkstra(nodes[startPt], nodes[goalPt]);
        if (method == 1)
            StartBFS(nodes[startPt], nodes[goalPt]);
        if (method == 0)
            StartDFS(nodes[startPt], nodes[goalPt]);
        
    }

    void StartDFS(Node start, Node goal)
    {
        StartCoroutine(DFS(start, goal));
    }

    void StartBFS(Node start, Node goal)
    {
        StartCoroutine(BFS(start, goal));
    }

    void StartDijkstra(Node start, Node goal)
    {
        StartCoroutine(Dijkstra(start, goal));
    }

    void StartAStar(Node start, Node goal)
    {
        StartCoroutine(AStar(start, goal));
    }

    IEnumerator DFS(Node startNode, Node goalNode)
    {
        if (path != null)
        {
            foreach (Transform child in container.transform)
                Destroy(child.gameObject);
        }

        foreach (Node n in nodes)
        {
            n.gCost = 0;
            n.parent = null;
        }
        
        Stopwatch sw = new Stopwatch();
        sw.Start();
        Stack<Node> unexplored = new Stack<Node>();
        HashSet<Node> explored = new HashSet<Node>();
        
        startNode.render.GetComponentInChildren<Renderer>().material = startMat;
        goalNode.render.GetComponentInChildren<Renderer>().material = selectMat;

        unexplored.Push(startNode);
        while (unexplored.Count != 0)
        {
            Node currentNode = unexplored.Pop();
            explored.Add(currentNode);

            if (currentNode == goalNode)
            {
                sw.Stop();
                GetPath(startNode, goalNode);
                break;
            }
            
            //change current node's material to explored (if !startnode)
            if (currentNode != startNode && currentNode != goalNode)
                currentNode.render.GetComponentInChildren<Renderer>().material = expMat;

            yield return new WaitForSeconds(delay);

            foreach (Edge c in currentNode.connections)
            {
                if (c.connection.gCost == 0)
                {
                    c.connection.gCost = currentNode.gCost + 1;
                    c.connection.parent = currentNode;
                    unexplored.Push(c.connection);
                }
            }
        }

        yield return new WaitForSeconds(delay * 3);
        DrawPath(startNode);
        yield return new WaitForSeconds(delay * 6);
        newPath = true;
    }

    IEnumerator BFS(Node startNode, Node goalNode)
    {
        if (path != null)
        {
            foreach (Transform child in container.transform)
                Destroy(child.gameObject);
        }

        foreach (Node n in nodes)
        {
            n.gCost = 0;
            n.parent = null;
        }
        
        Stopwatch sw = new Stopwatch();
        sw.Start();
        Queue<Node> unexplored = new Queue<Node>();
        HashSet<Node> explored = new HashSet<Node>();
        
        startNode.render.GetComponentInChildren<Renderer>().material = startMat;
        goalNode.render.GetComponentInChildren<Renderer>().material = selectMat;

        unexplored.Enqueue(startNode);
        while (unexplored.Count != 0)
        {
            Node currentNode = unexplored.Dequeue();
            explored.Add(currentNode);

            if (currentNode == goalNode)
            {
                sw.Stop();
                GetPath(startNode, goalNode);
                break;
            }

            //change current node's material to explored (if current index != startNode index)
            if (currentNode != startNode && currentNode != goalNode)
                currentNode.render.GetComponentInChildren<Renderer>().material = expMat;

            yield return new WaitForSeconds(delay);

            foreach (Edge c in currentNode.connections)
            {
                if (c.connection.gCost == 0 && !explored.Contains(c.connection))
                {
                    c.connection.gCost = currentNode.gCost + 1;
                    c.connection.parent = currentNode;
                    unexplored.Enqueue(c.connection);
                }
                    
            }
        }

        yield return new WaitForSeconds(delay * 3);
        DrawPath(startNode);
        yield return new WaitForSeconds(delay * 6);
        newPath = true;
    }

    IEnumerator Dijkstra(Node startNode, Node goalNode)
    {
        if (path != null)
        {
            foreach (Transform child in container.transform)
                Destroy(child.gameObject);
        }
        
        Stopwatch sw = new Stopwatch();
        sw.Start();
        Heap<Node> unexplored = new Heap<Node>(MaxSize);
        HashSet<Node> explored = new HashSet<Node>();   //HashSet ~1600% faster than list in this case

        //set startNode material
        startNode.render.GetComponentInChildren<Renderer>().material = startMat;
        goalNode.render.GetComponentInChildren<Renderer>().material = selectMat;

        unexplored.Add(startNode);
        while (unexplored.Count != 0)
        {
            Node currentNode = unexplored.RemoveFirst();
            explored.Add(currentNode);

            if (currentNode == goalNode)
            {
                sw.Stop();
                GetPath(startNode, goalNode);
                break;
            }

            //change current node's material to explored (if !startnode)
            if (currentNode != startNode && currentNode != goalNode)
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
                    edge.connection.hCost = 0;
                    edge.connection.parent = currentNode;

                    if (!unexplored.Contains(edge.connection))
                        unexplored.Add(edge.connection);
                    else
                        unexplored.UpdateItem(edge.connection);
                }
            }
        }

        //pause before displaying path
        yield return new WaitForSeconds(delay * 3);
        DrawPath(startNode);
        yield return new WaitForSeconds(delay * 6);        
        newPath = true;
    }

    IEnumerator AStar(Node startNode, Node goalNode)
    {
        if (path != null)
        {
            foreach (Transform child in container.transform)
                Destroy(child.gameObject);
        }
        
        Stopwatch sw = new Stopwatch();
        sw.Start();
        Heap<Node> unexplored = new Heap<Node>(MaxSize);
        HashSet<Node> explored = new HashSet<Node>();   //HashSet ~1600% faster than list in this case

        //set startNode & goalNode material
        startNode.render.GetComponentInChildren<Renderer>().material = startMat;
        goalNode.render.GetComponentInChildren<Renderer>().material = selectMat;

        unexplored.Add(startNode);
        while (unexplored.Count != 0)
        {
            Node currentNode = unexplored.RemoveFirst();
            explored.Add(currentNode);
            if (currentNode == goalNode)
            {
                sw.Stop();
                GetPath(startNode, goalNode);
                break;
            }

            //change current node's material to explored (if !startnode)
            if (currentNode != startNode && currentNode != goalNode)
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
                    edge.connection.hCost = GetDistance(edge.connection, goalNode);
                    edge.connection.parent = currentNode;

                    if (!unexplored.Contains(edge.connection))
                        unexplored.Add(edge.connection);
                    else
                        unexplored.UpdateItem(edge.connection);
                }
            }
        }

        //pause before displaying path
        yield return new WaitForSeconds(delay * 3);
        DrawPath(startNode);
        yield return new WaitForSeconds(delay * 6);
        newPath = true;
    }

    void DrawPath(Node start)
    {
        path.Insert(0, start);
        for (int i = 0; i < path.Count; ++i)
        {
            if (i == 0)
                continue;

            GameObject g = Instantiate(line);
            LineRenderer l = g.GetComponent<LineRenderer>();
            Color c = new Color(255, 0, 0, 128);
            l.SetColors(c, c);
            l.SetWidth(0.2f, 0.2f);
            l.SetPosition(0, path[i - 1].render.transform.position);
            l.SetPosition(1, path[i].render.transform.position);
            connectLines.Add(l);
            connectLines[connectLines.Count - 1].transform.parent = container.transform;
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
    }    

    int GetDistance(Node a, Node b)
    {
        int distX = Mathf.Abs(a.gridX - b.gridX);
        int distY = Mathf.Abs(a.gridY - b.gridY);

        //10 and 14 come from the root where a straight line is
        // 1 and a diagonal line is the square of 2
        if (distX > distY)
            return 14 * distY + 10 * (distX - distY);
        return 14 * distX + 10 * (distY - distX);
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
}
