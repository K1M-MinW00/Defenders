using UnityEngine;

public sealed class MonsterFSMController : MonoBehaviour
{
    private MonsterController owner;
    private StateMachine stateMachine;
    private MonsterIdleState idleState;
    private MonsterMoveState moveState;
    private MonsterAttackState attackState;
    private MonsterAbilityState abilityState;

    public bool IsUsingAbility => stateMachine?.CurrentState == abilityState;

    public string CurrentStateName => stateMachine?.CurrentState?.GetType().Name ?? "None";

    public void Initialize(MonsterController owner)
    {
        this.owner = owner;
        stateMachine = new StateMachine();
        idleState = new MonsterIdleState(owner);
        moveState = new MonsterMoveState(owner);
        attackState = new MonsterAttackState(owner);
        abilityState = new MonsterAbilityState(owner);
    }

    public void Tick()
    {
        if (CanRunBehavior())
            stateMachine.Tick();
    }

    public void ResetRuntime()
    {
        stateMachine?.Reset();
    }

    public void ChangeToIdle()
    {
        if (CanRunBehavior())
            stateMachine.ChangeState(idleState);
    }

    public void ChangeToMove()
    {
        if (CanRunBehavior())
            stateMachine.ChangeState(moveState);
    }

    public void ChangeToAttack()
    {
        if (CanRunBehavior())
            stateMachine.ChangeState(attackState);
    }

    public bool TryUseAbility(IMonsterAbility ability)
    {
        if (!CanRunBehavior() || IsUsingAbility || !abilityState.TryConfigure(ability))
            return false;

        stateMachine.ChangeState(abilityState);
        return true;
    }

    public void CompleteAbility()
    {
        if (IsUsingAbility)
            owner.ResumeBehaviorAfterControlEffect();
    }

    private bool CanRunBehavior()
    {
        return owner != null && owner.CanRunBehavior;
    }
}
