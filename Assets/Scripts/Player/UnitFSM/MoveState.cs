using UnityEngine;

public class MoveState : IState
{
    private PlayerCharacter owner;
    private PlayerFSM fsm;
    private float _nextRefreshTime;

    public MoveState(PlayerCharacter owner, PlayerFSM fsm)
    {
        this.owner = owner;
        this.fsm = fsm;
    }

    public void Enter()
    {
        owner.PlayMove();
        owner.ResumeMovement();

        _nextRefreshTime = 0f;
    }

    public void Update()
    {
        if (!owner.HasValidTarget())
        {
            if (!owner.TryFindTargetInSensor())
            {
                owner.ClearTarget();
                fsm.ChangeState(owner.idleState);
                return;
            }
        }

        if (owner.IsTargetInAttackRange())
        {
            fsm.ChangeState(owner.attackState);
            return;
        }

        owner.MoveToCurrentTarget();

        if (Time.time >= _nextRefreshTime)
        {
            _nextRefreshTime = Time.time + owner.TargetRefreshInterval;
            owner.RefreshTargetIfCloserInRange();
        }
    }

    public void Exit()
    {
        owner.StopMovement();
    }
}
