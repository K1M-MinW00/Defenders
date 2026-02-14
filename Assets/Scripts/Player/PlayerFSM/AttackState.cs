using UnityEngine;

public class AttackState : IPlayerState
{
    private PlayerCharacter owner;
    private PlayerFSM fsm;

    public AttackState(PlayerCharacter owner, PlayerFSM fsm)
    {
        this.owner = owner;
        this.fsm = fsm;
    }

    public void Enter()
    {
        owner.agent.isStopped = true;
    }

    public void Update()
    {
        if(!owner.HasValidTarget())
        {
            if(!owner.SearchTarget(forceRefresh : true))
            {
                fsm.ChangeState(owner.idleState);
                return;
            }
        }

        if(!owner.IsTargetInRange(owner.target))
        {
            fsm.ChangeState(owner.moveState);
            return;
        }

        owner.attackBehavior.TryAttack(owner.target);
    }

    public void Exit() { }
}
