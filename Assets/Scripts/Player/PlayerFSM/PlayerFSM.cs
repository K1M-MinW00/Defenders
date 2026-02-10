public class PlayerFSM
{
    private IPlayerState currentState;

    public void ChangeState(IPlayerState newState)
    {
        if (currentState == newState)
            return;

        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }

    public void Update()
    {
        currentState?.Update();
    }
}
