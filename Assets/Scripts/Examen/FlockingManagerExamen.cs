using System.Collections.Generic;
using UnityEngine;

public class FlockingManagerExamen : MonoBehaviour
{
    public static FlockingManagerExamen Instance;
    public Lider _lider;
    public Lider Lider => _lider;
    public List<Solder> _allSolders;
    public List<Solder> AllSolders => _allSolders;

    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        }
    }

    public void AddSolder(Solder solder) => _allSolders.Add(solder);
    public void RemoveSolder(Solder solder) => _allSolders.Remove(solder);
}