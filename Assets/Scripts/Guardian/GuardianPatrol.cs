using System.Collections.Generic;
using UnityEngine;

public class GuardianPatrol : State<NPCState>
{
    Guardian _guardian;
    List<Graph> _patrolPath;
    
    public GuardianPatrol(FSM<NPCState> fsm, List<Graph> path, Guardian guardian) : base(fsm)
    {
        _patrolPath = path;
        _guardian = guardian;
    }

    public override void Enter()
    {
        Debug.Log("Patrol enter");

        var start = _guardian.CurrentNode;
        var end = _patrolPath[_guardian.PatrolIndex];

        var path = PathManager.Instance.GetPath(start.transform.position, end.transform.position);

        _guardian.SetPath(path);
    }

    public override void Execute()
    {
        if (_guardian.FOVV.InFOV(_guardian.Target.position))
        {
            _fsm.ChangeState(NPCState.Persuit);
        }

        bool moving = _guardian.MoveAlongPath(_guardian.PatrolSpeed);

        if (_guardian.PatrolIndex >= _patrolPath.Count)
        {
            Debug.Log("Finish");
            _fsm.ChangeState(NPCState.Idle);
        }

        if (!moving)
        {
            _guardian.ChangePatrolIndex((_guardian.PatrolIndex + 1) % _patrolPath.Count);
            _fsm.ChangeState(NPCState.Idle);
        }
    }
}