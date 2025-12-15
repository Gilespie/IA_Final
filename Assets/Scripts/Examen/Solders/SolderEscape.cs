using System.Collections.Generic;
using UnityEngine;

public class SolderEscape : State<NPCState>
{
    Solder _solder;
    List<Graph> _path = new List<Graph>();
    int _pathIndex = 0;
    float _stopDistance = 1f;
    float _speed = 5f;
    Transform _savePlace;

    public SolderEscape(FSM<NPCState> fsm, Solder solder, Transform savePlace) : base(fsm)
    {
        _solder = solder;
        _savePlace = savePlace;
    }

    public override void Enter()
    {
        _pathIndex = 0;

        _path = PathManagerExamen.Instance.GetPath(
            _solder.transform.position,
            _savePlace.position
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

        Vector3 dir = _path[_pathIndex].transform.position - _solder.transform.position;

        if (dir.sqrMagnitude <= _stopDistance * _stopDistance)
        {
            _pathIndex++;
        }

        _solder.transform.position += dir.normalized * _speed * Time.deltaTime;
        _solder.transform.forward = dir;
    }
}