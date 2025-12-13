using System.Collections.Generic;
using UnityEngine;

public class Graph : MonoBehaviour
{
    [SerializeField] List<Graph> _neighbors;
    public List<Graph> Neighbors => _neighbors;

    [SerializeField] LayerMask _obstacleMask;

    [SerializeField] float _connectRadius = 10f;

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

        //FindAllGraph();
        //ConnectNodes();

        FindAndConnectNeighbors();
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
    { set { GetComponent<MeshRenderer>().material.color = value; } }


    public void AddNeighbors(Graph neighbor)
    {
        _neighbors.Add(neighbor);
    }

    private void FindAndConnectNeighbors()
    {
        _neighbors = new List<Graph>();

        Collider[] hits = Physics.OverlapSphere(transform.position, _connectRadius);

        foreach (var hit in hits)
        {
            Graph neighbor = hit.GetComponent<Graph>();
            if (neighbor == null || neighbor == this) continue;
            if (neighbor.IsWall) continue;

            if (!Physics.Linecast(transform.position, neighbor.transform.position, _obstacleMask))
            {
                _neighbors.Add(neighbor);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white * 0.3f;
        Gizmos.DrawWireSphere(transform.position, _connectRadius);

        Gizmos.color = _isWall ? Color.red * 0.3f : Color.cyan * 0.3f;

        if (_neighbors == null) return;

        foreach (var neighbor in _neighbors)
        {
            if (neighbor == null) continue;

            Gizmos.DrawLine(transform.position, neighbor.transform.position);
        }
    }
}