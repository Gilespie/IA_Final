using UnityEngine;

public class LiderPatrol : State<NPCState>
{
    Lider _l;
    Graph _lastNode;

    public LiderPatrol(FSM<NPCState> fsm, Lider l) : base(fsm)
    {
        _l = l;
    }

    public override void Enter()
    {
        _lastNode = null;
        PickNewPoint();
    }

    void PickNewPoint()
    {
        Graph closest = PathManagerExamen.Instance.Closest(_l.transform.position);
        if (closest == null) return;

        Graph[] allNodes = PathManagerExamen.Instance.AllNodes;
        if (allNodes == null || allNodes.Length == 0) return;

        Graph randomNode;
        int attempts = 0;
        do
        {
            randomNode = allNodes[Random.Range(0, allNodes.Length)];
            attempts++;
        } while (randomNode == _lastNode && allNodes.Length > 1 && attempts < 10);

        if (randomNode != null && randomNode != closest)
        {
            _lastNode = randomNode;
            _l.SetPathToNode(randomNode);
        }
    }

    public override void Execute()
    {
        // ПРИОРИТЕТ #1: Низкое HP важнее всего - убегаем даже если видим врага
        if (_l.CurrentHealth < 30f)
        {
            Debug.Log($"{_l.gameObject.name} HP низкое ({_l.CurrentHealth:F1}), убегает!");
            _fsm.ChangeState(NPCState.Salvation);
            return;
        }

        // ПРИОРИТЕТ #2: Проверяем врагов в радиусе обнаружения
        // Если видим врага - нападаем (только если HP нормальное)
        if (_l.FindEnemyInRadius())
        {
            Debug.Log($"{_l.gameObject.name} ВИДИТ ВРАГА! Переходит из патрулирования в атаку на {_l.EnemyTarget.name}!");
            _fsm.ChangeState(NPCState.Persuit);
            return;
        }

        // Патрулирование
        if (!_l.HasPath)
        {
            // Убеждаемся что velocity обнулен перед выбором новой точки
            _l.Stop();
            PickNewPoint();
        }
        
        if (_l.HasPath)
        {
            if (!_l.MoveByPath(_l.MovSpeed))
            {
                // Путь завершен, выбираем новую точку
            }
        }
    }
}
