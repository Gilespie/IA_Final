using System.Collections.Generic;
using UnityEngine;

public class PathManagerExamen : MonoBehaviour
{
    public static PathManagerExamen Instance;

    [SerializeField] Graph[] _allGraphs;
    public Graph[] AllNodes => _allGraphs;
    [SerializeField] LayerMask _obstacleMask;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public List<Graph> GetPath(Vector3 startPos, Vector3 endPos)
    {
        var start = Closest(startPos);
        var end = Closest(endPos);
        var path = PathfindingExamen.ThetaStar(start, end, _obstacleMask);

        for (int i = 0; i < path.Count; i++)
        {
            path[i].Color = Color.Lerp(path[i].DefaultColor, Color.green, (float)i / path.Count);
        }

        return path;
    }

    public Graph Closest(Vector3 pos)
    {
        float minDistance = int.MaxValue;
        Graph closest = null;

        for (int i = 0; i < _allGraphs.Length; i++)
        {
            var node = _allGraphs[i];
            var distance = (node.transform.position - pos).sqrMagnitude;

            if (distance < minDistance)
            {
                closest = node;
                minDistance = distance;
            }
        }
        return closest;
    }
}
