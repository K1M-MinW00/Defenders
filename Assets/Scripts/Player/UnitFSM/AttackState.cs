using UnityEngine;

public class AttackState : IState
{
    private UnitController owner;
    private UnitFSM fsm;

    private float lastRefreshTime;

    public AttackState(UnitController owner, UnitFSM fsm)
    {
        this.owner = owner;
        this.fsm = fsm;
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


        if(owner.SkillController.CanStartSkill())
        {
            owner.FSMController.ChangeToSkill();
            return;
        }

        if (!owner.Targeting.HasValidTarget())
        {
            bool found = owner.Targeting.TryFindTargetInSensor();
            if (!found)
                found = owner.Targeting.FindGlobalAliveMonster();
            
            if(!found)
            {
                owner.FSMController.ChangeToIdle();
                return;
            }
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
