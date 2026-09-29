using System.Collections.Generic;
using UnityEngine;

public abstract class MeleeUnitAttack : MonoBehaviour, IUnitAttack
{
    [Header("References")]
    protected UnitController owner;
    protected ICombatTarget currentTarget => attackLifecycle.Target;

    [Header("Combat")]
    [SerializeField] protected LayerMask targetLayer;
    [SerializeField] protected int hitBufferSize = 32;

    [Header("Target")]
    [SerializeField] protected int multiTargetUnlockStar = 3;

    protected Collider2D[] hitBuffer;
    protected ContactFilter2D hitFilter;
    protected readonly HashSet<ICombatHealth> damagedTargets = new();
    protected readonly AttackCooldown attackCooldown = new();
    protected readonly AttackLifecycle attackLifecycle = new();
    private Vector2 lockedAttackDirection;

    protected float Damage => owner.Attack;
    protected float Cooldown => 1f / owner.AttackPerSec;

    protected bool isAttacking => attackLifecycle.IsActive;

    protected TargetSelectionMode CurrentTargetMode =>
        owner != null && owner.Star >= multiTargetUnlockStar
        ? TargetSelectionMode.Multi
        : TargetSelectionMode.Single;

    public bool IsAttacking => isAttacking;
    public AttackPhase Phase => attackLifecycle.Phase;

    protected virtual void Awake()
    {
        if (owner == null)
            owner = GetComponent<UnitController>();

        hitBuffer = new Collider2D[hitBufferSize];

        hitFilter = new ContactFilter2D();
        hitFilter.useLayerMask = true;
        hitFilter.SetLayerMask(targetLayer);
        hitFilter.useTriggers = true;
    }

    public virtual bool CanAttack()
    {
        if (owner == null || owner.IsDead)
            return false;

        if (isAttacking)
            return false;

        if (!attackCooldown.IsReady(Time.time))
            return false;

        return true;
    }

    public virtual bool TryAttack(ICombatTarget target)
    {
        if (!CombatTargetSelector.IsValid(target))
            return false;

        if (!CanAttack())
            return false;

        if (!attackLifecycle.TryBegin(target))
            return false;

        lockedAttackDirection = ((Vector2)target.TargetTransform.position - (Vector2)transform.position).normalized;

        attackCooldown.Start(Time.time, Cooldown);

        owner.SkillController.NotifyAttackStarted(target);
        owner.FaceTarget();
        owner.Animation.PlayAttack();

        return true;
    }

    public abstract void OnAttackHit();

    protected bool TryEnterHitPhase()
    {
        if (!CombatTargetSelector.IsValid(currentTarget) && CurrentTargetMode == TargetSelectionMode.Single)
        {
            if (!owner.Targeting.TryFindTargetInSensor())
            {
                CancelAttack();
                return false;
            }

            ICombatTarget replacement = owner.Targeting.CurrentTarget;
            if (!attackLifecycle.TryReplaceTarget(replacement))
                return false;

            lockedAttackDirection = ((Vector2)replacement.TargetTransform.position - (Vector2)transform.position).normalized;
            owner.Animation.FaceTarget(replacement);
        }

        if (!attackLifecycle.TryEnterHitPhase())
            return false;

        GameAudioManager.Instance?.PlayCharacterSfx(
            owner.UnitData?.attackSound,
            GameAudioCue.UnitAttack,
            GameAudioPriority.Normal,
            0.08f,
            owner);
        return true;
    }

    public virtual void OnAttackFinished()
    {
        if (!attackLifecycle.Complete())
            return;

        damagedTargets.Clear();
    }

    public void CancelAttack()
    {
        if (!attackLifecycle.Cancel())
            return;

        damagedTargets.Clear();
    }

    protected virtual void ApplyDamage(ICombatTarget target)
    {
        if (!CombatTargetSelector.IsValid(target))
            return;

        float damage = Damage;
        owner.SkillController.NotifyAttackHit(target, ref damage);
        target.CombatHealth.ApplyDamage(
            new DamageRequest(damage, owner, DamageOrigin.BasicAttack));
    }

    protected virtual void ApplyDamage(Collider2D[] hits,int hitCount)
    {
        if (hits == null || hits.Length == 0)
            return;

        damagedTargets.Clear();

        for (int i=0;i< hitCount;i++)
        {
            Collider2D hit = hits[i];

            if (hit == null)
                continue;

            if (!hit.TryGetComponent(out ICombatTarget target) ||
                !CombatTargetSelector.IsValid(target))
                continue;

            ICombatHealth combatHealth = target.CombatHealth;

            if (!damagedTargets.Add(combatHealth))
                continue;

            float damage = Damage;
            owner.SkillController.NotifyAttackHit(target, ref damage);
            combatHealth.ApplyDamage(
                new DamageRequest(damage, owner, DamageOrigin.BasicAttack));
        }
    }

    protected int OverlapBox(Vector2 center, Vector2 size, float angle)
    {
        return Physics2D.OverlapBox(center,size,angle,hitFilter,hitBuffer);
    }

    protected int OverlapCircle(Vector2 center, float radius)
    {
        return Physics2D.OverlapCircle(center, radius, hitFilter, hitBuffer);
    }

    protected void SpawnVFX(GameObject vfxPrefab, Vector2 position, float angle)
    {
        if (owner.PoolManager == null || vfxPrefab == null)
            return;

        Poolable poolable = owner.PoolManager.Spawn(vfxPrefab, position, Quaternion.Euler(0f, 0f, angle), PoolCategory.Effect);

        if (poolable != null && poolable.TryGetComponent(out PooledVfx vfx))
            vfx.Play();
    }

    protected Vector2 GetAttackDirection()
    {
        if (lockedAttackDirection.sqrMagnitude > 0.0001f)
            return lockedAttackDirection;

        return owner.Animation.GetFacingDirection();
    }

}
