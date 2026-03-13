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
        owner.agent.isStopped = false;
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

        owner.FaceTo(owner.Target.transform.position);
        owner.agent.SetDestination(owner.Target.transform.position);

        if (owner.animator != null)
            owner.animator.SetFloat("MoveSpeed", owner.agent.velocity.magnitude);

        if (Time.time >= _nextRefreshTime)
        {
            _nextRefreshTime = Time.time + owner.targetRefreshInterval;

            MonsterController closest = owner.GetClosestEnemyInRange();

            if (closest != null && closest != owner.Target)
            {
                float currentDist = (owner.Target.transform.position - owner.transform.position).sqrMagnitude;
                float newDist = (closest.transform.position - owner.transform.position).sqrMagnitude;

                if (newDist < currentDist)
                    owner.SetTarget(closest);
            }
        }
    }

    public void Exit()
    {
        if (owner.animator != null)
            owner.animator.SetFloat("MoveSpeed", 0f);
    }
}
