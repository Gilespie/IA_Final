using System.Collections.Generic;

public class State<T> : IState<T>
{
    protected FSM<T> _fsm;
    private Dictionary<T, IState<T>> _transitions = new();

    public State(FSM<T> fsm) => _fsm = fsm;

    public virtual void Enter() { }
    public virtual void Execute() { }
    public virtual void FixedExecute() { }
    public virtual void Exit() { }

    public void AddTransition(T input, IState<T> state) =>
        _transitions.Add(input, state);

    public bool GetTransition(T input, out IState<T> state) =>
        _transitions.TryGetValue(input, out state);
}
