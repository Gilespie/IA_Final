using UnityEngine;

public class Patrol : State<NPCState>
{
    Hunter _hunter;
    float _speed;
    Transform _target;
    float _detectRadius = 5f;
    Collider[] _targets = new Collider[1];
    LayerMask _mask;

    public Patrol(FSM<NPCState> fsm, Hunter hunter, float speed, float detectRadius, LayerMask mask) : base(fsm)
    {
        _hunter = hunter;
        _speed = speed;
        _detectRadius = detectRadius;
        _mask = mask;
    }

    public override void Execute()
    {
        int count = Physics.OverlapSphereNonAlloc(_hunter.transform.position, _detectRadius, _targets, _mask);

        if (count > 0)
        {
            _target = _targets[0].transform;
            _hunter.SetTarget(_target);

            Debug.Log("Target Finded");
            if (_hunter.Target != null)
            {
                _fsm.ChangeState(NPCState.Hunt);
            }
        }

        Vector3 dir = _hunter.CurrentWaypoint.position - _hunter.transform.position;
        _hunter.transform.position += dir.normalized * Time.deltaTime * _speed;

        if (dir.sqrMagnitude < 0.2f * 0.2f)
        {
            _hunter.OnWaypointArrive();
            _fsm.ChangeState(NPCState.Idle);
            Debug.Log("Arrived");
        }

        if (_hunter.Energy.ConsumeEnergy(Time.deltaTime))
        {
            _fsm.ChangeState(NPCState.Idle);
            Debug.Log("Hunter is tired");
        }
    }
}