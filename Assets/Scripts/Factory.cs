using UnityEngine;

public abstract class Factory<T> : MonoBehaviour
{
    [SerializeField] protected T _prefab;
    [SerializeField] protected int _initialStock;
    protected ObjectPool<T> _pool;

    protected virtual void Awake()
    {
        _pool = new ObjectPool<T>(CreatePrefab, TurnOff, TurnOn, _initialStock);
        Debug.Log("Awake de padre");
    }

    protected abstract void TurnOn(T obj);
    protected abstract void TurnOff(T obj);
    protected abstract T CreatePrefab();
    public abstract T Create();
}