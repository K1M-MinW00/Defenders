using UnityEngine;

public class IdleState : IState
{
    private UnitController owner;
    private UnitFSM fsm;

    public IdleState(UnitController owner, UnitFSM fsm)
    {
        this.owner = owner;
        this.fsm = fsm;
    }

    public void Enter()
    {
        owner.Movement.Stop();
        owner.Animation.PlayIdle();
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

        if (!owner.Targeting.TryFindTargetInSensor())
            return;

        if (owner.Targeting.IsTargetInRange())
            owner.FSMController.ChangeToAttack();
        else
            owner.FSMController.ChangeToMove();
    }

    public void Exit() { }
}
