using UnityEngine;

public sealed class MonsterFSMController : MonoBehaviour
{
    private MonsterController owner;
    private StateMachine stateMachine;
    private MonsterIdleState idleState;
    private MonsterMoveState moveState;
    private MonsterAttackState attackState;

    public string CurrentStateName => stateMachine?.CurrentState?.GetType().Name ?? "None";

    public void Initialize(MonsterController owner)
    {
        this.owner = owner;
        stateMachine = new StateMachine();
        idleState = new MonsterIdleState(owner);
        moveState = new MonsterMoveState(owner);
        attackState = new MonsterAttackState(owner);
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

    private bool CanRunBehavior()
    {
        return owner != null && owner.CanRunBehavior;
    }
}
