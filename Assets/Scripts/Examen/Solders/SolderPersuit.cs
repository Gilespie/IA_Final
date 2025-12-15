using UnityEngine;

public class SolderPersuit : State<NPCState>
{
    Solder _solder;
    HealthSystem _healthSystem;
    float _damage = 10f;
    float _attackRadius = 1f;
    float _attackTimer = 1f;

    public SolderPersuit(FSM<NPCState> fsm, Solder solder, HealthSystem healthSystem) : base(fsm)
    {
        _solder = solder;
        _healthSystem = healthSystem;
    }

    public override void Execute()
    {
        if (!_solder.EnemyInFOV())
        {
            _fsm.ChangeState(NPCState.FollowToLider);
            return;
        }

        if (_healthSystem.IsLowHealth())
        {
            _fsm.ChangeState(NPCState.Escape);
            return;
        }

        float distance = (_solder.EnemyTarget.position - _solder.transform.position).sqrMagnitude;

        if (distance >= _attackRadius * _attackRadius)
        {
            _solder.PersuitTarget();
        }
        else
        { 
            _attackTimer -= Time.deltaTime;

            if (_attackTimer <= 0)
            {
                if(_solder.EnemyTarget.TryGetComponent<IDamageable>(out var dmgable))
          
                dmgable.TakeDamage(_damage);

                if (dmgable.CurrentHealth <= 0f)
                {
                    _solder.ClearEnemyTarget(); 
                    _fsm.ChangeState(NPCState.Idle);
                    return;
                }

                _attackTimer = 1f;
            }
        }
    }
}