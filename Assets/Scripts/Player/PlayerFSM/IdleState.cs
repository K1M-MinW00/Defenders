using UnityEngine;

public class IdleState : IState
{
    private PlayerCharacter owner;
    private PlayerFSM fsm;

    private float _nextCheckTime;
    public IdleState(PlayerCharacter owner, PlayerFSM fsm)
    {
        this.owner = owner;
        this.fsm = fsm;
    }

    public void Enter()
    {
        owner.agent.isStopped = true;
        _nextCheckTime = 0f;

        owner.ClearTarget();
    }

    public void Update()
    {
        if(Time.time >= _nextCheckTime)
        {
            _nextCheckTime = Time.time + owner.targetRefreshInterval;

            if(owner.DetectTargetInRange())
            {
                fsm.ChangeState(owner.attackState);
                return;
            }
        }
    }

    public void Exit() { }
}
