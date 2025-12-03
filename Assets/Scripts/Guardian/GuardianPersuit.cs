using UnityEngine;

public class GuardianPersuit : State<NPCState>
{
    Guardian _guardian;
    float _attackTimer = 1f;

    public GuardianPersuit(FSM<NPCState> fsm, Guardian guardian) : base(fsm)
    {
        _guardian = guardian;
    }

    public override void Enter()
    {
        Debug.Log("Persuit enter");
        BuildPath();
        GuardianManager.Instance.AlertAll(_guardian.Target.position, _guardian);
    }

    void BuildPath()
    {
        if (_guardian.Target == null) return;

        var start = _guardian.CurrentNode;
        var end = PathManager.Instance.Closest(_guardian.Target.position);

        var path = PathManager.Instance.GetPath(start.transform.position, end.transform.position);
        _guardian.SetPath(path);
    }

    public override void Execute()
    {
        if (!_guardian.FOVV.InFOV(_guardian.Target.position))
        {
            Vector3 lastSeen = _guardian.Target.position;
            _fsm.ChangeState(NPCState.GoLastTargetPoint);
        }

        var distance = (_guardian.Target.position - _guardian.transform.position);

        _guardian.transform.position += distance.normalized * _guardian.PersuitSpeed * Time.deltaTime;
        _guardian.transform.forward = distance;

        if (distance.sqrMagnitude < _guardian.AttackRadius * _guardian.AttackRadius)
        {
            _attackTimer -= Time.deltaTime;

            if (_attackTimer <= 0)
            {
                _guardian.Target.GetComponent<IDamageable>().TakeDamage(_guardian.Damage);

                _attackTimer = 1f;
            }
        }
    }
}