using UnityEngine;

public class LiderPersuit : State<NPCState>
{
    Lider _lider;

    public LiderPersuit(FSM<NPCState> fsm, Lider lider) : base(fsm)
    {
        _lider = lider;
    }

    public override void Execute()
    {
        if (!_lider.FOVV.InFOV(_lider.EnemyTarget.position))
        {
            if (_lider.HasPath)
                _fsm.ChangeState(NPCState.FollowToClick);
            else
                _fsm.ChangeState(NPCState.Idle);

            return;
        }

        float dist = Vector3.Distance(
            _lider.transform.position,
            _lider.EnemyTarget.position
        );

        if (dist <= _lider.AttackRadius)
        {

            return;
        }

        
    }
}