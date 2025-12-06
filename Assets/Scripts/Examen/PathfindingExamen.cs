using System.Collections.Generic;
using UnityEngine;

public class PathfindingExamen : MonoBehaviour
{
    public static float Heuristic(Graph a, Graph b)
    {
        return Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y);
    }

    private static bool InSight(Graph a, Graph b, LayerMask mask)
    {
        return !Physics.Linecast(a.transform.position, b.transform.position, mask);
    }

    public static List<Graph> ThetaStar(Graph start, Graph end, LayerMask obstacleMask)
    {
        var frontier = new PriorityQueue<Graph>();
        frontier.Enqueue(start, 0);
        var cameFrom = new Dictionary<Graph, Graph>();
        cameFrom[start] = null;
        var costSoFar = new Dictionary<Graph, float>();
        costSoFar[start] = 0;

        while (!frontier.IsEmpty)
        {
            var current = frontier.Dequeue();
            current.Color = Color.Lerp(Color.blue, Color.black, (float)current.Cost / 5);

            if (current == end)
            {
                var path = new List<Graph>();
                while (current != null)
                {
                    path.Add(current);
                    current = cameFrom[current];
                }
                path.Reverse();
                return path;
            }

            for (int i = 0; i < current.Neighbors.Count; i++)
            {
                var next = current.Neighbors[i];
                var realParent = current;

                if (cameFrom.ContainsKey(current) && cameFrom[current] != null && InSight(next, cameFrom[current], obstacleMask))
                {
                    realParent = cameFrom[current];
                }

                var newCost = costSoFar[realParent] + DistanceCost(next, realParent);
                if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                {
                    costSoFar[next] = newCost;
                    var priority = newCost + DistanceHeuristic(next, end);
                    frontier.Enqueue(next, priority);
                    cameFrom[next] = realParent;
                }
            }
        }
        return new List<Graph>();
    }

    private static float DistanceHeuristic(Graph node, Graph end)
    {
        return Vector3.Distance(node.transform.position, end.transform.position);
    }

    private static float DistanceCost(Graph a, Graph b)
    {
        return Vector3.Distance(a.transform.position, b.transform.position);
    }
}
