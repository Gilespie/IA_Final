using UnityEngine;

public class Idle : State<NPCState>
{
    Hunter _hunter;
    float _timer;

    public Idle(FSM<NPCState> fsm, Hunter hunter) : base(fsm)
    {
        _hunter = hunter;
    }

    public override void Enter()
    {
        base.Enter();
        _timer = Time.time + _hunter.IdleTime;
    }

    public override void Execute()
    {
        base.Execute();
        
        if (_hunter.Energy.AddEnergy(Time.deltaTime))
        {
            _fsm.ChangeState(NPCState.Patrol);
        }

        if (Time.time >= _timer)
        {
            _fsm.ChangeState(NPCState.Patrol);
        }
    }
}