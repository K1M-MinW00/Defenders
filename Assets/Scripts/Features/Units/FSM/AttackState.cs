using UnityEngine;

public class AttackState : IState
{
    private UnitController owner;
    private float lastRefreshTime;

    public AttackState(UnitController owner)
    {
        this.owner = owner;
    }

    public void Enter()
    {
        owner.Movement.Stop();
        owner.Animation.PlayIdle();

        lastRefreshTime = -999f;
    }

    public void Update()
    {
        if (owner.IsDead)
            return;


        if (owner.FSMController.TryChangeToSkill())
            return;

        if (!owner.FSMController.TryEnsureTarget(includeGlobal: true))
        {
            owner.FSMController.ChangeToIdle();
            return;
        }

        if(Time.time - lastRefreshTime >= owner.Combat.TargetRefreshInterval)
        {
            owner.Targeting.RefreshTargetIfCloserInRange();
            lastRefreshTime = Time.time;
        }

        if (!owner.Targeting.IsTargetInRange())
        {
            owner.FSMController.ChangeToMove();
            return;
        }

        owner.Combat.TryAttackCurrentTarget();
    }

    public void Exit()
    {
        owner.Combat.CancelAttack();
    }
}
