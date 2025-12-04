using System.Collections.Generic;
using UnityEngine;

public class FlockingManager : MonoBehaviour
{
    public static FlockingManager Instance;

    public List<Boid> _allBoids;
    public List<Boid> AllBoids => _allBoids;

    private void Awake()
    {
        if(!Instance)
        {
            Instance = this;
            
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        _allBoids = new List<Boid>();
    }

    public void AddBoid(Boid boid) => _allBoids.Add(boid);
    public void RemoveBoid(Boid boid) => _allBoids.Remove(boid);
}