using UnityEngine;

public class LiderPersuit : State<NPCState>
{
    Lider _lider;

    public LiderPersuit(FSM<NPCState> fsm, Lider lider) : base(fsm)
    {
        _lider = lider;
    }

    public override void Enter()
    {
        if (_lider.EnemyTarget == null)
        {
            _lider.FindEnemyInFOV();
        }
    }

    public override void Execute()
    {
        // ПРИОРИТЕТ #1: Низкое HP важнее всего - убегаем даже во время преследования
        if (_lider.IsLowHP)
        {
            Debug.Log($"{_lider.gameObject.name} HP низкое ({_lider.CurrentHealth:F1}) во время преследования, убегает!");
            _lider.EnemyTarget = null;
            _fsm.ChangeState(NPCState.Salvation);
            return;
        }

        // Приоритет: клик мыши прерывает преследование (только для игрового лидера)
        if (_lider.Controller != null && _lider.Controller.HasClick)
        {
            _lider.SetPathToClick(_lider.ClickPosition);
            _fsm.ChangeState(NPCState.FollowToClick);
            return;
        }

        // Проверяем что враг все еще в FOV (обязательно для продолжения преследования)
        if (!_lider.IsEnemyInFOV())
        {
            // Враг вышел из FOV или радиуса - теряем цель и возвращаемся к патрулированию
            _lider.EnemyTarget = null;
            if (_lider.Controller == null)
                _fsm.ChangeState(NPCState.Patrol);
            else
                _fsm.ChangeState(NPCState.Idle);
            return;
        }

        // Проверяем что цель все еще существует
        if (_lider.EnemyTarget == null || !_lider.EnemyTarget.gameObject.activeInHierarchy)
        {
            if (_lider.Controller == null)
                _fsm.ChangeState(NPCState.Patrol);
            else
                _fsm.ChangeState(NPCState.Idle);
            return;
        }

        // Атакуем врага (только если HP нормальное)
        _lider.PersuitTarget();
    }
}
