using System.Collections.Generic;
using UnityEngine;

public class Solder : SteeringBase, IDamageable
{
    private FSM<NPCState> _fsm;
    public FSM<NPCState> FSM => _fsm;

    [SerializeField] private Lider _lider;
    public Lider Lider => _lider;

    [Header("FOV")]
    [SerializeField] private float _viewRadius = 6f;
    [SerializeField] private float _viewAngle = 90f;
    public float ViewRadius => _viewRadius;
    public float ViewAngle => _viewAngle;

    [Header("Health")]
    [SerializeField] private float _maxHealth = 100f;
    [SerializeField] private float _lowHealthThreshold = 30f;
    private float _currentHealth;
    public float CurrentHealth => _currentHealth;
    public float HealthPercentage => _currentHealth / _maxHealth;
    public bool IsLowHealth => _currentHealth <= _lowHealthThreshold;

    [Header("Movement")]
    [SerializeField] private float _separationRadius = 1.5f;
    [SerializeField] private float _arriveRadius = 2f;
    public float SeparationRadius => _separationRadius;
    public float ArriveRadius => _arriveRadius;

    [Header("Weights")]
    [SerializeField, Range(0, 3)] private float _separationWeight = 1.5f;
    [SerializeField, Range(0, 3)] private float _arriveWeight = 1f;
    public float SeparationWeight => _separationWeight;
    public float ArriveWeight => _arriveWeight;

    [Header("Attack")]
    [SerializeField] private float _damage = 25f;
    [SerializeField] private float _attackRadius = 1.5f;
    [SerializeField] private float _attackCooldown = 1f;
    private float _attackTimer = 0f;
    public float Damage => _damage;
    public float AttackRadius => _attackRadius;

    [Header("Layers")]
    [SerializeField] private LayerMask _enemyMask;
    public LayerMask EnemyMask => _enemyMask;

    private FlockingManagerExamen _flockingManagerExamen => FlockingManagerExamen.Instance;

    private Transform _currentEnemyTarget;
    public Transform CurrentEnemyTarget => _currentEnemyTarget;

    public void ClearEnemyTarget() => _currentEnemyTarget = null;

    private List<Graph> _path;
    private int _pathIndex;
    private Vector3 _lastTarget;
    private float _stuckTimer = 0f;
    private Vector3 _lastPosition;

    public List<Graph> Path => _path;
    public int PathIndex => _pathIndex;
    public bool HasPath => _path != null && _pathIndex < _path.Count;
    public Vector3 CurrentPathPoint =>
        _path != null && _pathIndex < _path.Count ? _path[_pathIndex].transform.position
                                                  : transform.position;


    private void Start()
    {
        _currentHealth = _maxHealth;
        _lastPosition = transform.position;

        _flockingManagerExamen.AddSolder(this);
        SetFSM();
    }

    private void SetFSM()
    {
        _fsm = new FSM<NPCState>();

        var followLeader = new SoldersFollowLider(_fsm, this);
        var persuit = new SolderPersuit(_fsm, this);
        var escape = new SolderEscape(_fsm, this);

        followLeader.AddTransition(NPCState.Persuit, persuit);
        followLeader.AddTransition(NPCState.Salvation, escape);

        persuit.AddTransition(NPCState.Salvation, escape);
        persuit.AddTransition(NPCState.FollowToClick, followLeader);

        escape.AddTransition(NPCState.FollowToClick, followLeader);
        escape.AddTransition(NPCState.Persuit, persuit);

        _fsm.SetInnitialFSM(followLeader);
    }

    protected override void Update()
    {
        base.Update();

        _fsm.OnUpdate();
        _attackTimer -= Time.deltaTime;

        if (Vector3.Distance(transform.position, _lastPosition) < 0.1f)
            _stuckTimer += Time.deltaTime;
        else
            _stuckTimer = 0f;

        _lastPosition = transform.position;
    }

    private void FixedUpdate()
    {
        _fsm.OnFixedUpdate();
    }


