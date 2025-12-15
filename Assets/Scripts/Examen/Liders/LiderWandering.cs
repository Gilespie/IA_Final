using System.Collections.Generic;
using UnityEngine;

public class LiderWandering : State<NPCState>
{
    Lider _lider;
    List<Graph> _path;
    int _index;
    float _stopDistance = 0.5f;

    public LiderWandering(FSM<NPCState> fsm, Lider lider) : base(fsm)
    {
        _lider = lider;
    }

    public override void Enter()
    {
        _path = PathManagerExamen.Instance.GetPath(_lider.transform.position, GetRandomNode());
        _index = 0;

        if (_path == null || _path.Count == 0)
        {
            _fsm.ChangeState(NPCState.Wandering);
        }
    }

    public override void Execute()
    {
        _lider.FindTargetInFOV();

        if (_lider.EnemyInFOV())
        {
            _fsm.ChangeState(NPCState.Persuit);
            return;
        }

        if (_path == null || _path.Count == 0)
        {
            Enter();
            return;
        }
            

        if (_index >= _path.Count)
        {
            Enter();
            return;
        }

        Vector3 target = _path[_index].transform.position;
        float dist = (target - _lider.transform.position).sqrMagnitude;

        if (dist <= _stopDistance * _stopDistance)
        {
            _index++;
            return;
        }

        _lider.Wandering(target);
    }

    Vector3 GetRandomNode()
    {
        var nodes = PathManagerExamen.Instance.AllNodes;

        if (nodes == null || nodes.Length == 0)
            return _lider.transform.position;

        int index = Random.Range(0, nodes.Length);
        return nodes[index].transform.position;
    }
}