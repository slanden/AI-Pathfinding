using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Graph : MonoBehaviour
{
    public GameObject nodeObj;
    public int resolution;
    public int rows, cols;
    private List<Node> nodes = new List<Node>();
    
    public float delay = 0.08f;
    public bool newPath = false;
    public int startingPt = 0;
    public int searchMethod = 0;

    //materials
    public Material expMat, unexpMat, selectMat, startMat;
    public List<LineRenderer> connectLines = new List<LineRenderer>();
    public GameObject line;

    struct Node
    {
        public int value;
        public Vector2 position;
        public float gscore;
        public List<Edge> connections;
        public GameObject render;

        public Node(GameObject r, float x = 0, float y = 0, int val = 0, int score = 0)
        {
            render = r;
            value = val;
            gscore = score;
            position = new Vector2(x, y);
            connections = new List<Edge>();
        }
    }

    struct Edge
    {
        public Node connection;
        public int cost;

        public Edge(Node node, int cost = 0)
        {
            connection = node;
            this.cost = cost;
        }
    }

    void AddNode(GameObject r, float x = 0, float y = 0, int value = 0, int _score = 0)
    {
        nodes.Add(new Node(r, x, y, value, _score));
        //return nodes[nodes.Count-1];
    }

    void AddConnection(Node n1, Node n2, int c = 1)
    {
        n1.connections.Add(new Edge(n2, c));
        n2.connections.Add(new Edge(n1, c));
    }

    //public Graph() { }
    //public Graph(int resolution)
    //{
    //    int i = 0;
    //    for(int row = 0; row < resolution; ++row)
    //    {
    //        for(int col = 0; col < resolution; ++col)
    //        {
    //            //AddNode(i);
    //            //i++;
    //            Node n = new Node();
    //            n.value = i;
    //            nodes.Add(n);
    //            i++;
    //        }
    //    }
    //}

    void Start()
    {
        int iter = 0;
        for (int i = 0; i < rows; ++i)
        {
            for (int j = 0; j < cols; ++j)
            {
                //float x = (j + 1) * 0.75f / (cols + 1);
                //float y = (i + 1) * 0.75f / (rows + 1);
                float x = j * 0.75f;
                float y = i * 0.75f;

                GameObject o = Instantiate(nodeObj, new Vector3(x, y, transform.position.z),
                                           Quaternion.identity) as GameObject;
                o.transform.parent = transform;
                o.name = iter.ToString();

                AddNode(o, x, y, iter, 1);
                //print("Node " + nodes[iter].value + " position: " + nodes[iter].position);

                //vertical
                if (i != 0)
                {
                    Node _n1 = nodes[(i - 1) * cols + j];
                    Node _n2 = nodes[i * cols + j];
                    AddConnection(nodes[(i - 1) * cols + j], nodes[i * cols + j], 10);

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
                    AddConnection(nodes[i * cols + j - 1], nodes[i * cols + j], 10);

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
                    AddConnection(nodes[(i - 1) * cols + j - 1], nodes[i * cols + j], 14);

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
                    AddConnection(nodes[(i - 1) * cols + j + 1], nodes[i * cols + j], 14);

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

        //foreach( Node n in nodes)
        //{
        //    print("Node " + n.value);
        //    foreach (Edge e in n.connections)
        //    {
        //        print(" - connection to Node " + e.connection.value);
        //    }
        //}

        //Node a, b, c, d, e, f, g, h, i, j;

        //a = AddNode('a');
        //b = AddNode('b');
        //c = AddNode('c');
        //d = AddNode('d');
        //e = AddNode('e');
        //f = AddNode('f');
        //g = AddNode('g');
        //h = AddNode('h');
        //i = AddNode('i');
        //j = AddNode('j');

        //AddConnection(a, b);
        //AddConnection(a, d);
        //AddConnection(a, e);
        //AddConnection(b, c);
        //AddConnection(d, h);
        //AddConnection(e, h);
        //AddConnection(e, f);
        //AddConnection(f, c);
        //AddConnection(f, j);
        //AddConnection(g, c);
        //AddConnection(g, j);
        //AddConnection(i, j);
        //AddConnection(i, h);

        Solver(startingPt, searchMethod);
    }

    void Solver(int start, int method = 0)
    {
        //DFS
        if(method == 0)
            StartCoroutine(DFS(start));
        if (method == 1)
            StartCoroutine(BFS(start));
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
            print("Node " + current.value);

            explored.Add(current);
            //change current node's material to explored (if !startnode)
            if (current.value != startNode)
                current.render.GetComponentInChildren<Renderer>().material = expMat;

            yield return new WaitForSeconds(delay);

            foreach (Edge c in current.connections)
            {
                if (!explored.Contains(c.connection))
                {
                    unexplored.Push(c.connection);
                    print("Add Connection " + c.connection.value);
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
            print("Node " + current.value);

            explored.Add(current);
            //change current node's material to explored (if !startnode)
            if (current.value != startNode)
                current.render.GetComponentInChildren<Renderer>().material = expMat;

            yield return new WaitForSeconds(delay);

            foreach (Edge c in current.connections)
            {
                if (!explored.Contains(c.connection))
                {
                    unexplored.Enqueue(c.connection);
                    print("Add Connection " + c.connection.value);
                    //yield return new WaitForSeconds(delay);
                }
            }
            //yield return new WaitForSeconds(delay);
        }
    }

    void Dijkstra(Node startNode)
    {

    }
}
