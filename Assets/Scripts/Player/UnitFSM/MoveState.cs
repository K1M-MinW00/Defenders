using UnityEngine;

public class MoveState : IState
{
    private UnitController owner;
    private UnitFSM fsm;
    private float _nextRefreshTime;

    public MoveState(UnitController owner, UnitFSM fsm)
    {
        this.owner = owner;
        this.fsm = fsm;
    }

    public void Enter()
    {
        owner.PlayMove();
        owner.ResumeMovement();
        _nextRefreshTime = Time.time;
    }

    public void Update()
    {
        if (!owner.HasValidTarget())
        {
            if (!owner.TryFindTargetInSensor())
            {
                fsm.ChangeState(owner.idleState);
                return;
            }
        }

        if (owner.IsTargetInAttackRange())
        {
            fsm.ChangeState(owner.attackState);
            return;
        }

        owner.MoveToCurrentTarget();

        if (Time.time >= _nextRefreshTime)
        {
            _nextRefreshTime = Time.time + owner.TargetRefreshInterval;
            owner.RefreshTargetIfCloserInRange();
        }
    }

    public void Exit()
    {
        owner.StopMovement();
    }
}
