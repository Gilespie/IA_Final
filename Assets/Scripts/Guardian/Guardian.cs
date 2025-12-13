/*using System.Collections.Generic;
using UnityEngine;

public class Guardian : MonoBehaviour
{
    FSM<NPCState> _fsm;
    public FSM<NPCState> FSM => _fsm;

    [SerializeField] FOV _fov;
    public FOV FOVV => _fov;
    [SerializeField] float _idleTime = 0f;
    public float IdleTime => _idleTime;

    [SerializeField] float _patrolSpeed = 10f;
    public float PatrolSpeed => _patrolSpeed;


    [SerializeField] float _persuitSpeed = 15f;
    public float PersuitSpeed => _persuitSpeed;

    [SerializeField] float _damage = 50f;
    public float Damage => _damage;

    [SerializeField] Transform _target;
    public Transform Target => _target;

    [SerializeField] float _attackRadius = 1.5f;
    public float AttackRadius => _attackRadius;

    [SerializeField] LayerMask _obstacleMask;
    public LayerMask ObstacleMask => _obstacleMask;

    [SerializeField] float _detectRadius = 5f;
    [SerializeField] LayerMask _enemyMask;
    [SerializeField] Transform[] _waypoints;
    private int _currentWaypoint;
    public Transform CurrentWaypoint => _waypoints[_currentWaypoint];

    Vector3 _velocity;
    public Vector3 Velocity => _velocity;

    List<Graph> _path = new List<Graph>();
    public List<Graph> Path => _path;

    Vector3 _lastTargetPoint;
    public Vector3 LastTargetPoint => _lastTargetPoint;

    int _pathIndex;

    private void Start()
    {
        SetFSM();
    }

    private void SetFSM()
    {
        _fsm = new FSM<NPCState>();

        var idle = new GuardianIdle(_fsm, this);
        var patrol = new GuardianPatrol(_fsm, _path, this);
        var persuit = new GuardianPersuit(_fsm, this);
        var goToLastTargetPoint = new GuardianLastPoint(_fsm, this);

        idle.AddTransition(NPCState.Patrol, patrol);
        idle.AddTransition(NPCState.Hunt, persuit);

        patrol.AddTransition(NPCState.Idle, idle);
        patrol.AddTransition(NPCState.Hunt, persuit);

        persuit.AddTransition(NPCState.Idle, idle);
        persuit.AddTransition(NPCState.Patrol, patrol);

        _fsm.SetInnitialFSM(idle);
    }

    private void Update()
    {
        _fsm.OnUpdate();
    }

    private void FixedUpdate()
    {
        _fsm.OnFixedUpdate();
    }

    public void OnWaypointArrive()
    {
        _currentWaypoint = _currentWaypoint >= _waypoints.Length - 1 ? 0 : _currentWaypoint + 1;
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }

    public void SetPath(List<Graph> path)
    {
        _path = path;
        _pathIndex = 0;
    }

    public bool MoveByPath()
    {
        if (_path == null || _path.Count == 0)
            return true;

        var next = _path[_pathIndex].transform.position;
        Vector3 dir = next - transform.position;

        transform.position += dir.normalized * Time.deltaTime * _patrolSpeed;

        if (dir.sqrMagnitude < 0.3f * 0.3f)
        {
            _pathIndex++;

            if (_pathIndex >= _path.Count)
                return true;
        }

        return false;
    }

    public Vector3 SetVelocity()
    {
        return _velocity;
    }

    public void SetLastTargetPoint(Vector3 lastPoint)
    {
        _lastTargetPoint = lastPoint;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _detectRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _attackRadius);
    }
}*/

using System.Collections.Generic;
using UnityEngine;

public class Guardian : MonoBehaviour
{
    FSM<NPCState> _fsm;
    public FSM<NPCState> FSM => _fsm;

    [SerializeField] FOV _fov;
    public FOV FOVV => _fov;
    [SerializeField] float _distanceOffset = 0.3f;

