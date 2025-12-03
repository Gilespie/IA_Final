using UnityEngine;

public class GuardianLastPoint : State<NPCState>
{
    Guardian _guardian;

    public GuardianLastPoint(FSM<NPCState> fsm, Guardian guardian) : base(fsm)
    {
        _guardian = guardian;
    }

    public override void Enter()
    {
        Debug.Log("LastTargetPoint enter");
        var start = _guardian.CurrentNode;
        var end = PathManager.Instance.Closest(_guardian.Target.position);

        var path = PathManager.Instance.GetPath(start.transform.position, end.transform.position);
        if (path.Count > 1) path.RemoveAt(0);
        _guardian.SetPath(path);
    }

    public override void Execute()
    {
        bool moving = _guardian.MoveAlongPath(_guardian.PersuitSpeed);

        if (!moving)
        {
            _fsm.ChangeState(NPCState.Idle);
            return;
        }

        if (_guardian.Target && _guardian.FOVV.InFOV(_guardian.Target.position))
        {
            _fsm.ChangeState(NPCState.Persuit);
        }
    }
}