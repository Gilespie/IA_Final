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
        /*if (_lider.FOVV.InFOV(_lider.EnemyTarget.position))
        {
            _fsm.ChangeState(NPCState.Persuit);
            return;
        }*/

        if (_lider.Controller.HasClick)
        {
            _lider.SetPathToClick(_lider.ClickPosition);
            _fsm.ChangeState(NPCState.FollowToClick);
        }
    }
}