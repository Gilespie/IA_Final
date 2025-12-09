using System.Collections.Generic;
using UnityEngine;

public class Graph : MonoBehaviour
{
    [SerializeField] private bool _isWall;
    public bool IsWall => _isWall;

    [SerializeField] private LayerMask _obstacleMask;

    public List<Graph> Neighbors { get; private set; } = new();

    public int X { get; private set; }
    public int Y { get; private set; }

    private Color _defaultColor;
    public Color DefaultColor => _defaultColor;

    private MeshRenderer _renderer;

    private void Awake()
    {
        _renderer = GetComponent<MeshRenderer>();
        if (_renderer != null)
            _defaultColor = _renderer.material.color;

        FindCoordinates();
        BuildNeighbors();
    }

    private void FindCoordinates()
    {
        X = Mathf.RoundToInt(transform.position.x);
        Y = Mathf.RoundToInt(transform.position.z);
    }

    private void BuildNeighbors()
    {
        Graph[] all = FindObjectsOfType<Graph>();

        foreach (var node in all)
        {
            if (node == this) continue;

            if (!Physics.Linecast(transform.position, node.transform.position, _obstacleMask))
            {
                Neighbors.Add(node);
            }
        }
    }

    public void SetColor(Color c)
    {
        if (_renderer != null)
            _renderer.material.color = c;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = _isWall ? Color.red : Color.cyan;

        foreach (var n in Neighbors)
        {
            if (n == null) continue;

            Gizmos.DrawLine(transform.position, n.transform.position);
        }
    }
}
