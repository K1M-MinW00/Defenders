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
        if(!owner.Targeting.HasValidTarget())
        {
            if (owner.Targeting.TryAcquireClosest(owner.transform.position))
            {
                owner.ChangeToMove();
                return;
            }

            owner.ChangeToIdle();
            return;
        }

        if(!owner.Targeting.IsCurrentTargetInRange(owner.transform.position, owner.AttackRange))
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
