public interface IState<T>
{
    void Enter();
    void Execute();
    void FixedExecute();
    void Exit();

    void AddTransition(T input, IState<T> state);
    bool GetTransition(T input, out IState<T> state);
}
