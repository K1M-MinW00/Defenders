using System.Net;
using UnityEngine;

public class AttackState : IState
{
    private PlayerCharacter owner;
    private PlayerFSM fsm;

    private float _nextCheckTime;
    private float _nextAttackTime;
    private float checkInterval = .1f;

    public AttackState(PlayerCharacter owner, PlayerFSM fsm)
    {
        this.owner = owner;
        this.fsm = fsm;
    }

    public void Enter()
    {
        owner.agent.isStopped = true;

        _nextCheckTime = Time.time;
        _nextAttackTime = Time.time;
    }

    public void Update()
    {
        if (Time.time >= _nextCheckTime)
        {
            _nextCheckTime = Time.time + checkInterval;

            Debug.Log(owner.AttackPerSec);
            var newTarget = owner.GetClosestEnemyInRange();

            // 더 이상 사거리 안에 몬스터가 존재하지 않으면
            if (newTarget == null)
            {
                if (owner.HasValidTarget()) // 기존 타겟이 유효하면 따라가기
                    fsm.ChangeState(owner.moveState);
                else // 대기
                {
                    owner.ClearTarget();
                    fsm.ChangeState(owner.idleState);
                }
                return;
            }
            owner.SetTarget(newTarget);
        }

        if (Time.time < _nextAttackTime)
            return;

        if (!owner.HasValidTarget())
            return;

        owner.attackBehavior?.TryAttack(owner.Target.transform);
        _nextAttackTime = Time.time + (1f / owner.AttackPerSec);
    }

    public void Exit() { }
}
