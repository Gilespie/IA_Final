using UnityEngine;

public class SolderEscape : State<NPCState>
{
    Solder _solder;
    Vector3 _safePosition;
    float _escapeTimer = 0f;
    float _escapeDuration = 5f;

    public SolderEscape(FSM<NPCState> fsm, Solder solder) : base(fsm)
    {
        _solder = solder;
    }

    public override void Enter()
    {
        _escapeTimer = 0f;
        FindSafePosition();
    }

    public override void Execute()
    {
        // Восстанавливаем HP
        _solder.RecoverHealth(Time.deltaTime);

        // Если HP восстановилось, возвращаемся к нормальному поведению
        if (!_solder.IsLowHP)
        {
            Debug.Log($"{_solder.gameObject.name} HP восстановилось, возвращается к лидеру");
            _fsm.ChangeState(NPCState.FollowToClick);
            return;
        }

        // Если видим врага, убегаем от него
        if (_solder.CheckEnemyInFOV() && _solder.Target != null)
        {
            Vector3 dir = (_solder.transform.position - _solder.Target.position).normalized;
            Vector3 fleeTarget = _solder.transform.position + dir * 15f;
            fleeTarget.y = _solder.transform.position.y;

            Debug.Log($"{_solder.gameObject.name} убегает от врага {_solder.Target.name}");
            _solder.MoveWithTheta(fleeTarget);
            return;
        }

        // Движемся к безопасной позиции
        if (Vector3.Distance(_solder.transform.position, _safePosition) > 2f)
        {
            _solder.MoveWithTheta(_safePosition);
        }
        else
        {
            FindSafePosition();
        }

        _escapeTimer += Time.deltaTime;

        if (_escapeTimer > _escapeDuration)
        {
            if (!_solder.IsLowHP)
            {
                _fsm.ChangeState(NPCState.FollowToClick);
            }
        }
    }

    void FindSafePosition()
    {
        if (_solder.Lider != null)
        {
            Vector3 leaderPos = _solder.Lider.transform.position;
            Vector3 dir = (leaderPos - _solder.transform.position).normalized;

            _safePosition = _solder.transform.position + dir * 5f;
        }
        else
        {
            _safePosition = _solder.transform.position + Random.insideUnitSphere * 10f;
            _safePosition.y = _solder.transform.position.y;
        }
    }
}
