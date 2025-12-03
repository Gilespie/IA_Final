using UnityEngine;

/*public class GridPathfinding : MonoBehaviour
{
    [SerializeField] Graph _prefab;
    [SerializeField] Graph[] _grid;
    [SerializeField] float _separation;
    [SerializeField] int _width;
    [SerializeField] int _height;
    [SerializeField] int _baseCost = 1;
    public Graph[] Grid => _grid;


    [ContextMenu("Create Grid")]
    public void CreateGrid()
    {
        _grid = new Graph[_width * _height];

        for (int i = 0; i < _width; i++)
        {
            for (int j = 0; j < _height; j++)
            {
                var obj = Instantiate(_prefab, transform.position + new Vector3(i * _separation, 0, j * _separation), Quaternion.identity, transform);
                obj.SetCoordinates(i,j);
                _grid[i * _width + j] = obj;
            }
        }

        for (int i = 0; i < _width; i++)
        {
            for (int j = 0; j < _height; j++)
            {
                Graph graph = _grid[i * _width + j];

                if (i > 0) graph.AddNeighbors(_grid[(i-1) * _width + j]);
                if (i < _width - 1) graph.AddNeighbors(_grid[(i+1) * _width + j]);
                if (j > 0) graph.AddNeighbors(_grid[i * _width + j - 1]);
                if (i < _height - 1) graph.AddNeighbors(_grid[i * _width + j + 1]);
                
            }
        }
    }

    [ContextMenu("Delete Grid")]
    public void DeleteGrid()
    {
        for(int i = 0; i < _grid.Length;i++)
        {
            DestroyImmediate(_grid[i].gameObject);
        }
        _grid = null;
    }

    [ContextMenu("Set all cost")]
    public void DSetAllCost()
    {
        for (int i = 0; i < _grid.Length; i++)
        {
            _grid[i].SetCost(_baseCost);
        }
    }
}*/