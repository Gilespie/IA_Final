using System;
using System.Collections.Generic;

public class ObjectPool<T>
{
    public delegate T FactoryMethod();

    FactoryMethod _factory;
    Action<T> TurnOff;
    Action<T> TurnOn;
    List<T> _availablePrefabs = new List<T>();

    public ObjectPool(FactoryMethod factory, Action<T> turnOff, Action<T> turnOn, int initialStock = 5)
    {
        _factory = factory;
        TurnOff = turnOff;
        TurnOn = turnOn;

        for (int i = 0; i < initialStock; i++)
        {
            var x = _factory();

            TurnOff(x);
            _availablePrefabs.Add(x);
        }
    }

    public T GetFromPool()
    {
        if (_availablePrefabs.Count > 0)
        {
            var x = _availablePrefabs[0];
            _availablePrefabs.RemoveAt(0);

            TurnOn(x);
            return x;
        }

        return _factory();
    }

    public void ReturnToPool(T obj)
    {
        TurnOff(obj);
        _availablePrefabs.Add(obj);
    }
}
