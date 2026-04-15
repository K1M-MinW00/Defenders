using UnityEngine;

public class UnitFSMController : MonoBehaviour
{
    private UnitController owner;
    private UnitFSM fsm;

    private IdleState idleState;
    private MoveState moveState;
    private AttackState attackState;
    private SkillState skillState;
    private DeadState deadState;

    public void Initialize(UnitController owner)
    {
        this.owner = owner;

        fsm = new UnitFSM();
        idleState = new IdleState(owner, fsm);
        moveState = new MoveState(owner, fsm);
        attackState = new AttackState(owner, fsm);
        skillState = new SkillState(owner, fsm);
        deadState = new DeadState(owner, fsm);
    }

    public void Tick()
    {
        fsm?.Update();
    }

    public void ChangeToIdle() => fsm.ChangeState(idleState);
    public void ChangeToMove() => fsm.ChangeState(moveState);
    public void ChangeToAttack() => fsm.ChangeState(attackState);
    public void ChangeToSkill() => fsm.ChangeState(skillState);
    public void ChangeToDead() => fsm.ChangeState(deadState);
}