    public bool CheckEnemyInFOV()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, _viewRadius, _enemyMask);

        foreach (var enemy in enemies)
        {
            Vector3 dir = (enemy.transform.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, dir) < _viewAngle / 2f)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                // _obstacleMask унаследован из SteeringBase
                if (!Physics.Raycast(transform.position, dir, distance, _obstacleMask))
                {
                    _currentEnemyTarget = enemy.transform;
                    return true;
                }
            }
        }

        _currentEnemyTarget = null;
        return false;
    }


    public void Attack()
    {
        if (_currentEnemyTarget == null) return;

        float distance = Vector3.Distance(transform.position, _currentEnemyTarget.position);

        if (distance <= _attackRadius)
        {
            if (_attackTimer <= 0f)
            {
                IDamageable damageable = _currentEnemyTarget.GetComponent<IDamageable>();
                if (damageable != null)
                    damageable.TakeDamage(_damage);

                _attackTimer = _attackCooldown;
            }
        }
        else
        {
            FollowWithTheta(_currentEnemyTarget.position);
        }
    }


    public void FollowLeader()
    {
        if (_lider == null) return;

        Vector3 leaderPos = _lider.transform.position;

        if (HasLineOfSight(leaderPos))
        {
            // Arrive к лидеру
            Vector3 arriveVel = Arrive(_lider.transform, _velocity);
            AddForce(arriveVel * _arriveWeight);

            // Separation
            Vector3 separation = Separation();
            if (separation.sqrMagnitude > 0.01f)
                AddForce(separation * _separationWeight);

            // Avoidance
            Vector3 avoidance = Avoid.ChangeVelocity(_velocity);
            if (avoidance != _velocity)
                AddForce((avoidance - _velocity) * AvoidanceWeight);
        }
        else
        {
            // если лидер за стеной, используем Theta*
            FollowWithTheta(leaderPos);
        }
    }

    public Vector3 Separation()
    {
        Vector3 desired = Vector3.zero;
        int count = 0;

        foreach (var boid in _flockingManagerExamen.AllSolders)
        {
            if (boid == this) continue;
            if (boid.Lider != _lider) continue;

            Vector3 dir = transform.position - boid.transform.position;

            if (dir.sqrMagnitude > _separationRadius * _separationRadius)
                continue;

            count++;
            desired += dir.normalized / dir.magnitude;
        }

        if (count == 0) return Vector3.zero;

        desired /= count;
        desired = desired.normalized * _maxSpeed;

        return CalculateSteering(desired);
    }

    public bool HasLineOfSight(Vector3 target)
    {
        Vector3 dir = target - transform.position;
        float distance = dir.magnitude;

        return !Physics.Raycast(transform.position, dir.normalized, distance, _obstacleMask);
    }


    public void FollowWithTheta(Vector3 target)
    {
        // если прямая видимость — просто Seek
        if (HasLineOfSight(target))
        {
            AddForce(Seek(target));

            Vector3 avoidance = Avoid.ChangeVelocity(_velocity);
            if (avoidance != _velocity)
                AddForce((avoidance - _velocity) * AvoidanceWeight);

            _path = null;
            _pathIndex = 0;
            return;
        }

        bool targetChanged = Vector3.Distance(_lastTarget, target) > 1f;
        bool needsNewPath = _path == null || _pathIndex >= _path.Count || targetChanged;

        if (needsNewPath)
        {
            Graph start = PathManagerExamen.Instance.Closest(transform.position);
            Graph end = PathManagerExamen.Instance.Closest(target);

            if (start != null && end != null)
            {
                _path = PathManagerExamen.Instance.GetPath(start.transform.position, end.transform.position);
                _pathIndex = 0;
                _lastTarget = target;
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

            if (distanceToNode < 0.5f || _stuckTimer > 2f)
            {
                _pathIndex++;
                _stuckTimer = 0f;
            }

            if (_pathIndex < _path.Count)
            {
                Vector3 desired = Seek(_path[_pathIndex].transform.position);
                AddForce(desired);

                Vector3 avoidance = Avoid.ChangeVelocity(_velocity);
                if (avoidance != _velocity)
                    AddForce((avoidance - _velocity) * AvoidanceWeight);
            }
        }
    }

    public void SetPath(List<Graph> path)
    {
        _path = path;
        _pathIndex = 0;
        _lastTarget = Vector3.zero;
    }

    public void NextPathPoint() => _pathIndex++;


    public void TakeDamage(float damage)
    {
        if (damage <= 0f) return;

        _currentHealth -= damage;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);

        if (_currentHealth <= 0f)
        {
            _flockingManagerExamen.RemoveSolder(this);
            gameObject.SetActive(false);
        }
    }


    private void OnDrawGizmos()
    {
        if (_fsm != null && _fsm.CurrentState != null)
        {
            string stateName = _fsm.CurrentState.GetType().Name;

            if (stateName.Contains("FollowLider") || stateName.Contains("FollowLeader"))
                Gizmos.color = Color.blue;
            else if (stateName.Contains("Persuit"))
                Gizmos.color = Color.red;
            else if (stateName.Contains("Escape") || stateName.Contains("Salvation"))
                Gizmos.color = Color.yellow;
            else
                Gizmos.color = Color.white;

            Gizmos.DrawWireSphere(transform.position + Vector3.up * 2.5f, 0.3f);
        }

        // FOV радиус
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _viewRadius);

        // Separation радиус
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _separationRadius);

        // радиус атаки
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, _attackRadius);

        // полоска HP
        if (_currentHealth > 0)
        {
            float hpPercent = _currentHealth / _maxHealth;
            Gizmos.color = hpPercent > 0.5f ? Color.green :
                           hpPercent > 0.3f ? Color.yellow : Color.red;
            Gizmos.DrawLine(transform.position,
                            transform.position + Vector3.up * 2f * hpPercent);
        }

        // линия до врага
        if (_currentEnemyTarget != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, _currentEnemyTarget.position);
        }

        // путь
        if (_path != null && _path.Count > 0)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < _path.Count - 1; i++)
            {
                if (_path[i] != null && _path[i + 1] != null)
                    Gizmos.DrawLine(_path[i].transform.position, _path[i + 1].transform.position);
            }
        }
    }

    private void OnDestroy()
    {
        if (_flockingManagerExamen != null)
            _flockingManagerExamen.RemoveSolder(this);
    }
}
