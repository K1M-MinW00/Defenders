using System.Net;
using UnityEngine;

public class AttackState : IPlayerState
{
    private PlayerCharacter owner;
    private PlayerFSM fsm;

    public AttackState(PlayerCharacter owner, PlayerFSM fsm)
    {
        this.owner = owner;
        this.fsm = fsm;
    }

    public void Enter()
    {
        owner.agent.isStopped = true;
    }

    public void Update()
    {
        if(owner.HasValidTarget())
        {
            if (owner.IsTargetInRange(owner.target)) // 1. 타겟 유효 + 사거리 안이면 타겟 공격
            {
                owner.attackBehavior.TryAttack(owner.target);
                return;
            }
            else // 2. 타겟은 유효하지만 사거리 밖에 있을 때
            {
                Transform inRange = owner.GetClosestEnemyInRange();

                if (inRange != null) // 2-1. 사거리 안에 다른 몬스터가 있으면 타겟으로 설정하고 Attack 유지
                {
                    owner.SetTarget(inRange);
                    return;
                }
                else // 2-2. 사거리 안에 다른 몬스터가 없어, 기존의 타겟을 유지한 채 따라가기 위해 Move
                {
                    fsm.ChangeState(owner.moveState);
                    return;
                }
            }
        }

        // 3. 기존 타겟이 무효(null / 죽음)인 경우
        Transform candidate = owner.GetClosestEnemyInRange();
        if(candidate != null)
        {
            owner.SetTarget(candidate);
            return;
        }
        else // 3-2. 기존 타겟도 없고 사거리 내 몬스터도 없으면 Idle 상태로 돌아가 대기
        {
            owner.ClearTarget();
            fsm.ChangeState(owner.idleState);
        }

    }

    public void Exit() { }
}
