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

    public event Action OnSkillStarted;
    public event Action OnSkillApplied;
    public event Action OnSkillEnded;

    private int promotion;
    public int Promotion => promotion;

    public ActiveSkillBase ActiveSkill => activeSkill;
    public PassiveSkillBase PassiveSkill => passiveSkill;
    public bool IsSkillRunning => isSkillRunning;

    public bool HasPassive => promotion >= 1;
    public bool HasActiveUpgrade2 => promotion >= 2;
    public bool HasPassiveUpgrade2 => promotion >= 3;

    public bool HasActiveUpgrade3 => promotion >= 4;

    public void Initialize(UnitController owner)
    {
        this.owner = owner;

        promotion = owner.UserUnit.Promotion;

        activeSkill = GetComponent<ActiveSkillBase>();
        passiveSkill = GetComponent<PassiveSkillBase>();

        activeSkill?.Initialize(owner, this);
        passiveSkill?.Initialize(owner, this);

        owner.Energy.OnEnergyFull += HandleEnergyFull;
    }

    public void SetCombatPhase(bool active)
    {
        isCombatPhase = active;

        if(active)
            NotifyBattleStart();
        else
            NotifyBattleEnd();
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

        if (!owner.Runtime.CanUseActive)
            return false;

        if (!owner.Energy.IsFull)
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
        OnSkillStarted?.Invoke();
        NotifyActiveSkillStarted();

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
        OnSkillApplied?.Invoke();
        NotifyActiveSkillApplied();
    }

    public void EndSkill()
    {
        if (!isSkillRunning)
            return;

        activeSkill.OnSkillEnd(currentContext);
        OnSkillEnded?.Invoke();

        currentContext = null;
        isSkillRunning = false;

        NotifyActiveSkillEnded();
    }

    public void CancelSkill()
    {
        activeSkill?.CancelSkill();
        currentContext = null;
        isSkillRunning = false;
    }

    public void NotifyBattleStart()
    {
        if (HasActiveUpgrade3)
            owner.Energy.Add(50f);

        if (!HasPassive)
            return;

        passiveSkill?.OnBattleStart();
    }

    public void NotifyBattleEnd()
    {
        if (!HasPassive)
            return;

        passiveSkill?.OnBattleEnd();
    }

    public void NotifyAttackStarted(MonsterController target)
    {
        if (!HasPassive)
            return;

        passiveSkill?.OnAttackStarted(target);
    }

    public void NotifyAttackHit(MonsterController target, ref float damage)
    {
        if (!HasPassive)
            return;

        passiveSkill?.OnAttackHit(target, ref damage);
    }

    public void NotifyBeforeTakeDamage(ref float damage)
    {
        if (!HasPassive)
            return;

        passiveSkill?.OnBeforeTakeDamage(ref damage);
    }

    public void NotifyAfterTakeDamage(float finalDamage)
    {
        if (!HasPassive)
            return;

        passiveSkill?.OnAfterTakeDamage(finalDamage);
    }
    public void NotifyActiveSkillStarted()
    {
        if (!HasPassive)
            return;

        passiveSkill?.OnActiveSkillStarted();
    }

    public void NotifyActiveSkillApplied()
    {
        if (!HasPassive)
            return;

        passiveSkill?.OnActiveSkillApplied();
    }

    public void NotifyActiveSkillEnded()
    {
        if (!HasPassive)
            return;

        passiveSkill?.OnActiveSkillEnded();
    }
}