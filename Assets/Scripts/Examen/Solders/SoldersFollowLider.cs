public class SoldersFollowLider : State<NPCState>
{
    Solder _solder;
    HealthSystem _healthSystem;

    public SoldersFollowLider(FSM<NPCState> fsm, Solder solder, HealthSystem healthSystem) : base(fsm)
    {
        _solder = solder;
        _healthSystem = healthSystem;
    }

    public override void Execute()
    {
        _solder.CheckEnemyInFOV();

        if (_solder.EnemyInFOV())
        {
            _fsm.ChangeState(NPCState.Persuit);
            return;
        }

        if (_healthSystem.IsLowHealth())
        {
            _fsm.ChangeState(NPCState.Escape);
            return;
        }

        if (_solder.LeaderInSight())
        {
            
            _solder.FollowLeader();
        }
        else
        {
            _fsm.ChangeState(NPCState.FollowToLiderByPath);
            return;
        }
    }
}