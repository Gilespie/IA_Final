using System.Collections.Generic;
using UnityEngine;

public class PathManagerExamen : MonoBehaviour
{
    public static PathManagerExamen Instance;

    [SerializeField] private Graph[] _allNodes;
    [SerializeField] private LayerMask _obstacleMask;

    public Graph[] AllNodes => _allNodes;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public List<Graph> GetPath(Vector3 from, Vector3 to)
    {
        var start = Closest(from);
        var end = Closest(to);

        if (start == null || end == null)
            return new List<Graph>();

        var path = PathfindingExamen.ThetaStar(start, end, _obstacleMask);

        for (int i = 0; i < path.Count; i++)
        {
            Color c = Color.Lerp(Color.cyan, Color.green, (float)i / path.Count);
            path[i].SetColor(c);
        }

        return path;
    }

    public Graph Closest(Vector3 pos)
    {
        Graph best = null;
        float min = Mathf.Infinity;

        foreach (var node in _allNodes)
        {
            float dist = (node.transform.position - pos).sqrMagnitude;
            if (dist < min)
            {
                best = node;
                min = dist;
            }
        }
        return best;
    }
}
