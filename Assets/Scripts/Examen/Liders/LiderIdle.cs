public class LiderIdle : State<NPCState>
{
    Lider _l;

    public LiderIdle(FSM<NPCState> fsm, Lider lider) : base(fsm)
    {
        _l = lider;
    }

    public override void Execute()
    {
        // Приоритет: низкое HP важнее всего
        if (_l.IsLowHP)
        {
            _fsm.ChangeState(NPCState.Salvation);
            return;
        }

        // Приоритет: клик мыши важнее преследования врага
        if (_l.Controller != null && _l.Controller.HasClick)
        {
            _l.SetPathToClick(_l.ClickPosition);
            _fsm.ChangeState(NPCState.FollowToClick);
            return;
        }

        if (_l.FindEnemyInFOV())
        {
            _fsm.ChangeState(NPCState.Persuit);
            return;
        }
    }
}
