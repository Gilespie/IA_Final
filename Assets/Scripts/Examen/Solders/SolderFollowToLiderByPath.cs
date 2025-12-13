using System.Collections.Generic;
using UnityEngine;

public class SolderFollowToLiderByPath : State<NPCState>
{
    Solder _solder;
    List<Graph> _path = new List<Graph>();
    int _pathIndex = 0;
    float _stopDistance = 1f;

    public SolderFollowToLiderByPath(FSM<NPCState> fsm, Solder solder) : base(fsm)
    {
        _solder = solder;
    }

    public override void Enter()
    {
        if (_solder.Lider == null)
        {
            _fsm.ChangeState(NPCState.Escape);
            return;
        }

        _pathIndex = 0;

        var start = PathManagerExamen.Instance.Closest(_solder.transform.position);
        var end = PathManagerExamen.Instance.Closest(_solder.Lider.transform.position);

        if (start == null || end == null)
        {
            _fsm.ChangeState(NPCState.FollowToLider);
            return;
        }

        _path = PathManagerExamen.Instance.GetPath(
            start.transform.position,
            end.transform.position
        );

        if (_path == null || _path.Count == 0)
        {
            _fsm.ChangeState(NPCState.FollowToLider);
            return;
        }

        if (_path.Count > 1)
            _path.RemoveAt(0);
    }

    public override void Execute()
    {
        if (_path == null || _path.Count == 0)
        {
            _fsm.ChangeState(NPCState.FollowToLider);
            return;
        }

        if (_pathIndex >= _path.Count)
        {
            _fsm.ChangeState(NPCState.FollowToLider);
            return;
        }

        if (_solder.LeaderInSight())
        {
            _fsm.ChangeState(NPCState.FollowToLider);
            return;
        }

        Vector3 targetPos = _path[_pathIndex].transform.position;
        Vector3 dir = targetPos - _solder.transform.position;

        if (dir.sqrMagnitude <= _stopDistance * _stopDistance)
        {
            _pathIndex++;
            return;
        }

        _solder.AddForce(_solder.Seek(targetPos));
        _solder.Move();
    }
}