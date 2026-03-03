public class PlayerFSM
{
    public IPlayerState CurrentState { get; private set; }
    public void ChangeState(IPlayerState newState)
    {
        if (CurrentState == newState)
            return;

        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }

    public void Update()
    {
        CurrentState?.Update();
    }
}
