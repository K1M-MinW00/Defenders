public class DeadState : IState
{
    private UnitController owner;
    public DeadState(UnitController owner)
    {
        this.owner = owner;
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
