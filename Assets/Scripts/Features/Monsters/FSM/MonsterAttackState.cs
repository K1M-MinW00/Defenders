using UnityEngine;

public class MonsterAttackState : IState
{
    private MonsterController owner;
    private readonly AttackCooldown attackCooldown = new();

    public MonsterAttackState(MonsterController owner)
    {
        this.owner = owner;
    }

    public void Enter()
    {
        owner.StopMovement();
        owner.PlayIdle();

        attackCooldown.Reset();
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

        if (!attackCooldown.IsReady(Time.time))
            return;

        if (owner.TryAttackCurrentTarget())
            attackCooldown.Start(Time.time, owner.AttackCooldown);
    }

    public void Exit() => owner.CancelAttack();

}
