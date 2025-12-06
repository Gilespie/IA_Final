using UnityEngine;

public class SolderPersuit : State<NPCState>
{
    Solder _solder;

    public SolderPersuit(FSM<NPCState> fsm, Solder solder) : base(fsm)
    {
        _solder = solder;
    }

    public override void Execute()
    {
        if (_solder.IsLowHealth)
        {
            _solder.ClearEnemyTarget();
            _fsm.ChangeState(NPCState.Salvation);
            return;
        }

        if (!_solder.CheckEnemyInFOV() || _solder.CurrentEnemyTarget == null)
        {
            _solder.ClearEnemyTarget();
            _fsm.ChangeState(NPCState.FollowToClick);
            return;
        }

        _solder.Attack();

        Vector3 avoidance = _solder.Avoid.ChangeVelocity(_solder.Velocity);
        if (avoidance != _solder.Velocity)
        {
            _solder.AddForce((avoidance - _solder.Velocity) * _solder.AvoidanceWeight);
        }
    }
}
