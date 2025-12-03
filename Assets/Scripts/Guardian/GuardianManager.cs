using System.Collections.Generic;
using UnityEngine;

public class GuardianManager : MonoBehaviour
{
    public static GuardianManager Instance;

    [SerializeField] List<Guardian> _allGuardians;
    public List<Guardian> AllGuardians => _allGuardians;

    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AlertAll(Vector3 lastKnownPosition, Guardian sender)
    {
        foreach (var g in _allGuardians)
        {
            if (g == sender) continue;

            g.SetLastTargetPoint(lastKnownPosition);
            g.FSM.ChangeState(NPCState.GoLastTargetPoint);
        }
    }
}