public class MoveState : IPlayerState
{
    private PlayerCharacter owner;
    private PlayerFSM fsm;

    public MoveState(PlayerCharacter owner, PlayerFSM fsm)
    {
        this.owner = owner;
        this.fsm = fsm;
    }

    public void Enter()
    {
        owner.agent.isStopped = false;
    }

    public void Update()
    {
        if(!owner.HasValidTarget())
        {
            if(!owner.SearchTarget(forceRefresh: true))
            {
                fsm.ChangeState(owner.idleState);
                return;
            }
        }

        if (owner.IsTargetInRange(owner.target))
        {
            fsm.ChangeState(owner.attackState);
            return;
        }

        owner.agent.SetDestination(owner.target.position);
    }

    public void Exit() { }
}
