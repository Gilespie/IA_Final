using UnityEngine;

public class SoldersFollowLeader : State<NPCState>
{
    Solder _s;

    public SoldersFollowLeader(FSM<NPCState> fsm, Solder s) : base(fsm)
    {
        _s = s;
    }

    public override void Execute()
    {
        // Приоритет: низкое HP важнее всего (каждый солдат проверяет СВОЕ HP независимо)
        if (_s.IsLowHP)
        {
            Debug.Log($"{_s.gameObject.name} HP низкое ({_s.CurrentHP:F1}/{_s.MaxHealth}), убегает! (другие солдаты продолжают бой)");
            _fsm.ChangeState(NPCState.Salvation);
            return;
        }

        if (_s.CheckEnemyInFOV())
        {
            Debug.Log($"{_s.gameObject.name} видит врага, переходит в атаку");
            _fsm.ChangeState(NPCState.Persuit);
            return;
        }

        _s.FollowLeader();
    }
}
