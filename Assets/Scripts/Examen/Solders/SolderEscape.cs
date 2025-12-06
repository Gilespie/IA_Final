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
        if (!_solder.IsLowHealth)
        {
            _fsm.ChangeState(NPCState.FollowToClick);
            return;
        }

        if (_solder.CheckEnemyInFOV() && _solder.CurrentEnemyTarget != null)
        {
            Vector3 fleeDirection = (_solder.transform.position - _solder.CurrentEnemyTarget.position).normalized;
            Vector3 fleeTarget = _solder.transform.position + fleeDirection * 10f;
            _solder.FollowWithTheta(fleeTarget);
            return;
        }

        if (Vector3.Distance(_solder.transform.position, _safePosition) > 2f)
        {
            _solder.FollowWithTheta(_safePosition);
        }
        else
        {
            FindSafePosition();
        }

        _escapeTimer += Time.deltaTime;
        if (_escapeTimer > _escapeDuration)
        {
            if (!_solder.IsLowHealth)
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
            Vector3 direction = (leaderPos - _solder.transform.position).normalized;
            _safePosition = _solder.transform.position + direction * 5f;
        }
        else
        {
            _safePosition = _solder.transform.position + Random.insideUnitSphere * 10f;
            _safePosition.y = _solder.transform.position.y;
        }
    }
}
