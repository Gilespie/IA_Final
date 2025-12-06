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
        if (_lider.FindEnemyInFOV())
        {
            _fsm.ChangeState(NPCState.Persuit);
            return;
        }

        if (!_lider.HasPath)
        {
            _fsm.ChangeState(NPCState.Idle);
            return;
        }

        _lider.MoveByPath(_lider.MovSpeed);
    }
}
