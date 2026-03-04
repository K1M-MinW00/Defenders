
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

        _nextAcquireTime = Time.time;

        // TODO : 애니메이션 세팅
    }

    public void Update()
    {
        if (Time.time < _nextAcquireTime)
            return;

        _nextAcquireTime = Time.time + interval;

        UnitInstance newTarget = owner.FindClosestAliveUnit();
        if(newTarget != null)
        {
            owner.SetTarget(newTarget);
            fsm.ChangeState(owner.moveState);
        }
    }

    public void Exit()
    {

    }
}