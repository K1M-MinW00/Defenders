using UnityEngine;

public class MoveState : IState
{
    private UnitController owner;
    private UnitFSM fsm;

    public MoveState(UnitController owner, UnitFSM fsm)
    {
        this.owner = owner;
        this.fsm = fsm;
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

        if(owner.SkillController.CanStartSkill())
        {
            owner.FSMController.ChangeToSkill();
            return;
        }

        if (!owner.Targeting.HasValidTarget())
        {
            if (!owner.Targeting.TryFindTargetInSensor())
            {
                owner.FSMController.ChangeToIdle();
                return;
            }
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
