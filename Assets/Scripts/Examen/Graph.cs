using System.Collections.Generic;
using UnityEngine;

public class Graph : MonoBehaviour
{
    [SerializeField] List<Graph> _neighbors;
    public List<Graph> Neighbors => _neighbors;

    [SerializeField] LayerMask _obstacleMask;

    private int _x, _y;
    public int X => _x;
    public int Y => _y;

    [SerializeField] private bool _isWall;
    public bool IsWall => _isWall;

    [SerializeField] int _cost;
    public int Cost => _cost;

    Color _defaultColor;
    public Color DefaultColor => _defaultColor;

    MeshRenderer _meshRenderer;

    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _defaultColor = _meshRenderer.material.color;

        FindAllGraph();
        ConnectNodes();
    }

    public void SetCost(int value)
    {
        _cost = value;
    }

    public void SetCoordinates(int x, int y)
    {
        _x = x;
        _y = y;
    }

    public Color Color
    {
        set { GetComponent<MeshRenderer>().material.color = value; }
    }

    public void AddNeighbors(Graph neighbor)
    {
        _neighbors.Add(neighbor);
    }

    private void FindAllGraph()
    {
        Graph[] allGraphs = FindObjectsOfType<Graph>();
        _neighbors = new List<Graph>();

        foreach (Graph graph in allGraphs)
        {
            if (graph == this) continue;
            _neighbors.Add(graph);
        }
    }

    private void ConnectNodes()
    {
        for (int i = _neighbors.Count - 1; i >= 0; i--)
        {
            if (Physics.Linecast(transform.position, _neighbors[i].transform.position, _obstacleMask))
                _neighbors.RemoveAt(i);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = _isWall ? Color.red : Color.cyan;

        if (_neighbors == null) return;

        foreach (var neighbor in _neighbors)
        {
            if (neighbor == null) continue;

            if (!Physics.Linecast(transform.position, neighbor.transform.position, _obstacleMask))
                Gizmos.DrawLine(transform.position, neighbor.transform.position);
        }
    }
}
