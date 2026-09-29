using UnityEngine;

public sealed class MonsterMoveState : IState
{
    private MonsterController owner;
    private float _nextRefreshTime;

    public MonsterMoveState(MonsterController owner)
    {
        this.owner = owner;
    }

    public void Enter()
    {
        owner.PlayMove();
        owner.ResumeMovement();

        _nextRefreshTime = Time.time +
            owner.GetInitialUpdateDelay(owner.MoveTargetRefreshInterval);

        owner.MoveToTarget();
    }


    public void Update()
    {
        if (!owner.HasValidTarget())
        {
            if (!owner.TryFindClosestAliveUnit())
            {
                owner.ChangeToIdle();
                return;
            }
            else
            {
                owner.MoveToTarget();
                return;
            }
        }

        if(owner.IsTargetInAttackRange())
        {
            owner.ChangeToAttack();
            return;
        }

        if (Time.time >= _nextRefreshTime)
        {
            _nextRefreshTime = Time.time + owner.MoveTargetRefreshInterval;
            owner.TryFindClosestAliveUnit();
            owner.MoveToTarget();
        }
    }

    public void Exit() 
    {
        owner.StopMovement();
    }


}
