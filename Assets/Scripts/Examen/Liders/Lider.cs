using System.Collections.Generic;
using UnityEngine;

public class Lider : SteeringBase, IDamageable
{
    private FSM<NPCState> _fsm;
    public FSM<NPCState> FSM => _fsm;

    [SerializeField] private Transform _enemyTarget;
    public Transform EnemyTarget => _enemyTarget;

    public Vector3 ClickPosition { get; private set; }

    [SerializeField] private FOV _fov;
    public FOV FOVV => _fov;

    private MouseInputController _controller;
    public MouseInputController Controller => _controller;

    [Header("Health")]
    [SerializeField] private float _maxHealth = 100f;
    private float _currentHealth;
    public float CurrentHealth => _currentHealth;

    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 1f;
    public float MovSpeed => _moveSpeed;

    [SerializeField] private float _persuitSpeed = 3f;
    public float PersuitSpeed => _persuitSpeed;

    [SerializeField] private float _distanceStopOffset = 0.3f;

    [Header("Attack")]
    [SerializeField] private float _damage = 50f;
    public float Damage => _damage;

    [SerializeField] private float _attackCooldown = 1f;
    private float _attackTimer = 0f;

    [SerializeField] private float _attackRadius = 1.5f;
    public float AttackRadius => _attackRadius;

    [Header("Enemy Detection")]
    [SerializeField] private LayerMask _enemyMask;
    public LayerMask EnemyMask => _enemyMask;

    [SerializeField] private LayerMask _groundMask;
    [SerializeField] private bool _isPlayerControlled = true;

    [Header("Path")]
    private List<Graph> _path = new List<Graph>();
    public List<Graph> Path => _path;

    private int _pathIndex;
    private Vector3 _lastTarget;
    private float _stuckTimer = 0f;
    private Vector3 _lastPosition;

    public bool HasPath => _path != null && _pathIndex < _path.Count;
    public Vector3 CurrentPathPoint => _path[_pathIndex].transform.position;


    protected override void Awake()
    {
        base.Awake();

        if (_isPlayerControlled)
            _controller = new MouseInputController(_groundMask);
    }

    private void Start()
    {
        _currentHealth = _maxHealth;
        _lastPosition = transform.position;
        SetFSM();
    }

    private void SetFSM()
    {
        _fsm = new FSM<NPCState>();

        var idle = new LiderIdle(_fsm, this);
        var followToClick = new LiderGoToClick(_fsm, this);
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

        if (_isPlayerControlled && _controller != null)
        {
            _controller.InputUpdate();
            if (_controller.HasClick)
                ClickPosition = _controller.Position;
        }

        if (Vector3.Distance(transform.position, _lastPosition) < 0.1f)
            _stuckTimer += Time.deltaTime;
        else
            _stuckTimer = 0f;

        _lastPosition = transform.position;

        _attackTimer -= Time.deltaTime;

        _fsm.OnUpdate();
    }

    private void FixedUpdate()
    {
        _fsm.OnFixedUpdate();
    }


    public void SetPath(List<Graph> path)
    {
        _path = path;
        _pathIndex = 0;
        _lastTarget = Vector3.zero;
    }

    public void NextPathPoint() => _pathIndex++;

    public void SetPathToClick(Vector3 pos)
    {
        Graph start = PathManagerExamen.Instance.Closest(transform.position);
        Graph end = PathManagerExamen.Instance.Closest(pos);

        if (start != null && end != null)
        {
            var path = PathManagerExamen.Instance.GetPath(
                start.transform.position,
                end.transform.position
            );
            SetPath(path);
            _lastTarget = pos;
        }
    }

    public void ClearEnemyTarget()
    {
        _enemyTarget = null;
    }

    public void StopCompletely()
    {
        _velocity = Vector3.zero;
        _path = null;
        _pathIndex = 0;
        _lastTarget = Vector3.zero;
    }


