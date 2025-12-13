public class SoldersFollowLider : State<NPCState>
{
    Solder _solder;

    public SoldersFollowLider(FSM<NPCState> fsm, Solder solder, Lider lider) : base(fsm)
    {
        _solder = solder;
    }

    public override void Execute()
    {
        _solder.CheckEnemyInFOV();

        if (_solder.EnemyInFOV())
        {
            _fsm.ChangeState(NPCState.Persuit);
            return;
        }

        if (_solder.LeaderInSight())
        {
            
            _solder.FollowLeader();
        }
        else
        {
            _fsm.ChangeState(NPCState.FollowToLiderByPath);
        }
    }
}