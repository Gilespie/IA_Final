using System.Collections;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private float _width;
    [SerializeField] private float _height;
    [SerializeField] private float _delaySpawn = 3f;
    [SerializeField] private AppleFoodFactory _appleFoodFactory;
    private bool _isSpawning = true;

    private void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while(_isSpawning)
        {
            yield return new WaitForSeconds(_delaySpawn);
            var obj = _appleFoodFactory.Create();
            obj.transform.position = new Vector3(Random.Range(-_width/2,_width/2), 0.5f, Random.Range(-_height/2, _height/2));
            yield return null;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, new Vector3(_width, 0, _height));
    }
}