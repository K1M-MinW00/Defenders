public class SkillState : IState
{
    private UnitController owner;
    private UnitFSM fsm;

    private bool isWaitingForTarget;

    public SkillState(UnitController owner, UnitFSM fsm)
    {
        this.owner = owner;
        this.fsm = fsm;
    }

    public void Enter()
    {
        isWaitingForTarget = false;

        owner.Movement.Stop();
        owner.Combat.CancelAttack();

        bool prepared = owner.SkillController.TryPrepareSkill();

        if (prepared)
        {
            owner.SkillController.StartSkill();
            return;
        }

        if (owner.SkillController.ShouldWaitForTarget())
        {
            isWaitingForTarget = true;
            owner.Animation.PlayIdle();
            return;
        }

        owner.FSMController.ChangeToIdle();
    }

    public void Update()
    {
        if (owner.IsDead)
            return;

        if (owner.SkillController.IsSkillRunning)
            return;

        if(isWaitingForTarget)
        {
            bool prepared = owner.SkillController.TryPrepareSkill();
            if (prepared)
            {
                owner.SkillController.StartSkill();
                isWaitingForTarget = false;
                return;
            }

            if(!owner.Energy.IsFull)
            {
                isWaitingForTarget = false;
                owner.FSMController.ChangeToIdle();
                return;
            }
            return;
        }

        owner.FSMController.ChangeToIdle();
    }

    public void Exit() 
    {
        isWaitingForTarget = false;
    }
}