
using UnityEngine;

public class MonsterIdleState : IState
{
    private MonsterController owner;
    private float _nextAcquireTime;
    private float interval = .5f;

    public MonsterIdleState(MonsterController owner)
    {
        this.owner = owner;
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
            owner.ChangeToMove();
        
    }

    public void Exit() { }
}
