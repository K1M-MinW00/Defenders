using UnityEngine;

public sealed class MonsterMoveState : IState
{
    private MonsterController owner;
    private MonsterFSM fsm;

    private float _nextTickTime;
    private float interval = .25f;

    public MonsterMoveState(MonsterController owner, MonsterFSM fsm)
    {
        this.owner = owner;
        this.fsm = fsm;
    }

    public void Enter()
    {
        _nextTickTime = Time.time;

        if (owner.IsTargetValid(owner.TargetUnit))
        {
            Vector3 destination = owner.TargetUnit.transform.position;
            owner.MoveTo(destination);
        }
        else
        {
            fsm.ChangeState(owner.idleState);
        }
    }


    public void Update()
    {
        if (Time.time < _nextTickTime)
            return;

        _nextTickTime = Time.time + interval;

        if (!owner.IsTargetValid(owner.TargetUnit))
        {
            UnitInstance newTarget = owner.FindClosestAliveUnit();

            if (newTarget != null)
            {
                owner.SetTarget(newTarget);
                owner.MoveTo(owner.TargetUnit.transform.position);
            }
            else
            {
                fsm.ChangeState(owner.idleState);
                return;
            }
        }

        UnitInstance candidate = owner.FindClosestAliveUnit();
        if (candidate != null && candidate != owner.TargetUnit)
        {
            owner.SetTarget(candidate);
            owner.MoveTo(candidate.transform.position);
        }

        if (IsTargetInRange(owner.TargetUnit))
        {
            fsm.ChangeState(owner.attackState);
            return;
        }

    }

    public void Exit() { }

    private bool IsTargetInRange(UnitInstance target)
    {
        if (!owner.IsTargetValid(target))
            return false;

        float r = owner.AttackRange;
        float d2 = (target.transform.position - owner.transform.position).sqrMagnitude;

        return d2 <= r * r;
    }
}