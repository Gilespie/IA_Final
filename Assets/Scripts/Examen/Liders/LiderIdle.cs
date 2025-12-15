public class LiderIdle : State<NPCState>
{
    Lider _lider;

    public LiderIdle(FSM<NPCState> fsm, Lider lider) : base(fsm)
    {
        _lider = lider;
    }

    public override void Execute()
    {
        _lider.FindTargetInFOV();

        if (_lider.EnemyInFOV())
        {
            _fsm.ChangeState(NPCState.Persuit);
            return;
        }

        if (_lider.IsControlable)
        {
            if (_lider.Controller.HasClick)
            {
                _fsm.ChangeState(NPCState.FollowToClick);
            }
        }
    }
}