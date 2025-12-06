using UnityEngine;

public class LiderPersuit : State<NPCState>
{
    Lider _lider;

    public LiderPersuit(FSM<NPCState> fsm, Lider lider) : base(fsm)
    {
        _lider = lider;
    }

    public override void Execute()
    {
        if (_lider.EnemyTarget == null || !_lider.FOVV.InFOV(_lider.EnemyTarget.position))
        {
            _lider.ClearEnemyTarget();
            _lider.StopCompletely();
            _fsm.ChangeState(NPCState.Idle);
            return;
        }

        _lider.PersuitTarget();
    }
}
