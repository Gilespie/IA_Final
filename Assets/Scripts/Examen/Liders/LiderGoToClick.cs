using System.Collections.Generic;
using UnityEngine;

public class LiderGoToClick : State<NPCState>
{
    Lider _lider;

    public LiderGoToClick(FSM<NPCState> fsm, Lider lider) : base(fsm)
    {
        _lider = lider;
    }

    public override void Execute()
    {
        /*if (_lider.FOVV.InFOV(_lider.EnemyTarget.position))
        {
            _fsm.ChangeState(NPCState.Persuit);
            return;
        }*/

        if (!_lider.HasPath)
        {
            _fsm.ChangeState(NPCState.Idle);
            return;
        }

        Vector3 target = _lider.CurrentPathPoint;
        float dist = Vector3.Distance(_lider.transform.position, target);

        if (dist < 0.2f)
        {
            _lider.NextPathPoint();
            return;
        }
    }
}