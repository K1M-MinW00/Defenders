using UnityEngine;

public class AttackState : IState
{
    private UnitController owner;
    private UnitFSM fsm;

    public AttackState(UnitController owner, UnitFSM fsm)
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
