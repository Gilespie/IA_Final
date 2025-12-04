using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Lider : SteeringBase, IDamageable
{
    FSM<NPCState> _fsm;
    public FSM<NPCState> FSM => _fsm;

    [SerializeField] Transform _enemyTarget;
    public Transform EnemyTarget => _enemyTarget;

    public Vector3 ClickPosition { get; private set; }

    [SerializeField] FOV _fov;
    public FOV FOVV => _fov;

    MouseInputController _controller;
    public MouseInputController Controller => _controller;

    [Header("Health")]
    [SerializeField] float _maxHealth = 100f;
    float _currentHealth;
    public float CurrentHealth => _currentHealth;

    [Header("Movement")]
    [SerializeField] float _moveSpeed = 1f;
    public float MovSpeed => _moveSpeed;
    [SerializeField] float _persuitSpeed = 3f;
    public float PersuitSpeed => _persuitSpeed;
    [SerializeField] float _distanceStopOffset = 0.3f;

    [Header("Attack")]
    [SerializeField] float _damage = 50f;
    public float Damage => _damage;

    [SerializeField] float _attackRadius = 1.5f;
    public float AttackRadius => _attackRadius;


    [Header("Path")]
    List<Graph> _path = new List<Graph>();
    public List<Graph> Path => _path;

    int _pathIndex;

    [SerializeField] LayerMask _groundMask;

    protected override void Awake()
    {
        base.Awake();
        _controller = new MouseInputController(_groundMask);
    }

    void Start()
    {
        _currentHealth = _maxHealth;
        SetFSM();
    }

    void SetFSM()
    {
        _fsm = new FSM<NPCState>();

        var idle = new LiderIdle(_fsm, this);
        var followToClick = new LiderGoToClick(_fsm,this);
        var persuit = new LiderPersuit(_fsm, this);

        idle.AddTransition(NPCState.FollowToClick, followToClick);
        idle.AddTransition(NPCState.Persuit, persuit);

        followToClick.AddTransition(NPCState.Persuit, persuit);
        followToClick.AddTransition(NPCState.Idle, idle);

        persuit.AddTransition(NPCState.Idle, idle);
        persuit.AddTransition(NPCState.FollowToClick, followToClick);

        _fsm.SetInnitialFSM(idle);
    }

    protected override void Update()
    {
        base.Update();
        _controller.InputUpdate();

        if(_controller.HasClick)
        ClickPosition = _controller.Position;

        _fsm.OnUpdate();
    }

    void FixedUpdate()
    {
        _fsm.OnFixedUpdate();
    }

    public void SetPath(List<Graph> path)
    {
        _path = path;
        _pathIndex = 0;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _attackRadius);
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0f) return;

        _currentHealth -= damage;

        if (_currentHealth <= 0f)
        {
            gameObject.SetActive(false);
        }
    }

    public bool HasPath => _path != null && _pathIndex < _path.Count;

    public Vector3 CurrentPathPoint => _path[_pathIndex].transform.position;

    public void NextPathPoint() => _pathIndex++;

    public void SetPathToClick(Vector3 pos)
    {
        Graph start = PathManagerExamen.Instance.Closest(transform.position);
        Graph end = PathManagerExamen.Instance.Closest(pos);

        var path = PathManagerExamen.Instance.GetPath(
            start.transform.position,
            end.transform.position
        );

        SetPath(path);
    }

    public bool MoveByPath(float speed)
    {
        if (_path == null || _path.Count == 0) return false;
        if (_pathIndex >= _path.Count) return false;

        Vector3 nodeTarget = _path[_pathIndex].transform.position;
        Vector3 dir = nodeTarget - transform.position;

        if (dir.sqrMagnitude < _distanceStopOffset * _distanceStopOffset)
        {
            _pathIndex++;
            return _pathIndex < _path.Count;
        }

        AddForce(Seek(nodeTarget) * speed);
        Move();

        return true;
    }

    public void PersuitTarget()
    {
        if (_enemyTarget == null) return;

        AddForce(Seek(_enemyTarget.position) * _persuitSpeed);
        Move();
    }
}