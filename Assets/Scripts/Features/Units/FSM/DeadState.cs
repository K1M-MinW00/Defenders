public class DeadState : IState
{
    private UnitController owner;
    private UnitFSM fsm;

    public DeadState(UnitController owner, UnitFSM fsm)
    {
        this.owner = owner;
        this.fsm = fsm;
    }

    public void Enter()
    {
        owner.Movement.Stop();
        owner.Combat.CancelAttack();
        owner.SkillController.CancelSkill();
        owner.Animation.PlayDie();
    }

    public void Exit() { }

    public void Update() { }
}