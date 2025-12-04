using System.Collections.Generic;
using UnityEngine;

public class Solder : SteeringBase
{
    [SerializeField] Lider _lider;

    [Header("FOV")]
    [SerializeField] float _viewRadius = 6f;
    [SerializeField] float _viewAngle = 90f;

    [Header("Movement")]
    [SerializeField] float _separationRadius = 1.5f;
    [SerializeField] float _arriveRadius = 2f;

    [Header("Weights")]
    [SerializeField, Range(0, 3)] float _separationWeight = 1.5f;
    [SerializeField, Range(0, 3)] float _arriveWeight = 1f;

    [Header("Layers")]
    [SerializeField] LayerMask _enemyMask;
    [SerializeField] LayerMask _obstacleMask;

    FlockingManagerExamen _flockingManagerExamen => FlockingManagerExamen.Instance;

    Transform _currentEnemyTarget;

    List<Graph> _path;
    int _pathIndex;

    void Start()
    {
        _flockingManagerExamen.AddSolder(this);
    }

    void Update()
    {
        if (CheckEnemyInFOV())
        {
            Attack();
            return;
        }

        FollowLeader();

        AddForce(_avoid.ChangeVelocity(_velocity) * _avoidanceWeight);
        Move();
    }

    // ✅ 1. FoV + Attack
    bool CheckEnemyInFOV()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, _viewRadius, _enemyMask);

        foreach (var enemy in enemies)
        {
            Vector3 dir = (enemy.transform.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, dir) < _viewAngle / 2)
            {
                if (!Physics.Raycast(transform.position, dir,
                    Vector3.Distance(transform.position, enemy.transform.position),
                    _obstacleMask))
                {
                    _currentEnemyTarget = enemy.transform;
                    return true;
                }
            }
        }

        _currentEnemyTarget = null;
        return false;
    }

    void Attack()
    {
        AddForce(Seek(_currentEnemyTarget.position));
    }

    // ✅ 2. Leader Following (Arrive + Separation)
    void FollowLeader()
    {
        Vector3 arrive = Arrive(_lider.transform, _velocity);
        Vector3 separation = Separation();

        AddForce(arrive * _arriveWeight + separation * _separationWeight);
    }

    Vector3 Separation()
    {
        Vector3 desired = Vector3.zero;
        int count = 0;

        foreach (var boid in _flockingManagerExamen.AllSolders)
        {
            if (boid == this) continue;

            Vector3 dir = transform.position - boid.transform.position;

            if (dir.sqrMagnitude > _separationRadius * _separationRadius) continue;

            count++;
            desired += dir.normalized / dir.magnitude;
        }

        if (count == 0) return Vector3.zero;

        desired /= count;
        desired = desired.normalized * _maxSpeed;

        return CalculateSteering(desired);
    }

    // ✅ 3. LOS + THETA*
    bool HasLineOfSight(Vector3 target)
    {
        Vector3 dir = target - transform.position;

        return !Physics.Raycast(transform.position, dir.normalized,
            dir.magnitude, _obstacleMask);
    }

    void FollowWithTheta(Vector3 target)
    {
        if (HasLineOfSight(target))
        {
            AddForce(Seek(target));
            return;
        }

        if (_path == null || _pathIndex >= _path.Count)
        {
            Graph start = PathManagerExamen.Instance.Closest(transform.position);
            Graph end = PathManagerExamen.Instance.Closest(target);

            _path = PathManagerExamen.Instance.GetPath(
                start.transform.position,
                end.transform.position); // Theta*
            _pathIndex = 0;
        }

        Vector3 nodeTarget = _path[_pathIndex].transform.position;

        if (Vector3.Distance(transform.position, nodeTarget) < 0.5f)
            _pathIndex++;

        AddForce(Seek(nodeTarget));
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _viewRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _separationRadius);
    }

    void OnDestroy()
    {
        if (_flockingManagerExamen != null)
            _flockingManagerExamen.RemoveSolder(this);
    }
}