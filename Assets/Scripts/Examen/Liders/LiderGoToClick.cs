using UnityEngine;

public class LiderGoToClick : State<NPCState>
{
    Lider _l;

    public LiderGoToClick(FSM<NPCState> fsm, Lider l) : base(fsm)
    {
        _l = l;
    }

    public override void Execute()
    {
        // ПРИОРИТЕТ #1: Низкое HP - убегаем
        if (_l.IsLowHP)
        {
            _fsm.ChangeState(NPCState.Salvation);
            return;
        }

        // ПРИОРИТЕТ #2: Если наступаем на врага во время движения - атакуем его
        // (Игровой лидер атакует врага, если наступает на него первым)
        if (_l.FindEnemyInRadius())
        {
            float dist = Vector3.Distance(_l.transform.position, _l.EnemyTarget.position);
            if (dist <= _l.AttackRadius)
            {
                // Наступаем на врага - атакуем
                _l.PersuitTarget();
                return;
            }
            else
            {
                // Враг рядом, но не в радиусе атаки - преследуем
                _fsm.ChangeState(NPCState.Persuit);
                return;
            }
        }

        // ПРИОРИТЕТ #3: Новый клик мыши
        if (_l.Controller != null && _l.Controller.HasNewClick)
        {
            _l.SetPathToClick(_l.ClickPosition);
        }

        // Движемся к точке клика
        if (!_l.MoveByPath(_l.MovSpeed))
        {
            // Если достигли цели, очищаем клик
            if (_l.Controller != null)
                _l.Controller.ClearClick();

            // Если достигли цели и нет нового клика, проверяем врагов
            if (_l.FindEnemyInFOV())
            {
                _fsm.ChangeState(NPCState.Persuit);
            }
            else
            {
                _fsm.ChangeState(NPCState.Idle);
            }
            return;
        }
    }
}
