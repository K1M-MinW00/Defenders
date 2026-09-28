using System;
using UnityEngine;

public class UnitSkillController : MonoBehaviour
{
    private UnitController owner;

    private ActiveSkillBase activeSkill;
    private PassiveSkillBase passiveSkill;

    private readonly SkillExecutionLifecycle lifecycle = new();
    private bool isCombatPhase;

    public event Action OnSkillStarted;
    public event Action OnSkillApplied;
    public event Action OnSkillEnded;
    public event Action OnSkillCancelled;

    private int promotion;
    public int Promotion => promotion;

    public ActiveSkillBase ActiveSkill => activeSkill;
    public PassiveSkillBase PassiveSkill => passiveSkill;
    public bool IsSkillRunning => lifecycle.IsRunning;

    public bool HasPassive => passiveSkill != null && IsSkillStageUnlocked(owner?.UnitData?.passiveSkill, 0);
    public bool HasActiveUpgrade2 => activeSkill != null && IsSkillStageUnlocked(owner?.UnitData?.activeSkill, 1);
    public bool HasPassiveUpgrade2 => passiveSkill != null && IsSkillStageUnlocked(owner?.UnitData?.passiveSkill, 1);

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

    private bool IsSkillStageUnlocked(SkillDataSO skillData, int stageIndex)
    {
        return skillData != null && skillData.IsStageUnlocked(promotion, stageIndex);
    }

    public void SetCombatPhase(bool active)
    {
        isCombatPhase = active;

        if(active)
            NotifyBattleStart();
        else
        {
            CancelSkill();
            NotifyBattleEnd();
        }
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

        if (lifecycle.IsActive)
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

        if (activeSkill.TryBuildContext(out SkillExecutionContext context))
            return context != null && context.IsValid && lifecycle.TryPrepare(context);

        switch (activeSkill.TargetFailPolicy)
        {
            case SkillTargetFailPolicy.WaitUntilFound:
                return false;

            case SkillTargetFailPolicy.CastWithoutTarget:
                context = new SkillExecutionContext();
                context.Initialize(owner);
                context.SetCastPosition(owner.transform.position);
                return lifecycle.TryPrepare(context);

            case SkillTargetFailPolicy.CancelAndRefund:
            default:
                return false;
        }
    }

    public void StartSkill()
    {
        SkillExecutionContext context = lifecycle.Context;
        if (context == null || !context.IsValid || !lifecycle.TryStart())
            return;

        activeSkill.OnSkillStart(context);
        GameAudioManager.Instance?.PlayCharacterSfx(owner.UnitData?.activeSkill?.skillSound, GameAudioCue.UnitSkill, GameAudioPriority.High, 0.1f);
        OnSkillStarted?.Invoke();
        NotifyActiveSkillStarted();

        owner.Animation.PlaySkill();

        if (context.EnemyTarget != null)
            owner.Animation.FaceTarget(context.EnemyTarget);
    }

    public void ApplySkill()
    {
        SkillExecutionContext context = lifecycle.Context;
        if (context == null || !lifecycle.TryApply())
            return;

        owner.Energy.ConsumeAll();

        if (!activeSkill.CanApply(context))
            return;

        activeSkill.OnSkillApply(context);
        OnSkillApplied?.Invoke();
        NotifyActiveSkillApplied();
    }

    public void EndSkill()
    {
        SkillExecutionContext context = lifecycle.Context;
        if (!lifecycle.IsRunning || context == null)
            return;

        activeSkill.OnSkillEnd(context);
        lifecycle.Complete();
        OnSkillEnded?.Invoke();
        NotifyActiveSkillEnded();
    }

    public void CancelSkill()
    {
        bool wasRunning = lifecycle.IsRunning;
        if (!lifecycle.Cancel())
            return;

        activeSkill?.CancelSkill();
        OnSkillCancelled?.Invoke();

        if (wasRunning)
            NotifyActiveSkillEnded();
    }

    public void NotifyBattleStart()
    {
        PromotionProgressionSO progression = PromotionProgressionDatabase.Get();
        float startingEnergyPercent = progression?.GetStartingEnergyPercent(promotion) ?? 0f;

        if (startingEnergyPercent > 0f)
            owner.Energy.Add(owner.Energy.MaxEnergy * startingEnergyPercent / 100f);

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

    public void NotifyAttackStarted(ICombatTarget target)
    {
        if (!HasPassive)
            return;

        passiveSkill?.OnAttackStarted(target);
    }

    public void NotifyAttackHit(ICombatTarget target, ref float damage)
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
