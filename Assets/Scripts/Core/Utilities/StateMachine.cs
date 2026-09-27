public sealed class StateMachine
{
    public IState CurrentState { get; private set; }

    public void ChangeState(IState nextState)
    {
        if (CurrentState == nextState)
            return;

        CurrentState?.Exit();
        CurrentState = nextState;
        CurrentState?.Enter();
    }

    public void Tick()
    {
        CurrentState?.Update();
    }

    public void Reset()
    {
        CurrentState?.Exit();
        CurrentState = null;
    }
}
