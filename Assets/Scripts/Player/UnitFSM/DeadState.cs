public class DeadState : IState
{
    private UnitController owner;
    private UnitFSM fsm;

    public DeadState(UnitController owner, UnitFSM fsm)
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