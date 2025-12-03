using UnityEngine;

public class Hunt : State<NPCState>
{
    Hunter _hunter;
    float _attackRadius;
    float _speed;
    float _damage;
    float _attackTimer = 1f;
    float _detectRadius;

    public Hunt(FSM<NPCState> fsm, Hunter hunter, float attackRadius, float detectRadius, float speed, float damage) : base(fsm)
    {
        _hunter = hunter;
        _attackRadius = attackRadius;
        _speed = speed;
        _damage = damage;
        _detectRadius = detectRadius;
    }

    public override void Execute()
    {
        float distance = (_hunter.Target.position - _hunter.transform.position).sqrMagnitude;

        Vector3 dir = _hunter.Target.position - _hunter.transform.position;
        _hunter.transform.position += dir.normalized * Time.deltaTime * _speed;

        if (distance < _detectRadius * _detectRadius)
        {
            if (distance < _attackRadius * _attackRadius)
            {
                _attackTimer -= Time.deltaTime;

                if (_attackTimer <= 0)
                {
                    _hunter.Target.GetComponent<Needs>().TakeDamage(_damage);

                    if (_hunter.Target.GetComponent<Needs>().CurrentHealth <= 0)
                    {
                        _hunter.SetTarget(null);
                        _fsm.ChangeState(NPCState.Patrol);
                    }

                    _attackTimer = 1f;
                }
            }
        }
        else
        {
            _fsm.ChangeState(NPCState.Patrol);
            _hunter.SetTarget(null);
            Debug.Log("1");
            //Debug.Log("Cazador perdio su objetivo!");
        }

        if (_hunter.Energy.ConsumeEnergy(Time.deltaTime))
        {
            _fsm.ChangeState(NPCState.Idle);
            _hunter.SetTarget(null);
            //Debug.Log("Cazador esta cansado!");
        }
    }
}