public class IdleState : IPlayerState
{
    private PlayerCharacter owner;
    private PlayerFSM fsm;

    public IdleState(PlayerCharacter owner, PlayerFSM fsm)
    {
        this.owner = owner;
        this.fsm = fsm;
    }

    public void Enter()
    {
        owner.agent.isStopped = true;
        owner.ClearTarget();
    }

    public void Update()
    {
        if (owner.AcquireTargetInRange())
        {
            fsm.ChangeState(owner.attackState);
        }
    }

    public void Exit() { }
}
