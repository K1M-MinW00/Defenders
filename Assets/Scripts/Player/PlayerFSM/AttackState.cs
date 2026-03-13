using UnityEngine;

public class AttackState : IState
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
        owner.agent.ResetPath();

        if (owner.animator != null)
            owner.animator.SetFloat("MoveSpeed", 0f);
    }

    public void Update()
    {
        if (!owner.HasValidTarget())
        {
            if (!owner.TryFindTargetInSensor())
            {
                owner.ClearTarget();
                fsm.ChangeState(owner.idleState);
                return;
            }
        }

        if (!owner.IsTargetInAttackRange())
        {
            fsm.ChangeState(owner.moveState);
            return;
        }

        owner.attackBehavior?.TryAttack(owner.Target);
    }

    public void Exit() { }
}
