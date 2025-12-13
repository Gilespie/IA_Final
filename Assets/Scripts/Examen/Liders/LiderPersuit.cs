using UnityEngine;

public class LiderPersuit : State<NPCState>
{
    Lider _lider;
    float _damage = 20f;
    float _attackRadius = 1.5f;
    float _attackTimer = 1f;

    public LiderPersuit(FSM<NPCState> fsm, Lider lider) : base(fsm)
    {
        _lider = lider;
    }

    public override void Execute()
    {
        if (!_lider.EnemyInFOV())
        {
            _fsm.ChangeState(NPCState.Idle);
            return;
        }

        float distance = (_lider.EnemyTarget.position - _lider.transform.position).sqrMagnitude;

        if (distance >= _attackRadius * _attackRadius)
        {
            Debug.Log("Persuit");
            _lider.PersuitTarget();
        }
        else
        {
            _attackTimer -= Time.deltaTime;

            if (_attackTimer <= 0)
            {
                if(_lider.EnemyTarget.TryGetComponent<IDamageable>(out IDamageable dmgable))

                dmgable.TakeDamage(_damage);

                _attackTimer = 1f;

                if (dmgable.CurrentHealth <= 0f)
                {
                    _fsm.ChangeState(NPCState.Idle);
                }
            }
        }
    }
}