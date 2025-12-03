using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
/*    public static List<Graph> BFS(Graph start, Graph end)
    {
        var frontier = new Queue<Graph>();
        frontier.Enqueue(start);

        var camefrom = new Dictionary<Graph, Graph>();
        camefrom[start] = null;

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();
            current.Color = Color.magenta;

            if (current == end)
            {
                var path = new List<Graph>();

                while (current != null)
                {
                    path.Add(current);
                    current = camefrom[current];
                }

                path.Reverse();
                return path;
            }

            for (int i = 0; i < current.Neighbors.Count; i++)
            {
                var next = current.Neighbors[i];

                if (camefrom.ContainsKey(next)) continue;
                frontier.Enqueue(next);
                camefrom[next] = current;
            }
        }

        return new List<Graph>();
    }*/

    /*    public static List<Graph> DFS(Graph start, Graph end)
        {
            var frontier = new Stack<Graph>();
            frontier.Push(start);

            var camefrom = new Dictionary<Graph, Graph>();
            camefrom[start] = null;

            while (frontier.Count > 0)
            {
                var current = frontier.Pop();
                current.Color = Color.magenta;

                if (current == end)
                {
                    var path = new List<Graph>();

                    while (current != null)
                    {
                        path.Add(current);
                        current = camefrom[current];
                    }

                    path.Reverse();
                    return path;
                }

                for (int i = 0; i < current.Neighbors.Count; i++)
                {
                    var next = current.Neighbors[i];

                    if (camefrom.ContainsKey(next)) continue;
                    frontier.Push(next);
                    camefrom[next] = current;
                }
            }

            return new List<Graph>();
        }*/

    /*    public static List<Graph> AlgoritmoDijkstra(Graph start, Graph end) //costo path
        {
            var frontier = new PriorityQueue<Graph>();
            frontier.Enqueue(start, 0);

            var camefrom = new Dictionary<Graph, Graph>();
            camefrom[start] = null;

            var costPath = new Dictionary<Graph, float>();
            costPath[start] = 0;

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
                        current = camefrom[current];
                    }

                    path.Reverse();

                    return path;
                }

                for (int i = 0; i < current.Neighbors.Count; i++)
                {
                    var next = current.Neighbors[i];
                    var newCost = costPath[current] + next.Cost;

                    if (!camefrom.ContainsKey(next) || newCost < costPath[next])
                    {
                        costPath[next] = newCost;
                        var priority = newCost;
                        frontier.Enqueue(next, priority);
                        camefrom[next] = current;
                    }
                }
            }

            return new List<Graph>();
        }*/

    public static float Heuristic(Graph a, Graph b)
    {
        //distance Manhattan
        return Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y);
    }

/*    public static List<Graph> GreedyBestFirst(Graph start, Graph end) //costo path
    {
        var frontier = new PriorityQueue<Graph>();
        frontier.Enqueue(start, 0);

        var camefrom = new Dictionary<Graph, Graph>();
        camefrom[start] = null;


        while (!frontier.IsEmpty)
        {
            var current = frontier.Dequeue();
            current.Color = Color.blue;//Color.Lerp(Color.red, Color.green, (float)current.Cost / 5);

            if (current == end)
            {
                var path = new List<Graph>();

                while (current != null)
                {
                    path.Add(current);
                    current = camefrom[current];
                }

                path.Reverse();
                return path;
            }

            for (int i = 0; i < current.Neighbors.Count; i++)
            {
                var next = current.Neighbors[i];

                if (next.IsWall) continue;

                if (!camefrom.ContainsKey(next))
                {
                    var priority = Heuristic(next, end);
                    frontier.Enqueue(next, priority);
                    camefrom[next] = current;
                }
            }
        }

        return new List<Graph>();
    }*/

    public static List<Graph> Astar(Graph start, Graph end, LayerMask mask)
    {
        var frontier = new PriorityQueue<Graph>();
        frontier.Enqueue(start, 0);

        var camefrom = new Dictionary<Graph, Graph>();
        camefrom[start] = null;

        var costPath = new Dictionary<Graph, float>();
        costPath[start] = 0;

        while (!frontier.IsEmpty)
        {
            var current = frontier.Dequeue();
            current.Color = Color.Lerp(Color.red, Color.green, (float)current.Cost / 5);

            if (current == end)
            {
                var path = new List<Graph>();

                while (current != null)//Puede ser != start si no queremos agregar start al path
                {
                    path.Add(current);
                    current = camefrom[current];
                }

                path.Reverse();
                return path;
            }

            for (int i = 0; i < current.Neighbors.Count; i++)
            {
                var next = current.Neighbors[i];

                if (!InSight(current, next, mask)) continue;

                var newCost = costPath[current] + next.Cost;

                if (!camefrom.ContainsKey(next) || newCost < costPath[next])
                {
                    costPath[next] = newCost;
                    var priority = newCost + Heuristic(next, end);

                    frontier.Enqueue(next, priority);
                    camefrom[next] = current;
                }
            }
        }

        return new List<Graph>();
    }

    private static bool InSight(Graph a, Graph b, LayerMask mask)
    {
        return !Physics.Linecast(a.transform.position, b.transform.position, mask);
    }

/*    public static List<Graph> AstarPS(Graph start, Graph end, LayerMask obstacleMask)
    {
        var path = Astar(start, end, obstacleMask);
        int current = 0;
        while (current + 2 < path.Count)
        {
            if (InSight(path[current], path[current + 2], obstacleMask))
            {
                path.RemoveAt(current + 1);
            }
            else
                current++;
        }
        return path;
    }*/

    /*    public static List<Graph> ThetaStar(Graph start, Graph end, LayerMask obstacleMask)
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
                current.Color = Color.Lerp(Color.blue, Color.black, (float)current.Cost / 5); ;

                if (current == end)
                {
                    var path = new List<Graph>();
                    while (current != null) //Puede ser != start si no queremos agregar start al path
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
                    if (!InSight(current, next, obstacleMask)) continue;

                    if (cameFrom.ContainsKey(current) && cameFrom[current] != null && InSight(next, cameFrom[current], obstacleMask))
                        realParent = cameFrom[current];

                    var newCost = costSoFar[realParent] + DistanceCost(next, realParent);
                    if (!cameFrom.ContainsKey(next) || newCost < costSoFar[next])
                    {
                        costSoFar[next] = newCost;
                        var priority = newCost + DistanceHeuristic(next, end);
                        frontier.Enqueue(next, priority);
                        cameFrom[next] = realParent;
                    }
                }
            }
            return new List<Graph>();
        }*/

    private static float DistanceHeuristic(Graph node, Graph end)
    {
        return Vector3.Distance(node.transform.position, end.transform.position);

    }
    private static float DistanceCost(Graph a, Graph b)
    {
        return Vector3.Distance(a.transform.position, b.transform.position);
    }
}