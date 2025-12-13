using UnityEngine;

public class Hunter : MonoBehaviour
{
    FSM<NPCState> _fsm;

    [SerializeField] Transform[] _waypoints;
    [SerializeField] float _idleTime = 0f;
    public float IdleTime => _idleTime;

    [SerializeField] float _patrolSpeed = 10f;
    [SerializeField] float _huntSpeed = 15f;
    [SerializeField] float _damage = 50f;
    [SerializeField] HunterEnergy _energy;
    public HunterEnergy Energy => _energy;
    [SerializeField] Transform _target;
    public Transform Target => _target;
    [SerializeField] float _attackRadius = 1.5f;
    [SerializeField] float _detectRadius = 5f;

    [SerializeField] float _slowingRange = 3f;
    public float SlowingRange => _slowingRange;

    [SerializeField] LayerMask _enemyMask;

    [SerializeField] LayerMask _obstacleMask;
    public LayerMask ObstacleMask => _obstacleMask;

    private int _currentWaypoint;
    public Transform CurrentWaypoint => _waypoints[_currentWaypoint];

    Vector3 _velocity;
    public Vector3 Velocity => _velocity;


    private void Start()
    {
        SetFSM();
    }

    private void SetFSM()
    {
        _fsm = new FSM<NPCState>();

        var idle = new Idle(_fsm, this);
        var patrol = new Patrol(_fsm, this, _patrolSpeed, _detectRadius, _enemyMask);
        var hunt = new Hunt(_fsm, this, _attackRadius, _detectRadius, _huntSpeed, _damage);

        idle.AddTransition(NPCState.Patrol, patrol);
        idle.AddTransition(NPCState.Hunt, hunt);

        patrol.AddTransition(NPCState.Idle, idle);
        patrol.AddTransition(NPCState.Hunt, hunt);

        hunt.AddTransition(NPCState.Idle, idle);
        hunt.AddTransition(NPCState.Patrol, patrol);

        _fsm.SetInnitialFSM(idle);
    }

    private void Update()
    {
        _fsm.OnUpdate();
    }

    public void OnWaypointArrive()
    {
        _currentWaypoint = _currentWaypoint >= _waypoints.Length - 1 ? 0 : _currentWaypoint + 1;
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }

    public Vector3 SetVelocity()
    {
        return _velocity;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _detectRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _attackRadius);
    }
}