using UnityEngine;

public class LiderGoToClick : State<NPCState>
{
    Lider _lider;
    float _distanceStopOffset = 0.5f;

    public LiderGoToClick(FSM<NPCState> fsm, Lider lider) : base(fsm)
    {
        _lider = lider;
    }

    public override void Enter()
    {
        var path = PathManagerExamen.Instance.GetPath(
            _lider.transform.position,
            _lider.ClickPosition
        );

        if (path == null || path.Count == 0)
        {
            _fsm.ChangeState(NPCState.Idle);
            return;
        }

        if (path.Count > 1)
            path.RemoveAt(0);

        _lider.SetPath(path);
    }

    /*public override void Execute()
    {
        _lider.FindTargetInFOV();

        if (_lider.EnemyInFOV())
        {
            _fsm.ChangeState(NPCState.Persuit);
            return;
        }

        if (_lider.PathIndex >= _lider.Path.Count)
        {
            Finish();
            return;
        }

        Vector3 dir = _lider.Path[_lider.PathIndex].transform.position - _lider.transform.position;

        if (dir.sqrMagnitude <= _distanceStopOffset * _distanceStopOffset)
        {
            _lider.NextPooint();

            if (_lider.PathIndex >= _lider.Path.Count)
            {
                Finish();
                return;
            }
        }

        _lider.transform.position += dir.normalized * _followSpeed * Time.deltaTime;
        _lider.transform.forward = dir;
    }*/

    public override void Execute()
    {
        _lider.FindTargetInFOV();

        if (_lider.EnemyInFOV())
        {
            _fsm.ChangeState(NPCState.Persuit);
            return;
        }

        if (_lider.PathIndex >= _lider.Path.Count)
        {
            Finish();
            return;
        }

        Vector3 targetPos = _lider.Path[_lider.PathIndex].transform.position;
        Vector3 toTarget = targetPos - _lider.transform.position;

        if (toTarget.sqrMagnitude <= _distanceStopOffset * _distanceStopOffset)
        {
            _lider.NextPooint();
            return;
        }

        bool last = _lider.PathIndex == _lider.Path.Count - 1;

        if (last)
        {
            _lider.AddForce(_lider.Arrive(targetPos));
        }
        else
        {
            _lider.AddForce(_lider.Seek(targetPos));
        }
            
        _lider.Move();
    }

    void Finish()
    {
        _lider.ClearClick();
        _fsm.ChangeState(NPCState.Idle);
    }
}