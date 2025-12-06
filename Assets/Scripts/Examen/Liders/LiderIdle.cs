using UnityEngine;

public class LiderIdle : State<NPCState>
{
    Lider _lider;

    public LiderIdle(FSM<NPCState> fsm, Lider lider) : base(fsm)
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

        if (_lider.Controller != null && _lider.Controller.HasClick)
        {
            _lider.SetPathToClick(_lider.ClickPosition);
            _fsm.ChangeState(NPCState.FollowToClick);
        }
    }
}
