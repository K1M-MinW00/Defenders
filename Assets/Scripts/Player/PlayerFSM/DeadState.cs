public class DeadState : IState
{
    private PlayerCharacter owner;
    private PlayerFSM fsm;

    public DeadState(PlayerCharacter owner, PlayerFSM fsm)
    {
        this.owner = owner;
        this.fsm = fsm;
    }

    public void Enter()
    {
        owner.StopMovement();
    }

    public void Exit()
    {
    }

    public void Update()
    {
    }
}