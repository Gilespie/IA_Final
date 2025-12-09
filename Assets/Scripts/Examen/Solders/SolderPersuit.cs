using UnityEngine;

public class SolderPursuit : State<NPCState>
{
    Solder _s;

    public SolderPursuit(FSM<NPCState> fsm, Solder s) : base(fsm)
    {
        _s = s;
    }

    public override void Enter()
    {
        if (_s.Target == null)
        {
            _s.CheckEnemyInFOV();
        }
    }

    public override void Execute()
    {
        // Приоритет: низкое HP важнее всего
        if (_s.IsLowHP)
        {
            Debug.Log($"{_s.gameObject.name} HP низкое ({_s.CurrentHP:F1}) во время преследования, убегает!");
            _s.ClearTarget();
            _fsm.ChangeState(NPCState.Salvation);
            return;
        }

        // Проверяем врага в FOV
        if (!_s.CheckEnemyInFOV())
        {
            _s.ClearTarget();
            _fsm.ChangeState(NPCState.FollowToClick);
            return;
        }

        // Проверяем что цель все еще существует
        if (_s.Target == null)
        {
            _fsm.ChangeState(NPCState.FollowToClick);
            return;
        }

        // Атакуем врага (тот кто атакует - наносит урон)
        _s.Attack();

        var avoid = _s.Avoid.ChangeVelocity(_s.Velocity);
        if (avoid != _s.Velocity)
            _s.AddForce((avoid - _s.Velocity) * _s.AvoidanceWeight);
    }
}
