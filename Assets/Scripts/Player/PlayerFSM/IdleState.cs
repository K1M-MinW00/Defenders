using UnityEngine;

public class IdleState : IState
{
    private PlayerCharacter owner;
    private PlayerFSM fsm;
    private float _nextRefreshTime;

    public IdleState(PlayerCharacter owner, PlayerFSM fsm)
    {
        this.owner = owner;
        this.fsm = fsm;
    }

    public void Enter()
    {
        owner.agent.isStopped = true;
        owner.agent.ResetPath();

        owner.ClearTarget();

        if (owner.animator != null)
            owner.animator.SetFloat("MoveSpeed", 0f);
    }

    public void Update()
    {
        if (Time.time < _nextRefreshTime)
            return;

        _nextRefreshTime = Time.time + owner.targetRefreshInterval;

        if (!owner.TryFindTargetInSensor())
            return;

        if (owner.IsTargetInAttackRange())
            fsm.ChangeState(owner.attackState);
        else
            fsm.ChangeState(owner.moveState);
    }

    public void Exit() { }
}
