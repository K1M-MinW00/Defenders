using UnityEngine;

public class UnitFSMController : MonoBehaviour
{
    [Header("Target Search")]
    [SerializeField, Min(0.02f)] private float idleTargetSearchInterval = 0.1f;

    private UnitController owner;
    private StateMachine fsm;
    private float nextIdleTargetSearchTime;

    public bool IsIdleState => fsm != null && fsm.CurrentState == idleState;
    public string CurrentStateName => fsm?.CurrentState?.GetType().Name ?? "None";
    private IdleState idleState;
    private MoveState moveState;
    private AttackState attackState;
    private SkillState skillState;
    private DeadState deadState;

    public void Initialize(UnitController owner)
    {
        this.owner = owner;

        fsm = new StateMachine();
        idleState = new IdleState(owner);
        moveState = new MoveState(owner);
        attackState = new AttackState(owner);
        skillState = new SkillState(owner);
        deadState = new DeadState(owner);
        nextIdleTargetSearchTime = float.NegativeInfinity;
    }

    public void Tick()
    {
        fsm?.Tick();
    }

    public void ResetRuntime()
    {
        fsm?.Reset();
        owner = null;
    }

    public bool TryChangeToSkill()
    {
        if (!CanEnterCombatState())
            return false;

        if (!owner.SkillController.CanStartSkill())
            return false;

        // 타겟이 필요한 스킬은 유효한 실행 문맥을 확보한 뒤에만 현재 상태를 중단한다.
        // 에너지가 가득 찬 채 타겟을 잃어도 Idle <-> Skill을 반복하며 멈추지 않게 한다.
        if (!owner.SkillController.TryPrepareSkill())
            return false;

        ChangeToSkill();
        return true;
    }

    public void BeginCombat()
    {
        if (owner == null)
            return;

        nextIdleTargetSearchTime = Time.time +
            StaggeredUpdateSchedule.GetInitialDelay(owner.GetInstanceID(), idleTargetSearchInterval);
    }

    public bool TryEnsureTarget(bool forceSearch = false)
    {
        if (!CanEnterCombatState())
            return false;

        if (owner.Targeting.HasValidTarget())
            return true;

        if (!forceSearch && Time.time < nextIdleTargetSearchTime)
            return false;

        nextIdleTargetSearchTime = Time.time + idleTargetSearchInterval;

        if (owner.Targeting.TryFindTargetInSensor())
            return true;

        return owner.IsCombatAlerted && owner.Targeting.TryFindGlobalClosestTarget();
    }

    public void ChangeToTargetState()
    {
        if (!owner.Targeting.HasValidTarget())
        {
            ChangeToIdle();
            return;
        }

        if (owner.Targeting.IsTargetInRange())
            ChangeToAttack();
        else
            ChangeToMove();
    }

    public void ChangeToIdle()
    {
        if (owner == null || owner.IsDead)
            return;

        fsm.ChangeState(idleState);
    }

    public void ChangeToMove()
    {
        if (CanEnterCombatState())
            fsm.ChangeState(moveState);
    }

    public void ChangeToAttack()
    {
        if (CanEnterCombatState())
            fsm.ChangeState(attackState);
    }

    public void ChangeToSkill()
    {
        if (CanEnterCombatState())
            fsm.ChangeState(skillState);
    }

    public void ChangeToDead() => fsm.ChangeState(deadState);

    private bool CanEnterCombatState()
    {
        return owner != null && owner.IsCombatPhase && !owner.IsDead;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        idleTargetSearchInterval = Mathf.Max(0.02f, idleTargetSearchInterval);
    }
#endif
}
