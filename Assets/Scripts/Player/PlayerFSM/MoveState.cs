using System;
using UnityEngine;
using UnityEngine.Rendering;

public class MoveState : IPlayerState
{
    private PlayerCharacter owner;
    private PlayerFSM fsm;

    private Vector3 destination;
    
    public MoveState(PlayerCharacter owner, PlayerFSM fsm)
    {
        this.owner = owner;
        this.fsm = fsm;
    }

    public void Enter()
    {
        owner.agent.isStopped = false;

        if (owner.HasValidTarget())
            destination = owner.target.position;
        else
            destination = owner.transform.position;

        owner.agent.SetDestination(destination);
    }

    public void Update()
    {
        Transform inRange = owner.GetClosestEnemyInRange();

        if(inRange != null) // 이동 중 사거리 안에 몬스터가 들어오면 즉시 그 몬스터를 타겟으로 Attack
        {
            owner.SetTarget(inRange);
            fsm.ChangeState(owner.attackState);
            return;
        }

        if(!HasArrived())
            return;

        Transform atArrival = owner.GetClosestEnemyInRange();
        if (atArrival != null)
        {
            owner.SetTarget(atArrival);
            fsm.ChangeState(owner.attackState);
        }
        else
        {
            owner.ClearTarget();
            fsm.ChangeState(owner.idleState);
        }
    }

    private bool HasArrived()
    {
        if (owner.agent.pathPending)
            return false;

        // remainingDistance가 Infinity가 나오는 케이스 방지
        if (float.IsInfinity(owner.agent.remainingDistance)) 
            return true;

        return owner.agent.remainingDistance <= owner.agent.stoppingDistance;
    }

    public void Exit() { }
}
