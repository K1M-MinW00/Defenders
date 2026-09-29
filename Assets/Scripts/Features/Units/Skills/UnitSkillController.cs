using System;
using UnityEngine;

public class UnitSkillController : MonoBehaviour
{
    private const float MinimumExecutionTimeout = 0.5f;

    [SerializeField, Min(MinimumExecutionTimeout)]
    private float executionTimeout = 2f;

    private UnitController owner;

    private ActiveSkillBase activeSkill;
    private PassiveSkillBase passiveSkill;

    private readonly SkillExecutionLifecycle lifecycle = new();
    private bool isCombatPhase;
    private float skillStartedAt = float.NegativeInfinity;

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
        if (this.owner != null)
            this.owner.Energy.OnEnergyFull -= HandleEnergyFull;

        this.owner = owner;

        promotion = owner.UserUnit.Promotion;

        activeSkill = GetComponent<ActiveSkillBase>();
        passiveSkill = GetComponent<PassiveSkillBase>();

        activeSkill?.Initialize(owner, this);
        passiveSkill?.Initialize(owner, this);

        owner.Energy.OnEnergyFull += HandleEnergyFull;
    }

    public void Shutdown()
    {
        if (owner != null)
            owner.Energy.OnEnergyFull -= HandleEnergyFull;

        isCombatPhase = false;
        skillStartedAt = float.NegativeInfinity;
        lifecycle.Cancel();
        activeSkill?.CancelSkill();
        activeSkill = null;
        passiveSkill = null;
        owner = null;
        promotion = 0;
        OnSkillStarted = null;
        OnSkillApplied = null;
        OnSkillEnded = null;
        OnSkillCancelled = null;
    }

    private bool IsSkillStageUnlocked(SkillDataSO skillData, int stageIndex)
    {
        return skillData != null && skillData.IsStageUnlocked(promotion, stageIndex);
    }

    public void SetCombatPhase(bool active)
    {
        if (isCombatPhase == active)
            return;

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

        skillStartedAt = Time.time;
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
        if (lifecycle.Phase != SkillExecutionPhase.Running)
            return;

        SkillExecutionContext context = lifecycle.Context;
        if (context == null)
            return;

        if (!activeSkill.TryResolveContext(context) || !activeSkill.CanApply(context))
        {
            CancelSkill();
            return;
        }

        if (!lifecycle.TryApply())
            return;

        owner.Energy.ConsumeAll();
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
        skillStartedAt = float.NegativeInfinity;
        OnSkillEnded?.Invoke();
        NotifyActiveSkillEnded();
    }

    public void CancelSkill()
    {
        bool wasActive = lifecycle.IsActive;
        bool wasRunning = lifecycle.IsRunning;
        lifecycle.Cancel();
        skillStartedAt = float.NegativeInfinity;

        activeSkill?.CancelSkill();

        if (!wasActive)
            return;

        OnSkillCancelled?.Invoke();

        if (wasRunning)
            NotifyActiveSkillEnded();
    }

    public void RecoverInterruptedSkill()
    {
        if (!lifecycle.IsRunning)
            return;

        float timeout = Mathf.Max(MinimumExecutionTimeout, executionTimeout);
        if (Time.time < skillStartedAt + timeout)
            return;

        if (lifecycle.Phase == SkillExecutionPhase.Running)
            ApplySkill();

        if (lifecycle.IsRunning)
            EndSkill();
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
