using System.Collections.Generic;
using UnityEngine;

public class Solder : SteeringBase, IDamageable
{
    FSM<NPCState> _fsm;

    [Header("Leader Reference")]
    [SerializeField] Lider _lider;

    public Lider Lider => _lider;

    [Header("FOV")]
    [SerializeField] float _viewRadius = 6f;
    [SerializeField] float _viewAngle = 90f;
    [SerializeField] LayerMask _enemyMask;

    Transform _currentTarget;

    [Header("Health")]
    [SerializeField] float _maxHealth = 100f;
    [SerializeField] float _lowHP = 30f;
    [SerializeField] float _healthRecoveryRate = 5f;
    float _currentHP;

    public float CurrentHP => _currentHP;
    public float MaxHealth => _maxHealth;
    public bool IsLowHP => _currentHP <= _lowHP;

    public void RecoverHealth(float deltaTime)
    {
        if (_currentHP < _maxHealth)
        {
            _currentHP += _healthRecoveryRate * deltaTime;
            _currentHP = Mathf.Min(_currentHP, _maxHealth);
        }
    }

    [Header("Combat")]
    [SerializeField] float _attackRadius = 1.5f;
    [SerializeField] float _damage = 25f;
    [SerializeField] float _attackCooldown = 1f;
    float _attackTimer;

    [Header("Flocking")]
    [SerializeField] float _separationRadius = 2.5f;
    [SerializeField] float _separationWeight = 2f;
    [SerializeField] float _arriveWeight = 1f;

    List<Graph> _path;
    int _pathIndex;
    float _stuckTimer;
    Vector3 _lastPosition;
    Vector3 _lastTarget;

    void Start()
    {
        _currentHP = _maxHealth;
        _attackTimer = 0f; // Можно атаковать сразу
        _lastPosition = transform.position;

        FlockingManagerExamen.Instance.AddSolder(this);

        SetupFSM();

        if (_enemyMask == 0)
            Debug.LogError($"{gameObject.name}: Enemy Mask не настроен!");
    }

    void SetupFSM()
    {
        _fsm = new FSM<NPCState>();

        var follow = new SoldersFollowLeader(_fsm, this);
        var pursue = new SolderPursuit(_fsm, this);
        var escape = new SolderEscape(_fsm, this);

        follow.AddTransition(NPCState.Persuit, pursue);
        follow.AddTransition(NPCState.Salvation, escape);

        pursue.AddTransition(NPCState.FollowToClick, follow);
        pursue.AddTransition(NPCState.Salvation, escape);

        escape.AddTransition(NPCState.FollowToClick, follow);
        escape.AddTransition(NPCState.Persuit, pursue);

        _fsm.SetInnitialFSM(follow);
    }

    protected override void Update()
    {
        base.Update();
        _fsm.OnUpdate();

        _attackTimer -= Time.deltaTime;

        if (Vector3.Distance(transform.position, _lastPosition) < 0.05f)
            _stuckTimer += Time.deltaTime;
        else
            _stuckTimer = 0f;

        _lastPosition = transform.position;
    }

    void FixedUpdate()
    {
        _fsm.OnFixedUpdate();
    }



