using System.Collections.Generic;
using UnityEngine;

public class Lider : SteeringBase, IDamageable
{
    FSM<NPCState> _fsm;
    public FSM<NPCState> FSM => _fsm;

    [SerializeField] Transform _enemyTarget;
    public Transform EnemyTarget { get => _enemyTarget; set => _enemyTarget = value; }

    public Vector3 ClickPosition { get; private set; }

    [Header("FOV")]
    [SerializeField] FOV _fov;
    public FOV FOVV => _fov;

    MouseInputController _controller;
    public MouseInputController Controller => _controller;

    [Header("Health")]
    [SerializeField] float _maxHealth = 100f;
    float _currentHealth;

    public float CurrentHealth { get => _currentHealth; set => _currentHealth = value; }
    public float MaxHealth => _maxHealth;
    public float HealthPercent => _maxHealth > 0 ? _currentHealth / _maxHealth : 0f;
    public bool IsLowHP => _currentHealth < 30f;

    [Header("Movement")]
    [SerializeField] float _moveSpeed = 1f;
    public float MovSpeed => _moveSpeed;

    [SerializeField] float _persuitSpeed = 3f;
    public float PersuitSpeed => _persuitSpeed;

    [SerializeField] float _distanceStopOffset = 1.5f;

    [Header("Attack")]
    [SerializeField] float _damage = 50f;
    [SerializeField] float _attackCooldown = 1f;
    float _attackTimer = 0f;

    [SerializeField] float _attackRadius = 1.5f;
    public float AttackRadius => _attackRadius;

    [Header("Enemy Detection")]
    [SerializeField] LayerMask _enemyMask;
    [SerializeField] float _detectionRadius = 15f; // Радиус обнаружения врагов (больше чем FOV)

    [Header("Path")]
    List<Graph> _path;
    int _pathIndex;
    Vector3 _lastTarget;

    float _stuckTimer = 0f;
    Vector3 _lastPosition;

    [Header("Control")]
    [SerializeField] LayerMask _groundMask;
    [SerializeField] bool _isPlayerControlled = false;

    protected override void Awake()
    {
        base.Awake();
        if (_isPlayerControlled)
            _controller = new MouseInputController(_groundMask);
    }

    void Start()
    {
        _currentHealth = _maxHealth;
        _lastPosition = transform.position;
        SetFSM();

        if (_fov == null)
            Debug.LogError($"{gameObject.name}: FOV компонент не назначен!");
        if (_enemyMask == 0)
            Debug.LogError($"{gameObject.name}: Enemy Mask не настроен!");
    }

    void SetFSM()
    {
        _fsm = new FSM<NPCState>();

        var idle = new LiderIdle(_fsm, this);
        var gotoClick = new LiderGoToClick(_fsm, this);
        var pursuit = new LiderPersuit(_fsm, this);
        var patrol = new LiderPatrol(_fsm, this);
        var escape = new LiderEscape(_fsm, this);

        idle.AddTransition(NPCState.FollowToClick, gotoClick);
        idle.AddTransition(NPCState.Persuit, pursuit);
        idle.AddTransition(NPCState.Patrol, patrol);
        idle.AddTransition(NPCState.Salvation, escape);

        gotoClick.AddTransition(NPCState.Idle, idle);
        gotoClick.AddTransition(NPCState.Persuit, pursuit);
        gotoClick.AddTransition(NPCState.Salvation, escape);

        pursuit.AddTransition(NPCState.Idle, idle);
        pursuit.AddTransition(NPCState.FollowToClick, gotoClick);
        pursuit.AddTransition(NPCState.Salvation, escape);

        patrol.AddTransition(NPCState.Persuit, pursuit);
        patrol.AddTransition(NPCState.Idle, idle);
        patrol.AddTransition(NPCState.Salvation, escape);

        escape.AddTransition(NPCState.Idle, idle);
        escape.AddTransition(NPCState.Patrol, patrol);

        _fsm.SetInnitialFSM(_isPlayerControlled ? idle : patrol);
    }

    protected override void Update()
    {
        if (_isPlayerControlled && _controller != null)
        {
            _controller.InputUpdate();
            if (_controller.HasClick)
                ClickPosition = _controller.Position;
        }

        if (Vector3.Distance(transform.position, _lastPosition) < 0.05f)
            _stuckTimer += Time.deltaTime;
        else
            _stuckTimer = 0f;

        _lastPosition = transform.position;
        _attackTimer -= Time.deltaTime;

        if (_fsm != null)
            _fsm.OnUpdate();

        base.Update();
    }

    public bool HasPath => _path != null && _pathIndex < _path.Count;

    public void SetPath(List<Graph> path)
    {
        _path = path;
        _pathIndex = 0;
    }

    public bool HasLineOfSight(Vector3 target)
    {
        Vector3 dir = target - transform.position;
        return !Physics.Raycast(transform.position, dir.normalized, dir.magnitude, _obstacleMask);
    }

