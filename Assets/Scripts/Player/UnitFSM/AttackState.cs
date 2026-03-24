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
        owner.StopMovement();
        owner.PlayIdle();
    }

    public void Update()
    {
        if (!owner.HasValidTarget())
        {
            if (!owner.TryFindTargetInSensor())
            {
                fsm.ChangeState(owner.idleState);
                return;
            }
        }

        if (!owner.IsTargetInAttackRange())
        {
            fsm.ChangeState(owner.moveState);
            return;
        }

        owner.TryAttackCurrentTarget();
    }

    public void Exit()
    {
        owner.CancelAttack();
    }
}