    public bool CheckEnemyInFOV()
    {
        // Проверяем текущую цель
        if (_currentTarget != null)
        {
            if (_currentTarget.gameObject.activeInHierarchy)
            {
                Vector3 dir = (_currentTarget.position - transform.position).normalized;
                float dist = Vector3.Distance(transform.position, _currentTarget.position);

                if (dist <= _viewRadius && 
                    Vector3.Angle(transform.forward, dir) < _viewAngle / 2f &&
                    !Physics.Raycast(transform.position, dir, dist, _obstacleMask))
                {
                    return true;
                }
                else
                {
                    // Враг вышел из FOV - очищаем цель
                    _currentTarget = null;
                }
            }
            else
            {
                // Враг деактивирован - очищаем цель
                _currentTarget = null;
            }
        }

        // Ищем новых врагов
        Collider[] enemies = Physics.OverlapSphere(transform.position, _viewRadius, _enemyMask);

        if (enemies.Length == 0)
        {
            return false;
        }

        // ПРИОРИТЕТ: Сначала ищем солдат, потом лидеров
        Transform closestSoldier = null;
        float closestSoldierDist = float.MaxValue;
        Transform closestLeader = null;
        float closestLeaderDist = float.MaxValue;

        foreach (var e in enemies)
        {
            if (e.transform == transform) continue;
            if (!e.gameObject.activeInHierarchy) continue;

            // Проверяем что это действительно враг (не союзник)
            bool isEnemy = false;
            bool isSoldier = false;
            bool isLeader = false;
            
            // Проверяем если это солдат
            var enemySolder = e.GetComponent<Solder>();
            if (enemySolder != null)
            {
                // Это враг если у него другой лидер или лидер null
                if (enemySolder.Lider != _lider)
                {
                    isEnemy = true;
                    isSoldier = true;
                }
            }
            // Проверяем если это лидер
            else if (e.GetComponent<Lider>() != null)
            {
                // Лидер всегда враг (если он не наш лидер)
                if (e.transform != _lider?.transform)
                {
                    isEnemy = true;
                    isLeader = true;
                }
            }
            else
            {
                // Если это не солдат и не лидер, но в enemyMask - значит враг
                isEnemy = true;
            }

            if (!isEnemy) continue;

            Vector3 dir = (e.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dir);
            float dist = Vector3.Distance(transform.position, e.transform.position);

            // Проверяем FOV и Line of Sight
            if (angle < _viewAngle / 2f)
            {
                if (!Physics.Raycast(transform.position, dir, dist, _obstacleMask))
                {
                    // ПРИОРИТЕТ: Сначала сохраняем солдат, потом лидеров
                    if (isSoldier && dist < closestSoldierDist)
                    {
                        closestSoldier = e.transform;
                        closestSoldierDist = dist;
                    }
                    else if (isLeader && dist < closestLeaderDist)
                    {
                        closestLeader = e.transform;
                        closestLeaderDist = dist;
                    }
                }
            }
        }

        // Выбираем цель: сначала солдат, если нет солдат - лидер
        Transform closestEnemy = null;
        float closestDist = float.MaxValue;
        string enemyType = "";

        if (closestSoldier != null)
        {
            closestEnemy = closestSoldier;
            closestDist = closestSoldierDist;
            enemyType = "солдат";
        }
        else if (closestLeader != null)
        {
            closestEnemy = closestLeader;
            closestDist = closestLeaderDist;
            enemyType = "лидер";
        }

        if (closestEnemy != null)
        {
            _currentTarget = closestEnemy;
            Debug.Log($"{gameObject.name} нашел врага ({enemyType}): {closestEnemy.name} (расстояние: {closestDist:F2})");
            return true;
        }

        return false;
    }

    public void FollowLeader()
    {
        if (_lider == null) return;

        Vector3 leaderPos = _lider.transform.position;
        float distToLeader = Vector3.Distance(transform.position, leaderPos);

        if (!HasLineOfSight(leaderPos))
        {
            MoveWithTheta(leaderPos);
            return;
        }

        Vector3 desiredVelocity = Vector3.zero;

        if (distToLeader > 3f)
        {
            Vector3 offset = (leaderPos - transform.position).NoY();
            float speed = _maxSpeed * Mathf.Min(distToLeader / _slowingRange, 1f);
            desiredVelocity = offset.normalized * speed;
        }
        else if (distToLeader < 2f)
        {
            Vector3 offset = (transform.position - leaderPos).NoY();
            desiredVelocity = offset.normalized * _maxSpeed * 0.5f;
        }

        if (desiredVelocity.sqrMagnitude > 0.01f)
        {
            Vector3 arriveSteering = CalculateSteering(desiredVelocity);
            AddForce(arriveSteering * _arriveWeight);
        }

        Vector3 separation = Separation();
        if (separation.sqrMagnitude > 0.01f)
            AddForce(separation * _separationWeight);

        var avoid = Avoid.ChangeVelocity(_velocity);
        if (avoid != _velocity)
            AddForce((avoid - _velocity) * AvoidanceWeight * 2f);
    }

    public bool HasLineOfSight(Vector3 target)
    {
        Vector3 dir = target - transform.position;
        return !Physics.Raycast(transform.position, dir.normalized, dir.magnitude, _obstacleMask);
    }