    public void SetPathToClick(Vector3 pos)
    {
        _lastTarget = pos;
        
        if (HasLineOfSight(pos))
        {
            _path = null;
            _pathIndex = 0;
            return;
        }

        Graph start = PathManagerExamen.Instance.Closest(transform.position);
        Graph end = PathManagerExamen.Instance.Closest(pos);

        if (start == null || end == null)
        {
            _path = null;
            _pathIndex = 0;
            return;
        }

        SetPath(PathManagerExamen.Instance.GetPath(start.transform.position, end.transform.position));
    }

    public void SetPathToNode(Graph targetNode)
    {
        if (targetNode == null)
        {
            _path = null;
            _pathIndex = 0;
            return;
        }

        _lastTarget = targetNode.transform.position;
        
        // Всегда используем pathfinding по graphs для патрулирования
        Graph start = PathManagerExamen.Instance.Closest(transform.position);
        Graph end = targetNode;

        if (start == null || end == null)
        {
            _path = null;
            _pathIndex = 0;
            return;
        }

        SetPath(PathManagerExamen.Instance.GetPath(start.transform.position, end.transform.position));
    }

    public bool MoveByPath(float speed)
    {
        if (!HasPath)
        {
            if (_lastTarget != Vector3.zero)
            {
                float distToTarget = Vector3.Distance(transform.position, _lastTarget);
                if (distToTarget < _distanceStopOffset)
                {
                    _velocity = Vector3.zero;
                    _lastTarget = Vector3.zero;
                    return false;
                }

                if (!HasLineOfSight(_lastTarget))
                {
                    SetPathToClick(_lastTarget);
                    if (HasPath)
                        return MoveByPath(speed);
                }

                Vector3 offset = (_lastTarget - transform.position).NoY();
                float dist = offset.magnitude;
                float slowingRange = _slowingRange;
                float targetSpeedFactor = Mathf.Min(dist / slowingRange, 1f);
                Vector3 desiredVel = offset.normalized * _maxSpeed * targetSpeedFactor * speed;
                AddForce(CalculateSteering(desiredVel));

                var avoidDir = Avoid.ChangeVelocity(_velocity);
                if (avoidDir != _velocity)
                    AddForce((avoidDir - _velocity) * AvoidanceWeight * 2f);
                return true;
            }
            return false;
        }

        var node = _path[_pathIndex].transform.position;
        float distance = Vector3.Distance(transform.position, node);

        if (distance < _distanceStopOffset || _stuckTimer > 1.5f)
        {
            _pathIndex++;
            _stuckTimer = 0f;
            
            if (_pathIndex >= _path.Count)
            {
                // Достигли конечной точки - полностью останавливаемся
                _velocity = Vector3.zero;
                _path = null;
                _pathIndex = 0;
                _lastTarget = Vector3.zero;
                return false;
            }
        }

        // Добавляем замедление при приближении к ноде
        Vector3 directionToNode = (node - transform.position).normalized;
        float speedFactor = Mathf.Min(distance / _slowingRange, 1f);
        speedFactor = Mathf.Max(speedFactor, 0.1f); // Минимальная скорость чтобы не останавливаться слишком рано
        
        Vector3 desired = directionToNode * _maxSpeed * speedFactor * speed;
        AddForce(CalculateSteering(desired));

        var avoidVel = Avoid.ChangeVelocity(_velocity);
        if (avoidVel != _velocity)
            AddForce((avoidVel - _velocity) * AvoidanceWeight * 3f);

        return true;
    }

    public void PersuitTarget()
    {
        if (_enemyTarget == null || !_enemyTarget.gameObject.activeInHierarchy)
        {
            return;
        }

        float dist = Vector3.Distance(transform.position, _enemyTarget.position);

        if (dist <= _attackRadius)
        {
            if (_attackTimer <= 0f)
            {
                var dmg = _enemyTarget.GetComponent<IDamageable>();
                if (dmg != null)
                {
                    string targetType = _enemyTarget.GetComponent<Solder>() != null ? "солдат" : "лидер";
                    dmg.TakeDamage(_damage);
                    Debug.Log($"[АТАКА] {gameObject.name} атакует {targetType} {_enemyTarget.name}, нанесено {_damage} урона. Расстояние: {dist:F2}, HP атакующего: {_currentHealth:F1}");
                }
                else
                {
                    Debug.LogWarning($"{gameObject.name} не может нанести урон {_enemyTarget.name} - нет IDamageable");
                }
                _attackTimer = _attackCooldown;
            }
            _velocity = Vector3.zero;
            return;
        }

        if (HasLineOfSight(_enemyTarget.position))
        {
            Vector3 offset = (_enemyTarget.position - transform.position).NoY();
            float distLOS = offset.magnitude;
            if (distLOS > _distanceStopOffset)
            {
                float speedFactor = Mathf.Min(distLOS / _slowingRange, 1f);
                Vector3 desiredVel = offset.normalized * _maxSpeed * speedFactor * _persuitSpeed;
                AddForce(CalculateSteering(desiredVel));
            }
            else
            {
                _velocity = Vector3.zero;
            }
        }
        else
        {
            if (!HasPath || _lastTarget != _enemyTarget.position)
            {
                SetPathToClick(_enemyTarget.position);
            }
            
            if (HasPath)
            {
                MoveByPath(_persuitSpeed);
            }
            else if (_lastTarget != Vector3.zero)
            {
                Vector3 offset = (_lastTarget - transform.position).NoY();
                float distToTarget = offset.magnitude;
                if (distToTarget > _distanceStopOffset)
                {
                    float speedFactor = Mathf.Min(distToTarget / _slowingRange, 1f);
                    Vector3 desiredVel = offset.normalized * _maxSpeed * speedFactor * _persuitSpeed;
                    AddForce(CalculateSteering(desiredVel));
                }
            }
            else
            {
                Vector3 offset = (_enemyTarget.position - transform.position).NoY();
                float distFallback = offset.magnitude;
                if (distFallback > _distanceStopOffset)
                {
                    float speedFactor = Mathf.Min(distFallback / _slowingRange, 1f);
                    Vector3 desiredVel = offset.normalized * _maxSpeed * speedFactor * _persuitSpeed;
                    AddForce(CalculateSteering(desiredVel));
                }
            }
        }

        var avoidVel = Avoid.ChangeVelocity(_velocity);
        if (avoidVel != _velocity)
            AddForce((avoidVel - _velocity) * AvoidanceWeight * 3f);
    }

