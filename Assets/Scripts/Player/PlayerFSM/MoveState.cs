using UnityEngine;

public class MoveState : IState
{
    private PlayerCharacter owner;
    private PlayerFSM fsm;

    private float _nextDestRefreshTime;
    private float interval = .2f;


    public MoveState(PlayerCharacter owner, PlayerFSM fsm)
    {
        this.owner = owner;
        this.fsm = fsm;
    }

    public void Enter()
    {
        owner.agent.isStopped = false;

        if (!owner.HasValidTarget())
        {
            fsm.ChangeState(owner.idleState);
            return;
        }

        Vector3 destination = owner.Target.transform.position;
        owner.agent.SetDestination(destination);

        _nextDestRefreshTime = Time.time + interval;
    }

    public void Update()
    {
        MonsterController inRange = owner.GetClosestEnemyInRange();

        if (inRange != null) // 이동 중 사거리 안에 몬스터가 들어오면 즉시 그 몬스터를 타겟으로 Attack
        {
            owner.SetTarget(inRange);
            fsm.ChangeState(owner.attackState);
            return;
        }

        if (!owner.HasValidTarget())
        {
            owner.ClearTarget();
            fsm.ChangeState(owner.idleState);
            return;
        }

        if (Time.time >= _nextDestRefreshTime)
        {
            owner.agent.SetDestination(owner.Target.transform.position);
            _nextDestRefreshTime = Time.time + interval;
        }

        if (HasArrived())
        {
            owner.ClearTarget();
            fsm.ChangeState(owner.idleState);
            return;
        }
    }

    private bool HasArrived()
    {
        if (owner.agent.pathPending)
            return false;

        if (float.IsInfinity(owner.agent.remainingDistance))
            return true;

        return owner.agent.remainingDistance <= owner.agent.stoppingDistance;
    }

    public void Exit() { }
}
