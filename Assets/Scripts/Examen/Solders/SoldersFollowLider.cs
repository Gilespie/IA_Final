using UnityEngine;

public class SoldersFollowLider : State<NPCState>
{
    Solder _solder;

    public SoldersFollowLider(FSM<NPCState> fsm, Solder solder) : base(fsm)
    {
        _solder = solder;
    }

    public override void Execute()
    {
        if (_solder.IsLowHealth)
        {
            _fsm.ChangeState(NPCState.Salvation);
            return;
        }

        if (_solder.CheckEnemyInFOV())
        {
            _fsm.ChangeState(NPCState.Persuit);
            return;
        }

        if (_solder.Lider != null)
        {
            _solder.FollowLeader();
        }
    }
}