    public bool FindEnemyInRadius()
    {
        // Ищем новых врагов в радиусе обнаружения (для начала преследования)
        if (_enemyMask == 0)
        {
            return false;
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, _detectionRadius, _enemyMask);
        
        if (hits.Length == 0)
        {
            return false;
        }

        Transform closestEnemy = null;
        float closestDist = float.MaxValue;
        bool foundInFOV = false;

        foreach (var h in hits)
        {
            if (h.transform == transform) continue;
            if (!h.gameObject.activeInHierarchy) continue;

            float dist = Vector3.Distance(transform.position, h.transform.position);
            
            // Проверяем что враг в радиусе обнаружения
            if (dist <= _detectionRadius)
            {
                bool inFOV = _fov != null && _fov.InFOV(h.transform.position);
                
                // Приоритет врагам в FOV
                if (inFOV)
                {
                    foundInFOV = true;
                    if (dist < closestDist)
                    {
                        closestEnemy = h.transform;
                        closestDist = dist;
                    }
                }
                else if (!foundInFOV)
                {
                    // Если нет врагов в FOV, выбираем ближайшего в радиусе
                    // (начнем преследование и повернемся к нему)
                    if (dist < closestDist)
                    {
                        closestEnemy = h.transform;
                        closestDist = dist;
                    }
                }
            }
        }

        if (closestEnemy != null)
        {
            _enemyTarget = closestEnemy;
            bool inFOV = _fov != null && _fov.InFOV(closestEnemy.position);
            
            // Если враг не в FOV, быстро поворачиваемся к нему
            if (!inFOV)
            {
                Vector3 direction = (closestEnemy.position - transform.position).NoY().normalized;
                if (direction != Vector3.zero)
                {
                    transform.forward = direction; // Мгновенный поворот для быстрого обнаружения
                }
            }
            
            Debug.Log($"{gameObject.name} ОБНАРУЖИЛ ВРАГА: {closestEnemy.name} (расстояние: {closestDist:F2}, в FOV: {inFOV})");
            return true;
        }

        return false;
    }

    public bool IsEnemyInFOV()
    {
        // Проверяем текущую цель - должна быть в FOV для продолжения преследования
        if (_enemyTarget != null && _enemyTarget.gameObject.activeInHierarchy)
        {
            float dist = Vector3.Distance(transform.position, _enemyTarget.position);
            
            // Проверяем что враг в радиусе обнаружения
            if (dist <= _detectionRadius)
            {
                // Враг должен быть в FOV для продолжения преследования
                if (_fov != null && _fov.InFOV(_enemyTarget.position))
                {
                    return true;
                }
                else
                {
                    // Враг вышел из FOV - теряем цель
                    Debug.Log($"{gameObject.name} потерял врага из FOV (спрятался)");
                    _enemyTarget = null;
                    return false;
                }
            }
            else
            {
                // Враг вышел из радиуса - теряем цель
                Debug.Log($"{gameObject.name} потерял врага (вышел из радиуса)");
                _enemyTarget = null;
                return false;
            }
        }

        return false;
    }

    public bool FindEnemyInFOV()
    {
        // Используем радиус обнаружения вместо только FOV
        return FindEnemyInRadius();
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0f) return;

        float oldHP = _currentHealth;
        _currentHealth -= damage;

        Debug.Log($"[УРОН] {gameObject.name} получил {damage} урона. HP: {oldHP:F1} → {_currentHealth:F1}/{_maxHealth}");
    }

    public void RecoverHealth(float deltaTime)
    {
        if (_currentHealth < _maxHealth)
        {
            _currentHealth += 5f * deltaTime;
            _currentHealth = Mathf.Min(_currentHealth, _maxHealth);
        }
    }
}
