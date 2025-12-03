public interface IState<T>
{
    public void Enter();
    public void Execute();
    public void FixedExecute();
    public void Exit();

    void AddTransition(T input, IState<T> state);
    bool GetTransition(T input, out IState<T> state);
}