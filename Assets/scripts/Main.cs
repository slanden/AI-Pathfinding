using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    public Button btnDFS, btnBFS, btnDjks, btnAStar;
    Graph graph;

    void Start()
    {
        graph = FindObjectOfType<Graph>();

        btnDFS.onClick.AddListener(SetDFS);
        btnBFS.onClick.AddListener(SetDFS);
        btnDjks.onClick.AddListener(SetDjks);
        btnAStar.onClick.AddListener(SetAStar);

    }

    // 0 = DFS, 1 = BFS, 2 = Dijkstra's, 3 = A*
    void SetDFS()
    {
        graph.searchMethod = 0;
    }

    void SetBFS()
    {
        graph.searchMethod = 1;
    }

    void SetDjks()
    {
        graph.searchMethod = 2;
    }

    void SetAStar()
    {
        graph.searchMethod = 3;
    }
}