    public bool MoveByPath(float speed)
    {
        if (_path == null || _path.Count == 0) return false;
        if (_pathIndex >= _path.Count) return false;

        Vector3 nodeTarget = _path[_pathIndex].transform.position;
        float distanceToNode = Vector3.Distance(transform.position, nodeTarget);

        if (distanceToNode < _distanceStopOffset || _stuckTimer > 2f)
        {
            _pathIndex++;
            _stuckTimer = 0f;
            return _pathIndex < _path.Count;
        }

        Vector3 desired = Seek(nodeTarget) * speed;
        AddForce(desired);

        Vector3 avoidance = Avoid.ChangeVelocity(_velocity);
        if (avoidance != _velocity)
            AddForce((avoidance - _velocity) * AvoidanceWeight);

        return true;
    }


    public void PersuitTarget()
    {
        if (_enemyTarget == null) return;

        float distance = Vector3.Distance(transform.position, _enemyTarget.position);

        // атака в радиусе
        if (distance <= _attackRadius)
        {
            if (_attackTimer <= 0f)
            {
                IDamageable damageable = _enemyTarget.GetComponent<IDamageable>();
                if (damageable != null)
                    damageable.TakeDamage(_damage);

                _attackTimer = _attackCooldown;
            }
            return;
        }

        // иначе — строим путь к врагу через Theta*
        Vector3 targetPos = _enemyTarget.position;
        bool targetChanged = Vector3.Distance(_lastTarget, targetPos) > 1f;
        bool needsNewPath = _path == null || _pathIndex >= _path.Count || targetChanged;

        if (needsNewPath)
        {
            Graph start = PathManagerExamen.Instance.Closest(transform.position);
            Graph end = PathManagerExamen.Instance.Closest(targetPos);

            if (start != null && end != null)
            {
                _path = PathManagerExamen.Instance.GetPath(start.transform.position, end.transform.position);
                _pathIndex = 0;
                _lastTarget = targetPos;
            }
            else
            {
                _velocity = Vector3.zero;
                return;
            }
        }

        if (_path != null && _pathIndex < _path.Count)
        {
            Vector3 nodeTarget = _path[_pathIndex].transform.position;
            float distanceToNode = Vector3.Distance(transform.position, nodeTarget);

            if (distanceToNode < _distanceStopOffset || _stuckTimer > 2f)
            {
                _pathIndex++;
                _stuckTimer = 0f;
            }

            if (_pathIndex < _path.Count)
            {
                Vector3 desired = Seek(_path[_pathIndex].transform.position) * _persuitSpeed;
                AddForce(desired);
            }
        }

        Vector3 avoidVel = Avoid.ChangeVelocity(_velocity);
        if (avoidVel != _velocity)
            AddForce((avoidVel - _velocity) * AvoidanceWeight);
    }


    public bool FindEnemyInFOV()
    {
        if (_fov == null) return false;

        // ищем коллайдеры на слое врага
        Collider[] enemies = Physics.OverlapSphere(transform.position, _fov.VisionRange, _enemyMask);

        foreach (var enemy in enemies)
        {
            if (_fov.InFOV(enemy.transform.position))
            {
                _enemyTarget = enemy.transform;
                return true;
            }
        }

        _enemyTarget = null;
        return false;
    }


    public void TakeDamage(float damage)
    {
        if (damage <= 0f) return;

        _currentHealth -= damage;

        if (_currentHealth <= 0f)
            gameObject.SetActive(false);
    }

    private void OnDrawGizmos()
    {
        // текущий State
        if (_fsm != null && _fsm.CurrentState != null)
        {
            string stateName = _fsm.CurrentState.GetType().Name;
            if (stateName.Contains("Idle")) Gizmos.color = Color.green;
            else if (stateName.Contains("GoToClick") ||
                     stateName.Contains("FollowToClick")) Gizmos.color = Color.blue;
            else if (stateName.Contains("Persuit")) Gizmos.color = Color.red;
            else Gizmos.color = Color.white;

            Gizmos.DrawWireSphere(transform.position + Vector3.up * 2.5f, 0.3f);
        }

        // радиус атаки
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _attackRadius);

        // путь
        if (_path != null && _path.Count > 0)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < _path.Count - 1; i++)
            {
                if (_path[i] != null && _path[i + 1] != null)
                    Gizmos.DrawLine(_path[i].transform.position, _path[i + 1].transform.position);
            }

            if (_pathIndex < _path.Count && _path[_pathIndex] != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(_path[_pathIndex].transform.position, 0.5f);
            }
        }

        if (_enemyTarget != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, _enemyTarget.position);
        }
    }
}
