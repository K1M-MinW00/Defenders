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

    public void ResetRuntime()
    {
        fsm?.Reset();
        owner = null;
    }

    public bool TryChangeToSkill()
    {
        if (!CanEnterCombatState())
            return false;

        if (!owner.SkillController.CanStartSkill())
            return false;

        ChangeToSkill();
        return true;
    }

    public bool TryEnsureTarget()
    {
        if (!CanEnterCombatState())
            return false;

        if (owner.Targeting.HasValidTarget())
            return true;

        if (owner.Targeting.TryFindTargetInSensor())
            return true;

        return owner.IsCombatAlerted && owner.Targeting.TryFindGlobalClosestTarget();
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

    public void ChangeToIdle()
    {
        if (owner == null || owner.IsDead)
            return;

        fsm.ChangeState(idleState);
    }

    public void ChangeToMove()
    {
        if (CanEnterCombatState())
            fsm.ChangeState(moveState);
    }

    public void ChangeToAttack()
    {
        if (CanEnterCombatState())
            fsm.ChangeState(attackState);
    }

    public void ChangeToSkill()
    {
        if (CanEnterCombatState())
            fsm.ChangeState(skillState);
    }

    public void ChangeToDead() => fsm.ChangeState(deadState);

    private bool CanEnterCombatState()
    {
        return owner != null && owner.IsCombatPhase && !owner.IsDead;
    }
}
