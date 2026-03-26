public class SkillState : IState
{
    private UnitController owner;
    private UnitFSM fsm;

    public SkillState(UnitController owner, UnitFSM fsm)
    {
        this.owner = owner;
        this.fsm = fsm;
    }

    public void Enter()
    {
        owner.CancelAttack();
        owner.PlaySkill();
    }


    public void Update()
    {
        
    }
    public void Exit()
    {

    }
}