    Vector3 Separation()
    {
        Vector3 result = Vector3.zero;
        int count = 0;

        foreach (var s in FlockingManagerExamen.Instance.AllSolders)
        {
            if (s == this) continue;
            if (s.Lider != this.Lider) continue;

            Vector3 diff = transform.position - s.transform.position;

            if (diff.sqrMagnitude < _separationRadius * _separationRadius)
            {
                result += diff.normalized / diff.magnitude;
                count++;
            }
        }

        if (count == 0) return Vector3.zero;

        result /= count;
        result = result.normalized * _maxSpeed;

        return CalculateSteering(result);
    }


    public void MoveWithTheta(Vector3 target)
    {
        if (HasLineOfSight(target))
        {
            AddForce(Seek(target));
            var avoidDir = Avoid.ChangeVelocity(_velocity);
            if (avoidDir != _velocity)
                AddForce((avoidDir - _velocity) * AvoidanceWeight * 2f);
            return;
        }

        bool newPathNeeded =
            _path == null ||
            _pathIndex >= _path.Count ||
            Vector3.Distance(_lastTarget, target) > 1f;

        if (newPathNeeded)
        {
            Graph start = PathManagerExamen.Instance.Closest(transform.position);
            Graph end = PathManagerExamen.Instance.Closest(target);

            if (start != null && end != null)
            {
                _path = PathManagerExamen.Instance.GetPath(start.transform.position, end.transform.position);
                _pathIndex = 0;
                _lastTarget = target;
            }
            else return;
        }

        if (_pathIndex >= _path.Count) return;

        Vector3 node = _path[_pathIndex].transform.position;
        float dist = Vector3.Distance(transform.position, node);

        if (dist < 1.5f || _stuckTimer > 2f)
        {
            _pathIndex++;
            _stuckTimer = 0;
            return;
        }

        Vector3 directionToNode = (node - transform.position).normalized;
        Vector3 desired = directionToNode * _maxSpeed;
        AddForce(CalculateSteering(desired));

        var avoidVel = Avoid.ChangeVelocity(_velocity);
        if (avoidVel != _velocity)
            AddForce((avoidVel - _velocity) * AvoidanceWeight * 2f);
    }

    public void Attack()
    {
        if (_currentTarget == null)
        {
            Debug.LogWarning($"{gameObject.name} пытается атаковать, но Target = null!");
            return;
        }

        if (!_currentTarget.gameObject.activeInHierarchy)
        {
            Debug.Log($"{gameObject.name} цель деактивирована, очищаем");
            _currentTarget = null;
            return;
        }

        float dist = Vector3.Distance(transform.position, _currentTarget.position);

        if (dist <= _attackRadius)
        {
            _velocity = Vector3.zero;
            if (_attackTimer <= 0f)
            {
                var dmg = _currentTarget.GetComponent<IDamageable>();
                if (dmg != null)
                {
                    string targetType = _currentTarget.GetComponent<Solder>() != null ? "солдат" : "лидер";
                    dmg.TakeDamage(_damage);
                    Debug.Log($"[АТАКА] {gameObject.name} атакует {targetType} {_currentTarget.name}, нанесено {_damage} урона. Расстояние: {dist:F2}, HP атакующего: {_currentHP:F1}");
                }
                else
                {
                    Debug.LogWarning($"{gameObject.name} не может нанести урон {_currentTarget.name} - нет IDamageable");
                }
                _attackTimer = _attackCooldown;
            }
            else
            {
                // Таймер еще не готов
                Debug.Log($"{gameObject.name} в радиусе атаки, но таймер: {_attackTimer:F2}");
            }
            return;
        }

        // Движемся к врагу для атаки
        MoveWithTheta(_currentTarget.position);

        var avoid = Avoid.ChangeVelocity(_velocity);
        if (avoid != _velocity)
            AddForce((avoid - _velocity) * AvoidanceWeight * 2f);
    }

    public Transform Target => _currentTarget;

    public void ClearTarget() => _currentTarget = null;

    public void TakeDamage(float dmg)
    {
        if (dmg <= 0f) return;

        float oldHP = _currentHP;
        _currentHP -= dmg;
        _currentHP = Mathf.Max(0, _currentHP);

        Debug.Log($"[УРОН] {gameObject.name} получил {dmg} урона. HP: {oldHP:F1} → {_currentHP:F1}/{_maxHealth}");

        if (_currentHP <= 0)
        {
            Debug.Log($"{gameObject.name} уничтожен!");
            FlockingManagerExamen.Instance.RemoveSolder(this);
            Destroy(gameObject);
        }
    }
}
