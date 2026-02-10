using UnityEngine;
using UnityEngine.InputSystem.Layouts;

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
        if (owner.target == null)
        {
            fsm.ChangeState(owner.idleState);
            return;
        }

        if (owner.IsTargetInRange(owner.target))
        {
            owner.attackBehavior.TryAttack(owner.target);
            return;
        }

        Transform candidate = owner.FindClosestEnemy();

        if (candidate == owner.target)
            fsm.ChangeState(owner.moveState);

        else
            owner.target = candidate;
    }

    public void Exit() { }
}
