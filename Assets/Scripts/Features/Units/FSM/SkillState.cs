public class SkillState : IState
{
    private UnitController owner;
    private bool isWaitingForTarget;

    public SkillState(UnitController owner)
    {
        this.owner = owner;
    }

    public void Enter()
    {
        isWaitingForTarget = false;

        owner.Movement.Stop();
        owner.Combat.CancelAttack();

        bool prepared = owner.SkillController.ExecutionPhase == SkillExecutionPhase.Prepared ||
                        owner.SkillController.TryPrepareSkill();

        if (prepared)
        {
            if (owner.SkillController.StartSkill())
                return;

            owner.FSMController.ChangeToIdle();
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

        if (!owner.SkillController.ValidateRunningSkillTarget())
        {
            if (!isWaitingForTarget)
                owner.FSMController.ChangeToIdle();
            return;
        }

        owner.SkillController.RecoverInterruptedSkill();

        if (owner.SkillController.IsSkillRunning)
            return;

        if(isWaitingForTarget)
        {
            bool prepared = owner.SkillController.TryPrepareSkill();
            if (prepared)
            {
                if (owner.SkillController.StartSkill())
                    isWaitingForTarget = false;
                else
                    owner.FSMController.ChangeToIdle();
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
