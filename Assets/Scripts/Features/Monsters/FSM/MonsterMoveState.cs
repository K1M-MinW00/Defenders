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
        if (!owner.Targeting.HasValidTarget())
        {
            if (!owner.Targeting.TryAcquireClosest(owner.transform.position))
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

        if(owner.Targeting.IsCurrentTargetInRange(owner.transform.position, owner.AttackRange))
        {
            owner.ChangeToAttack();
            return;
        }

        if (Time.time >= _nextRefreshTime)
        {
            _nextRefreshTime = Time.time + owner.MoveTargetRefreshInterval;
            owner.Targeting.TryAcquireClosest(owner.transform.position);
            owner.MoveToTarget();
        }
    }

    public void Exit() 
    {
        owner.StopMovement();
    }


}
