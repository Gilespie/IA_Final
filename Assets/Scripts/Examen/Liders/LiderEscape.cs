using UnityEngine;

public class LiderEscape : State<NPCState>
{
    Lider _lider;
    Vector3 _safePosition;
    float _escapeTimer = 0f;
    float _escapeDuration = 5f;

    public LiderEscape(FSM<NPCState> fsm, Lider lider) : base(fsm)
    {
        _lider = lider;
    }

    public override void Enter()
    {
        _escapeTimer = 0f;
        FindSafePosition();
    }

    public override void Execute()
    {
        // Восстанавливаем HP
        _lider.RecoverHealth(Time.deltaTime);

        // Если HP восстановилось, возвращаемся к нормальному поведению
        if (_lider.CurrentHealth >= 30f)
        {
            if (_lider.Controller != null)
                _fsm.ChangeState(NPCState.Idle);
            else
                _fsm.ChangeState(NPCState.Patrol);
            return;
        }

        // Если видим врага, убегаем от него
        if (_lider.FindEnemyInFOV() && _lider.EnemyTarget != null)
        {
            Vector3 dir = (_lider.transform.position - _lider.EnemyTarget.position).normalized;
            Vector3 fleeTarget = _lider.transform.position + dir * 10f;
            _lider.SetPathToClick(fleeTarget);
            _lider.MoveByPath(_lider.MovSpeed);
            return;
        }

        // Движемся к безопасной позиции
        if (Vector3.Distance(_lider.transform.position, _safePosition) > 2f)
        {
            _lider.SetPathToClick(_safePosition);
            _lider.MoveByPath(_lider.MovSpeed);
        }
        else
        {
            FindSafePosition();
        }

        _escapeTimer += Time.deltaTime;

        if (_escapeTimer > _escapeDuration && _lider.CurrentHealth >= 30f)
        {
            if (_lider.Controller != null)
                _fsm.ChangeState(NPCState.Idle);
            else
                _fsm.ChangeState(NPCState.Patrol);
        }
    }

    void FindSafePosition()
    {
        // Ищем безопасную позицию подальше от врагов
        Vector3 currentPos = _lider.transform.position;
        _safePosition = currentPos + Random.insideUnitSphere * 15f;
        _safePosition.y = currentPos.y;
    }
}

