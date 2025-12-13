using UnityEngine;

public class Solder : SteeringBase
{
    FSM<NPCState> _fsm;
    public FSM<NPCState> FSM => _fsm;

    [SerializeField] Lider _lider;
    public Lider Lider => _lider;

    [SerializeField] HealthSystem _healthSystem;

    [Header("FOV")]
    [SerializeField] FOV _fov;
    public FOV Fovv => _fov;

    [Header("Movement")]
    [SerializeField] float _separationRadius = 1.5f;
    [SerializeField] float _persuitSpeed = 3f;

    [Header("Save place")]
    [SerializeField] Transform _saveNode;
    public Transform SaveNode => _saveNode;

    [Header("Weights")]
    [SerializeField, Range(0, 3)] float _separationWeight = 1.5f;

    [Header("Layers")]
    [SerializeField] LayerMask _enemyMask;
    public LayerMask ObstacleMask => _obstacleMask;

    FlockingManagerExamen _flockingManagerExamen => FlockingManagerExamen.Instance;
    public Graph CurrentNode => PathManagerExamen.Instance.Closest(transform.position);
    Transform _enemy;
    public Transform EnemyTarget => _enemy;


    void Start()
    {
        _flockingManagerExamen.AddSolder(this);
        SetFSM();
    }

    void SetFSM()
    {
        _fsm = new FSM<NPCState>();

        var followToLider = new SoldersFollowLider(_fsm, this, _lider);
        var escape = new SolderEscape(_fsm, this, _saveNode);
        var persuit = new SolderPersuit(_fsm, this, _healthSystem);
        var followToliderByPath = new SolderFollowToLiderByPath(_fsm, this);

        followToLider.AddTransition(NPCState.Escape, escape);
        followToLider.AddTransition(NPCState.Persuit, persuit);
        followToLider.AddTransition(NPCState.FollowToLiderByPath, followToliderByPath);

        escape.AddTransition(NPCState.Persuit, persuit);
        escape.AddTransition(NPCState.FollowToLider, followToLider);
        escape.AddTransition(NPCState.FollowToLiderByPath, followToliderByPath);

        persuit.AddTransition(NPCState.FollowToLider, followToLider);
        persuit.AddTransition(NPCState.Escape, escape);
        persuit.AddTransition(NPCState.FollowToLiderByPath, followToliderByPath);

        _fsm.SetInnitialFSM(followToLider);
    }

    private void Update()
    {
        _fsm.OnUpdate();
    }

    void OnDisable()
    {
        if (_flockingManagerExamen != null)
            _flockingManagerExamen.RemoveSolder(this);
    }

    public void ClearEnemyTarget()
    {
        _enemy = null;
    }

    public void CheckEnemyInFOV()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, _fov.VisionRange, _enemyMask);

        Transform best = null;
        float bestDist = float.MaxValue;

        foreach (var hit in hits)
        {
            Vector3 pos = hit.transform.position;

            if (!_fov.InAngle(pos)) continue;
            if (!_fov.InSight(pos)) continue;

            float dist = (pos - transform.position).sqrMagnitude;

            if (dist <= bestDist * bestDist)
            {
                bestDist = dist;
                best = hit.transform;
            }
        }

        _enemy = best;
    }

    public void FollowLeader()
    {
        Vector3 toLeader = _lider.transform.position - transform.position;
        float dist = toLeader.magnitude;

        Vector3 force = Separation() * _separationWeight;

        if (dist > _slowingRange)
            force += Seek(_lider.transform.position);
        else
            force += Arrive(_lider.transform.position);

        AddForce(force);
        Move();
    }

    public bool EnemyInFOV()
    {
        return _enemy != null && _fov.InFOV(_enemy.position);
    }

    private Vector3 Separation()
    {
        Vector3 desired = Vector3.zero;

        int count = 0;

        for (int i = 0; i < _flockingManagerExamen.AllSolders.Count; i++)
        {
            Solder current = _flockingManagerExamen.AllSolders[i];

            if (current == this) continue;

            Vector3 dir = current.transform.position - transform.position;

            if (dir.sqrMagnitude > _separationRadius * _separationRadius) continue;

            count++;

            desired += dir;
        }

        if (count == 0) return desired;

        desired /= count;
        desired *= -1;

        return CalculateSteering(desired.normalized * _maxSpeed);
    }

    public bool LeaderInSight()
    {
        if (_lider == null) return false;

        Vector3 origin = transform.position;
        Vector3 target = _lider.transform.position;
        Vector3 dir = target - origin;
        float dist = dir.magnitude;

        if (!Physics.Raycast(origin, dir.normalized, dist, ObstacleMask))
            return true;

        return false;
    }

    public void PersuitTarget()
    {
        if (_enemy == null) return;

        AddForce(Seek(_enemy.position) * _persuitSpeed);
        Move();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _fov.VisionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _separationRadius);

            if (_lider == null) return;

            Vector3 origin = transform.position;
            Vector3 target = _lider.transform.position;
            Vector3 dir = target - origin;
            float dist = dir.magnitude;

            RaycastHit hit;
            bool blocked = Physics.Raycast(
                origin,
                dir.normalized,
                out hit,
                dist,
                ObstacleMask
            );

            // Линия луча
            Gizmos.color = blocked ? Color.red : Color.green;
            Gizmos.DrawLine(origin, target);

            // Точка попадания
            if (blocked)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(hit.point, 0.15f);
            }

            // Точка лидера
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(target, 0.2f);
        
    }
}