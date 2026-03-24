using UnityEngine;

public sealed class MonsterMoveState : IState
{
    private MonsterController owner;
    private MonsterFSM fsm;

    private float _nextRefreshTime;
    private float interval = .25f;

    public MonsterMoveState(MonsterController owner, MonsterFSM fsm)
    {
        this.owner = owner;
        this.fsm = fsm;
    }

    public void Enter()
    {
        owner.PlayMove();
        owner.ResumeMovement();

        _nextRefreshTime = Time.time;

        owner.MoveToTarget();
    }


    public void Update()
    {
        if (!owner.HasValidTarget())
        {
            if (!owner.TryFindClosestAliveUnit())
            {
                fsm.ChangeState(owner.idleState);
                return;
            }
        }

        if(owner.IsTargetInAttackRange())
        {
            fsm.ChangeState(owner.attackState);
            return;
        }

        if (Time.time >= _nextRefreshTime)
        {
            _nextRefreshTime = Time.time + interval;
            owner.TryFindClosestAliveUnit();
        }
    }

    public void Exit() 
    {
        owner.StopMovement();
    }


}