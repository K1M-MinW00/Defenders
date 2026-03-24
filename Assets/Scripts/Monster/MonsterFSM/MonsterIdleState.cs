
using UnityEngine;

public class MonsterIdleState : IState
{
    private MonsterController owner;
    private MonsterFSM fsm;

    private float _nextAcquireTime;
    private float interval = .5f;

    public MonsterIdleState(MonsterController owner, MonsterFSM fsm)
    {
        this.owner = owner;
        this.fsm = fsm;
    }

    public void Enter()
    {
        owner.StopMovement();
        owner.ClearTarget();
        owner.PlayIdle();

        _nextAcquireTime = Time.time;
    }

    public void Update()
    {
        if (Time.time < _nextAcquireTime)
            return;

        _nextAcquireTime = Time.time + interval;

        if (owner.TryFindClosestAliveUnit())
            fsm.ChangeState(owner.moveState);
        
    }

    public void Exit() { }
}