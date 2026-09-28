using UnityEngine;

public class MoveState : IState
{
    private UnitController owner;
    public MoveState(UnitController owner)
    {
        this.owner = owner;
    }

    public void Enter()
    {
        owner.Animation.PlayMove();
        owner.Movement.Resume();
    }

    public void Update()
    {
        if (owner.IsDead)
            return;

        if (owner.FSMController.TryChangeToSkill())
            return;

        if (!owner.FSMController.TryEnsureTarget())
        {
            owner.FSMController.ChangeToIdle();
            return;
        }

        if (owner.Targeting.IsTargetInRange())
        {
            owner.FSMController.ChangeToAttack();
            return;
        }

        owner.MoveToCurrentTarget();
    }

    public void Exit()
    {
        owner.Movement.Stop();
    }
}
