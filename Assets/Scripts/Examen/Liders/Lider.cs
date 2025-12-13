using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lider : SteeringBase
{
    [SerializeField] bool _isControlable = true;
    public bool IsControlable => _isControlable;

    FSM<NPCState> _fsm;
    public FSM<NPCState> FSM => _fsm;

    [SerializeField] Transform _enemyTarget;
    public Transform EnemyTarget => _enemyTarget;

    public Vector3 ClickPosition { get; private set; }

    [SerializeField] FOV _fov;
    public FOV FOVV => _fov;

    MouseInputController _controller;
    public MouseInputController Controller => _controller;

    [Header("Movement")]
    [SerializeField] float _persuitSpeed = 3f;
    public float PersuitSpeed => _persuitSpeed;

    [SerializeField] LayerMask _enemyMask;

    List<Graph> _path = new List<Graph>();
    public List<Graph> Path => _path;
    int _pathIndex;
    public int PathIndex => _pathIndex;

    public Graph CurrentNode => PathManagerExamen.Instance.Closest(transform.position);

    [SerializeField] LayerMask _groundMask;

    protected override void Awake()
    {
        base.Awake();

        if(_isControlable) _controller = new MouseInputController(_groundMask);
    }

    void Start()
    {
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

    private void Update()
    {
        if(_isControlable)
        {
            _controller.InputUpdate();

            if (_controller.HasClick)
                ClickPosition = _controller.Position;
        }

        _fsm.OnUpdate();
    }

    public void WalkRandom()
    {
        AddForce(Wander());
        Move();
    }

    public void PersuitTarget()
    {
        if (_enemyTarget == null) return;

        AddForce(Seek(_enemyTarget.position) * _persuitSpeed);
        Move();
    }

    public void ClearClick()
    {
        _path.Clear();
        _pathIndex = 0;
    }

    public void SetPath(List<Graph> path)
    { 
        _path = path;
        _pathIndex = 0;
    }

    public void NextPooint() => _pathIndex++;

    public bool EnemyInFOV()
    {
        return _enemyTarget != null && _fov.InFOV(_enemyTarget.position);
    }

    public void FindTargetInFOV()
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

        _enemyTarget = best;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 1.5f);
    }
}