using UnityEngine;

public class IdleState : IState
{
    private UnitController owner;
    private UnitFSM fsm;
    private float _nextRefreshTime;

    public IdleState(UnitController owner, UnitFSM fsm)
    {
        this.owner = owner;
        this.fsm = fsm;
    }

    public void Enter()
    {
        owner.StopMovement();
        owner.ClearTarget();
        owner.PlayIdle();
        _nextRefreshTime = Time.time;
    }

    public void Update()
    {
        if (Time.time < _nextRefreshTime)
            return;

        _nextRefreshTime = Time.time + owner.TargetRefreshInterval;

        if (!owner.TryFindTargetInSensor())
            return;

        if (owner.IsTargetInAttackRange())
            fsm.ChangeState(owner.attackState);
        else
            fsm.ChangeState(owner.moveState);
    }

    public void Exit() { }
}
