using UnityEngine;

public class UnitFSMController : MonoBehaviour
{
    private UnitController owner;
    private StateMachine fsm;

    public bool IsIdleState => fsm != null && fsm.CurrentState == idleState;
    private IdleState idleState;
    private MoveState moveState;
    private AttackState attackState;
    private SkillState skillState;
    private DeadState deadState;

    public void Initialize(UnitController owner)
    {
        this.owner = owner;

        fsm = new StateMachine();
        idleState = new IdleState(owner);
        moveState = new MoveState(owner);
        attackState = new AttackState(owner);
        skillState = new SkillState(owner);
        deadState = new DeadState(owner);
    }

    public void Tick()
    {
        fsm?.Tick();
    }

    public bool TryChangeToSkill()
    {
        if (!owner.SkillController.CanStartSkill())
            return false;

        ChangeToSkill();
        return true;
    }

    public bool TryEnsureTarget(bool includeGlobal)
    {
        if (owner.Targeting.HasValidTarget())
            return true;

        if (owner.Targeting.TryFindTargetInSensor())
            return true;

        return includeGlobal && owner.Targeting.FindGlobalAliveMonster();
    }

    public void ChangeToTargetState()
    {
        if (!owner.Targeting.HasValidTarget())
        {
            ChangeToIdle();
            return;
        }

        if (owner.Targeting.IsTargetInRange())
            ChangeToAttack();
        else
            ChangeToMove();
    }

    public void ChangeToIdle() => fsm.ChangeState(idleState);
    public void ChangeToMove() => fsm.ChangeState(moveState);
    public void ChangeToAttack() => fsm.ChangeState(attackState);
    public void ChangeToSkill() => fsm.ChangeState(skillState);
    public void ChangeToDead() => fsm.ChangeState(deadState);
}
