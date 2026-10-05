using UnityEngine;

public abstract class ActiveSkillBase : MonoBehaviour
{
    private readonly SkillExecutionContext reusableContext = new();
    private readonly System.Collections.Generic.List<Coroutine> executionCoroutines = new();
    private readonly System.Collections.Generic.HashSet<Poolable> executionEffects = new();
    private SkillTelegraphView telegraph;

    protected UnitController owner;
    protected UnitSkillController skillController;
    protected int Promotion => owner.UserUnit.Promotion;
    public abstract ActiveSkillTargetType TargetType { get; }
    public virtual SkillTargetFailPolicy TargetFailPolicy => SkillTargetFailPolicy.CancelAndRefund;
    public virtual TargetResolutionPolicy ResolutionPolicy =>
        TargetType == ActiveSkillTargetType.SelfArea
            ? TargetResolutionPolicy.Self
            : TargetResolutionPolicy.RetargetOnResolve;
    public virtual int TargetCount => 1;

    public virtual void Initialize(UnitController owner, UnitSkillController skillController)
    {
        this.owner = owner;
        this.skillController = skillController;
    }

    protected SkillTelegraphView Telegraph =>
        telegraph != null ? telegraph : telegraph = SkillTelegraphView.GetOrCreate(owner.transform);

    internal void HideTelegraph()
    {
        telegraph?.Hide();
    }

    public abstract bool TryBuildContext(out SkillExecutionContext context);

    protected SkillExecutionContext PrepareReusableContext()
    {
        reusableContext.Initialize(owner);
        return reusableContext;
    }

    protected bool TryPrepareEnemyInRangeContext(out SkillExecutionContext context)
    {
        context = PrepareReusableContext();

        ICombatTarget target = owner?.Targeting?.GetClosestEnemyInRange();
        if (!CombatTargetSelector.IsValid(target))
            return false;

        context.SetEnemyTarget(target);
        return true;
    }

    protected bool PrepareSelfAreaContext(
        out SkillExecutionContext context,
        bool includeClosestEnemy = true)
    {
        context = PrepareReusableContext();

        if (includeClosestEnemy)
        {
            ICombatTarget target = owner?.Targeting?.GetClosestEnemyInRange();
            if (CombatTargetSelector.IsValid(target))
                context.SetEnemyTarget(target);
        }

        context.SetCastPosition(owner.transform.position);
        return true;
    }

    protected bool TryPrepareEnemyWithinRangeContext(
        float skillRange,
        out SkillExecutionContext context)
    {
        context = PrepareReusableContext();

        ICombatTarget target = owner?.Targeting?.GetClosestEnemyInRange();
        if (!CombatTargetSelector.IsWithinRange(
                target,
                owner.transform.position,
                Mathf.Max(0f, skillRange)))
        {
            return false;
        }

        context.SetEnemyTarget(target);
        return true;
    }

    protected bool PrepareSelfAreaWithEnemyInRangeContext(
        float effectRadius,
        out SkillExecutionContext context)
    {
        if (!TryPrepareEnemyWithinRangeContext(effectRadius, out context))
            return false;

        // 적 존재 여부는 발동 조건으로만 사용하고 실제 판정 중심은 시전자 위치로 고정한다.
        context.SetCastPosition(owner.transform.position);
        return true;
    }

    protected float ResolveActiveUpgrade(float baseValue, float upgradedValue)
    {
        return skillController != null && skillController.HasActiveUpgrade2
            ? upgradedValue
            : baseValue;
    }

    protected int ResolveActiveUpgrade(int baseValue, int upgradedValue)
    {
        return skillController != null && skillController.HasActiveUpgrade2
            ? upgradedValue
            : baseValue;
    }

    protected Coroutine StartExecutionCoroutine(System.Collections.IEnumerator routine)
    {
        if (routine == null || owner == null)
            return null;

        Coroutine coroutine = owner.StartCoroutine(routine);
        executionCoroutines.Add(coroutine);
        return coroutine;
    }

    protected T TrackExecutionEffect<T>(T effect) where T : Component
    {
        if (effect != null && effect.TryGetComponent(out Poolable poolable))
            executionEffects.Add(poolable);

        return effect;
    }

    protected Poolable TrackExecutionEffect(Poolable effect)
    {
        if (effect != null)
            executionEffects.Add(effect);

        return effect;
    }

    internal void CleanupExecutionResources()
    {
        HideTelegraph();
        if (owner != null)
        {
            for (int i = 0; i < executionCoroutines.Count; i++)
            {
                Coroutine coroutine = executionCoroutines[i];
                if (coroutine != null)
                    owner.StopCoroutine(coroutine);
            }
        }

        executionCoroutines.Clear();

        foreach (Poolable effect in executionEffects)
        {
            if (effect != null && effect.IsSpawned)
                effect.ReturnToPool();
        }

        executionEffects.Clear();
    }

    protected bool TrySpawnSkillObject<T>(
        T prefab,
        Vector3 position,
        Quaternion rotation,
        PoolCategory category,
        out T spawned,
        Transform parent = null) where T : Component
    {
        spawned = null;

        if (prefab == null)
        {
            Debug.LogError($"[{GetType().Name}] Skill prefab is not assigned.", this);
            return false;
        }

        if (owner == null || owner.PoolManager == null)
        {
            Debug.LogError($"[{GetType().Name}] Skill owner or StagePoolManager is not available.", this);
            return false;
        }

        spawned = owner.PoolManager.Spawn(prefab, position, rotation, category, parent);
        if (spawned == null)
            Debug.LogError($"[{GetType().Name}] Failed to spawn '{prefab.name}'.", this);

        return spawned != null;
    }

    protected bool TrySpawnSkillObject(
        GameObject prefab,
        Vector3 position,
        Quaternion rotation,
        PoolCategory category,
        out Poolable spawned,
        Transform parent = null)
    {
        spawned = null;

        if (prefab == null)
        {
            Debug.LogError($"[{GetType().Name}] Skill prefab is not assigned.", this);
            return false;
        }

        if (owner == null || owner.PoolManager == null)
        {
            Debug.LogError($"[{GetType().Name}] Skill owner or StagePoolManager is not available.", this);
            return false;
        }

        spawned = owner.PoolManager.Spawn(prefab, position, rotation, category, parent);
        if (spawned == null)
            Debug.LogError($"[{GetType().Name}] Failed to spawn '{prefab.name}'.", this);

        return spawned != null;
    }

    public virtual bool CanApply(SkillExecutionContext context)
    {
        return SkillTargetPolicy.CanApply(TargetType, ResolutionPolicy, context);
    }

    public virtual bool TryResolveContext(SkillExecutionContext context)
    {
        return SkillTargetPolicy.TryResolve(
            TargetType,
            ResolutionPolicy,
            context,
            owner);
    }

    public virtual void OnSkillStart(SkillExecutionContext context) { }
    public abstract void OnSkillApply(SkillExecutionContext context);
    public virtual void OnSkillEnd(SkillExecutionContext context) { }
    public virtual void CancelSkill() { }
}
