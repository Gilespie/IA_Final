using System;
using System.Collections.Generic;

public class PriorityQueue<TData>
{
    public bool IsEmpty { get { return _data.Count < 1; } }

    private List<Tuple<TData, float>> _data;
    private Dictionary<TData, int> _indexes;
    private Func<float, float, bool> _critery;

    public PriorityQueue()
    {
        _data = new List<Tuple<TData, float>>();
        _indexes = new Dictionary<TData, int>();
        _critery = (x, y) => x.CompareTo(y) < 0;
    }

    public PriorityQueue(Func<float, float, bool> critery)
    {
        _data = new List<Tuple<TData, float>>();
        _indexes = new Dictionary<TData, int>();
        _critery = critery;
    }

    public void Enqueue(TData data, float priority)
    {
        Enqueue(new Tuple<TData, float>(data, priority));
    }

    public void Enqueue(Tuple<TData, float> dp)
    {
        int currentIndex;
        int parentIndex;

        if (_indexes.ContainsKey(dp.Item1))
        {
            currentIndex = _indexes[dp.Item1];
            parentIndex = (currentIndex - 1) / 2;

            if (_critery(_data[currentIndex].Item2, dp.Item2)) return;

            _data[currentIndex] = dp;
        }
        else
        {
            _data.Add(dp);

            currentIndex = _data.Count - 1;
            parentIndex = (currentIndex - 1) / 2;

            _indexes.Add(dp.Item1, currentIndex);
        }

        while (currentIndex > 0 && _critery(_data[currentIndex].Item2, _data[parentIndex].Item2))
        {
            Swap(currentIndex, parentIndex);

            currentIndex = parentIndex;
            parentIndex = (currentIndex - 1) / 2;
        }
    }

    private void EnqueueData(Tuple<TData, float> dp)
    {
        _data.Add(dp);

        int currentIndex = _data.Count - 1;//La posicion del dato recien ingresado en la lista.
        int parentIndex = (currentIndex - 1) / 2; //La posicion del nodo padre en la lista. 

        _indexes.Add(dp.Item1, currentIndex);

        while (currentIndex > 0 && _critery(_data[currentIndex].Item2, _data[parentIndex].Item2))
        {
            Swap(currentIndex, parentIndex);

            currentIndex = parentIndex;
            parentIndex = (currentIndex - 1) / 2;
        }
    }

    public TData Peek()
    {
        return PeekTuple().Item1;
    }

    public Tuple<TData, float> PeekTuple()
    {
        return _data[0];
    }

    public TData Dequeue()
    {
        return DequeueTuple().Item1;
    }

    public Tuple<TData, float> DequeueTuple()
    {
        var date = _data[0];

        _data[0] = _data[_data.Count - 1];
        _indexes[_data[0].Item1] = 0;

        _data.RemoveAt(_data.Count - 1);
        _indexes.Remove(date.Item1);

        int currentIndex = 0;
        int leftIndex = 1;
        int rightIndex = 2;
        int explorIndex = GetExplorerIndex(leftIndex, rightIndex);


        if (explorIndex == -1) return date;

        while (_critery(_data[explorIndex].Item2, _data[currentIndex].Item2))
        {
            Swap(explorIndex, currentIndex);

            currentIndex = explorIndex;
            leftIndex = (currentIndex * 2) + 1;
            rightIndex = (currentIndex * 2) + 2;
            explorIndex = GetExplorerIndex(leftIndex, rightIndex);

            if (explorIndex == -1) break;
        }
        return date;
    }

    private int GetExplorerIndex(int leftIndex, int rightIndex)
    {
        if (_data.Count > rightIndex)
            return _critery(_data[leftIndex].Item2, _data[rightIndex].Item2) ? leftIndex : rightIndex;
        else if (_data.Count > leftIndex)
            return leftIndex;

        return -1;
    }

    private void Swap(int from, int to)
    {
        //Swapeo referencia de indices en diccionario.
        _indexes[_data[from].Item1] = to;
        _indexes[_data[to].Item1] = from;

        //swapeo objetos en la lista.
        var aux = _data[from];
        _data[from] = _data[to];
        _data[to] = aux;
    }

    #region OLD
    //Dictionary<Graph, float> _allNodes = new Dictionary<Graph, float>();

    /*public int Count => _allNodes.Count;

    public void Put(Graph graph, float cost)
    {
        if(_allNodes.ContainsKey(graph)) _allNodes[graph] = cost;
        else _allNodes.Add(graph, cost);
    }

    public Graph Get()
    {
        Graph graph = null;
        float lowestValue = Mathf.Infinity;

        foreach (var item in _allNodes)
        {
            if (item.Value < lowestValue)
            {
                lowestValue = item.Value;
                graph = item.Key;
            }
        }

        _allNodes.Remove(graph);
        return graph;
    }
*/

    #endregion
}