using System.Collections.Generic;
using UnityEngine;

public static class PathfindingExamen
{
    private static bool HasLineOfSight(Graph a, Graph b, LayerMask mask)
    {
        return !Physics.Linecast(a.transform.position, b.transform.position, mask);
    }

    public static List<Graph> ThetaStar(Graph start, Graph end, LayerMask obstacleMask)
    {
        var frontier = new PriorityQueue<Graph>();
        var cameFrom = new Dictionary<Graph, Graph>();
        var costSoFar = new Dictionary<Graph, float>();

        frontier.Enqueue(start, 0);
        cameFrom[start] = null;
        costSoFar[start] = 0;

        while (!frontier.IsEmpty)
        {
            Graph current = frontier.Dequeue();

            if (current == end)
                return ReconstructPath(cameFrom, end);

            foreach (var next in current.Neighbors)
            {
                Graph parent = cameFrom[current];

                if (parent != null && HasLineOfSight(parent, next, obstacleMask))
                    current = parent;

                float newCost = costSoFar[current] + Vector3.Distance(current.transform.position, next.transform.position);

                if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                {
                    costSoFar[next] = newCost;
                    float priority = newCost + Vector3.Distance(next.transform.position, end.transform.position);

                    frontier.Enqueue(next, priority);
                    cameFrom[next] = current;
                }
            }
        }

        return new List<Graph>();
    }

    private static List<Graph> ReconstructPath(Dictionary<Graph, Graph> cameFrom, Graph end)
    {
        List<Graph> path = new();
        Graph current = end;

        while (current != null)
        {
            path.Add(current);
            current = cameFrom[current];
        }

        path.Reverse();
        return path;
    }
}