    [SerializeField] float _idleTime = 0f;
    public float IdleTime => _idleTime;

    [SerializeField] float _patrolSpeed = 10f;
    public float PatrolSpeed => _patrolSpeed;


    [SerializeField] float _persuitSpeed = 15f;
    public float PersuitSpeed => _persuitSpeed;

    [SerializeField] float _damage = 50f;
    public float Damage => _damage;

    [SerializeField] Transform _target;
    public Transform Target => _target;

    [SerializeField] float _attackRadius = 1.5f;
    public float AttackRadius => _attackRadius;

    [SerializeField] LayerMask _obstacleMask;
    public LayerMask ObstacleMask => _obstacleMask;

    List<Graph> _path = new List<Graph>();
    public List<Graph> Path => _path;

    Vector3 _lastTargetPoint;
    public Vector3 LastTargetPoint => _lastTargetPoint;
    public Graph CurrentNode => PathManager.Instance.Closest(transform.position);

    [SerializeField] List<Graph> _patrolPath;
    public List<Graph> PatrolPath => _patrolPath;
    int _patrolIndex;
    public int PatrolIndex => _patrolIndex;
    int _pathIndex;
    MeshRenderer _meshRenderer;
    [SerializeField] Color _color;

    void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _meshRenderer.material.color = _color;
    }

    void Start()
    {
        SetFSM();
        GuardianManager.Instance.AllGuardians.Add(this);
    }

    void SetFSM()
    {
        _fsm = new FSM<NPCState>();

        var idle = new GuardianIdle(_fsm, this);
        var patrol = new GuardianPatrol(_fsm, _patrolPath, this);
        var persuit = new GuardianPersuit(_fsm, this);
        var goToLastTargetPoint = new GuardianLastPoint(_fsm, this);

        idle.AddTransition(NPCState.Patrol, patrol);
        idle.AddTransition(NPCState.Persuit, persuit);
        idle.AddTransition(NPCState.GoLastTargetPoint, goToLastTargetPoint);

        patrol.AddTransition(NPCState.Idle, idle);
        patrol.AddTransition(NPCState.Persuit, persuit);
        patrol.AddTransition(NPCState.GoLastTargetPoint, goToLastTargetPoint);

        persuit.AddTransition(NPCState.Idle, idle);
        persuit.AddTransition(NPCState.Persuit, patrol);
        persuit.AddTransition(NPCState.GoLastTargetPoint, goToLastTargetPoint);

        goToLastTargetPoint.AddTransition(NPCState.Idle, idle);
        goToLastTargetPoint.AddTransition(NPCState.Persuit, persuit);

        _fsm.SetInnitialFSM(idle);
    }

    void Update()
    {
        _fsm.OnUpdate();
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }

    public void ChangePatrolIndex(int value)
    {
        _patrolIndex = value;
    }

    public void SetPath(List<Graph> path)
    {
        _path = path;
        _pathIndex = 0;
    }

    public void SetLastTargetPoint(Vector3 lastPoint)
    {
        _lastTargetPoint = lastPoint;
    }

    public bool MoveAlongPath(float speed)
    {
        if (_path == null || _path.Count == 0)
        {
            Debug.Log("Empty");
            return false;
        }

        if (_pathIndex == 0)
        {
            Debug.Log("PATH LENGTH = " + _path.Count);
        }

        if (_pathIndex >= _path.Count) return false;
        
        Vector3 target = _path[_pathIndex].transform.position;
        Vector3 dir = target - transform.position;

        if (dir.sqrMagnitude < _distanceOffset * _distanceOffset)
        {
            _pathIndex++;
            Debug.Log(_patrolIndex);
            return _pathIndex < _path.Count;
        }

        transform.position += dir.normalized * speed * Time.deltaTime;
        transform.forward = dir;
        return true;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _attackRadius);
    }
}