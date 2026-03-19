using UnityEngine;

public class MonsterAttackState : IState
{
    private MonsterController owner;
    private MonsterFSM fsm;

    private float _nextAttackTime;
    private float _nextCheckTime;
    private float checkInterval = .1f;

    public MonsterAttackState(MonsterController owner, MonsterFSM fsm)
    {
        this.owner = owner;
        this.fsm = fsm;
    }

    public void Enter()
    {
        owner.StopMovement();

        _nextAttackTime = Time.time;
        _nextCheckTime = Time.time;
    }

    public void Update()
    {
        if (Time.time < _nextCheckTime)
            return;

        _nextCheckTime = Time.time + checkInterval;

        if(!owner.IsTargetValid(owner.TargetUnit))
        {
            UnitRuntime newTarget = owner.TargetUnit;
            if (newTarget != null)
            {
                owner.SetTarget(newTarget);
                fsm.ChangeState(owner.moveState);
            }
            else
            {
                fsm.ChangeState(owner.idleState);
            }
            return;
        }

        if(!IsTargetInRange(owner.TargetUnit))
        {
            fsm.ChangeState(owner.moveState);
            return;
        }

        if (Time.time < _nextAttackTime)
            return;

        if (owner.Attack != null)
            owner.Attack.Execute(owner);

        _nextAttackTime = Time.time + owner.AttackCooldown;
    }

    public void Exit() { }

    private bool IsTargetInRange(UnitRuntime target)
    {
        if (!owner.IsTargetValid(target))
            return false;

        float r = owner.AttackRange;
        float d2 = (target.transform.position - owner.transform.position).sqrMagnitude;

        return d2 <= r * r;
    }
}