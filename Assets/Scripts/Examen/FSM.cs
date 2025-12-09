using System.Collections.Generic;

public class FSM<T>
{
    private IState<T> _currentState;

    public IState<T> CurrentState => _currentState;

    public void SetInnitialFSM(IState<T> firstState)
    {
        _currentState = firstState;
        _currentState.Enter();
    }

    public void OnUpdate() => _currentState.Execute();

    public void OnFixedUpdate() => _currentState.FixedExecute();

    public void ChangeState(T input)
    {
        if (_currentState.GetTransition(input, out IState<T> next))
        {
            _currentState.Exit();
            _currentState = next;
            _currentState.Enter();
        }
    }
}
