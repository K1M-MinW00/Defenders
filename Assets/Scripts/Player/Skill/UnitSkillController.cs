using System;
using UnityEngine;

public class UnitSkillController : MonoBehaviour
{
    private UnitController owner;

    private ActiveSkillBase activeSkill;
    private PassiveSkillBase passiveSkill;

    private SkillExecutionContext currentContext;
    private bool isCombatPhase;
    private bool isSkillRunning;

    public ActiveSkillBase ActiveSkill => activeSkill;
    public PassiveSkillBase PassiveSkill => passiveSkill;
    public bool IsSkillRunning => isSkillRunning;

    public event Action OnSkillStarted;
    public event Action OnSkillApplied;
    public event Action OnSkillEnded;

    public void Initialize(UnitController owner)
    {
        this.owner = owner;

        activeSkill = GetComponent<ActiveSkillBase>();
        passiveSkill = GetComponent<PassiveSkillBase>();

        if (activeSkill == null)
            activeSkill = GetComponentInChildren<ActiveSkillBase>();

        if (passiveSkill == null)
            passiveSkill = GetComponentInChildren<PassiveSkillBase>();

        activeSkill?.Initialize(owner, this);
        passiveSkill?.Initialize(owner);

        owner.Energy.OnEnergyFull += HandleEnergyFull;
    }

    public void SetCombatPhase(bool active)
    {
        isCombatPhase = active;
    }

    private void HandleEnergyFull()
    {
        if (!CanStartSkill())
            return;

        owner.FSMController.ChangeToSkill();
    }

    public bool CanStartSkill()
    {
        if (!isCombatPhase)
            return false;

        if (isSkillRunning)
            return false;

        if (activeSkill == null)
            return false;

        if (owner.IsDead)
            return false;

        if (!owner.CanUseActive)
            return false;

        if (!owner.IsEnergyFull)
            return false;

        return true;
    }
    
    public bool ShouldWaitForTarget()
    {
        return activeSkill.TargetFailPolicy == SkillTargetFailPolicy.WaitUntilFound;
    }

    public bool TryPrepareSkill()
    {
        if (!CanStartSkill())
            return false;

        if (activeSkill.TryBuildContext(out currentContext))
            return true;

        switch (activeSkill.TargetFailPolicy)
        {
            case SkillTargetFailPolicy.WaitUntilFound:
                return false;

            case SkillTargetFailPolicy.CastWithoutTarget:
                currentContext = new SkillExecutionContext();
                currentContext.Initialize(owner);
                currentContext.SetCastPosition(owner.transform.position);
                return true;

            case SkillTargetFailPolicy.CancelAndRefund:
            default:
                currentContext = null;
                return false;
        }
    }

    public void StartSkill()
    {
        if (currentContext == null || !currentContext.IsValid)
            return;

        isSkillRunning = true;

        activeSkill.OnSkillStart(currentContext);
        //passiveSkill?.OnActiveSkillStarted();
        OnSkillStarted?.Invoke();

        owner.Animation.PlaySkill();

        if (currentContext.EnemyTarget != null)
            owner.Animation.FaceTarget(currentContext.EnemyTarget);
    }

    public void ApplySkill()
    {
        if (!isSkillRunning || currentContext == null)
            return;

        owner.Energy.ConsumeAll();

        activeSkill.OnSkillApply(currentContext);
        //passiveSkill?.OnActiveSkillApplied();
        OnSkillApplied?.Invoke();
    }

    public void EndSkill()
    {
        if (!isSkillRunning)
            return;

        activeSkill.OnSkillEnd(currentContext);
        //passiveSkill?.OnActiveSkillEnded();
        OnSkillEnded?.Invoke();

        currentContext = null;
        isSkillRunning = false;
    }

    public void CancelSkill()
    {
        activeSkill?.CancelSkill();
        currentContext = null;
        isSkillRunning = false;
    }

    //public void NotifyBattleStart()
    //{
    //    passiveSkill?.OnBattleStart();
    //}

    //public void NotifyBattleEnd()
    //{
    //    passiveSkill?.OnBattleEnd();
    //}

    //public void NotifyAttackStarted(MonsterController target)
    //{
    //    passiveSkill?.OnAttackStarted(target);
    //}

    //public void NotifyAttackHit(MonsterController target, ref float damage)
    //{
    //    passiveSkill?.OnAttackHit(target, ref damage);
    //}

    //public void NotifyBeforeTakeDamage(ref float damage, MonsterController attacker)
    //{
    //    passiveSkill?.OnBeforeTakeDamage(ref damage, attacker);
    //}

    //public void NotifyAfterTakeDamage(float finalDamage, MonsterController attacker)
    //{
    //    passiveSkill?.OnAfterTakeDamage(finalDamage, attacker);
    //}
}