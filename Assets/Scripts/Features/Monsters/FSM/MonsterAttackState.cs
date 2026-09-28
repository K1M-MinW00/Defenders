using UnityEngine;

public class MonsterAttackState : IState
{
    private MonsterController owner;
    private float _nextAttackTime;

    public MonsterAttackState(MonsterController owner)
    {
        this.owner = owner;
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
                owner.ChangeToMove();
                return;
            }

            owner.ChangeToIdle();
            return;
        }

        if(!owner.IsTargetInAttackRange())
        {
            owner.ChangeToMove();
            return;
        }

        if (Time.time < _nextAttackTime)
            return;

        owner.TryAttackCurrentTarget();

        _nextAttackTime = Time.time + owner.AttackCooldown;
    }

    public void Exit() => owner.CancelAttack();

}
