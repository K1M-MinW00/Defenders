using UnityEngine;

public class MonsterAttackState : IState
{
    private MonsterController owner;
    private MonsterFSM fsm;

    private float _nextAttackTime;

    public MonsterAttackState(MonsterController owner, MonsterFSM fsm)
    {
        this.owner = owner;
        this.fsm = fsm;
    }

    public void Enter()
    {
        owner.StopMovement();
        owner.PlayIdle();

        _nextAttackTime = Time.time;
    }

    public void Update()
    {
        if(!owner.HasValidTarget())
        {
            if (owner.TryFindClosestAliveUnit())
            {
                fsm.ChangeState(owner.moveState);
                return;
            }
        }

        if(!owner.IsTargetInAttackRange())
        {
            fsm.ChangeState(owner.moveState);
            return;
        }

        if (Time.time < _nextAttackTime)
            return;

        owner.TryAttackCurrentTarget();

        _nextAttackTime = Time.time + owner.AttackCooldown;
    }

    public void Exit() { }

}