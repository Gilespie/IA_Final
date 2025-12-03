using UnityEngine;

public class GuardianIdle : State<NPCState>
{
    Guardian _guardian;
    float _timer;

    public GuardianIdle(FSM<NPCState> fsm, Guardian guardian) : base(fsm)
    {
        _guardian = guardian;
    }

    public override void Enter()
    {
        Debug.Log("Idle enter");
        base.Enter();
        _timer = Time.time + _guardian.IdleTime;
    }

    public override void Execute()
    {
        base.Execute();

        if (Time.time >= _timer)
        {
            _fsm.ChangeState(NPCState.Patrol);
        }
        if(_guardian.FOVV.InFOV(_guardian.Target.position))
        {
            _fsm.ChangeState(NPCState.Persuit);
        }
    }
}