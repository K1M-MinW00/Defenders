using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Poolable))]
public sealed class ToxicPoolArea : MonoBehaviour, IPoolable
{
    [SerializeField, Min(1)] private int hitBufferSize = 32;

    [Header("Visual Fade")]
    [SerializeField, Range(0f, 1f)] private float maximumOpacity = 0.45f;
    [SerializeField, Min(0f)] private float fadeInDuration = 0.15f;
    [SerializeField, Min(0f)] private float fadeOutDuration = 0.4f;

    private readonly HashSet<ICombatHealth> damagedTargets = new();
    private Poolable poolable;
    private SpriteRenderer spriteRenderer;
    private Color baseColor;
    private Collider2D[] hitBuffer;
    private ContactFilter2D hitFilter;
    private UnitController source;
    private LayerMask targetLayer;
    private float radius;
    private float damagePerTick;
    private float tickInterval;
    private float remainingDuration;
    private float totalDuration;
    private float timeUntilNextTick;
    private float slowRatio;
    private bool isActive;

    private void Awake()
    {
        poolable = GetComponent<Poolable>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        baseColor = spriteRenderer != null ? spriteRenderer.color : Color.white;
        hitBuffer = new Collider2D[Mathf.Max(1, hitBufferSize)];
        hitFilter = new ContactFilter2D { useLayerMask = true, useTriggers = true };
    }

    public void Initialize(
        UnitController source,
        float radius,
        float damagePerTick,
        float tickInterval,
        float duration,
        float slowRatio,
        LayerMask targetLayer)
    {
        this.source = source;
        this.radius = Mathf.Max(0.05f, radius);
        this.damagePerTick = Mathf.Max(0f, damagePerTick);
        this.tickInterval = Mathf.Max(0.05f, tickInterval);
        totalDuration = Mathf.Max(0.05f, duration);
        remainingDuration = totalDuration;
        timeUntilNextTick = this.tickInterval;
        this.slowRatio = Mathf.Clamp01(slowRatio);
        this.targetLayer = targetLayer;
        hitFilter.SetLayerMask(targetLayer);
        isActive = true;
        SetOpacity(0f);
    }

    private void Update()
    {
        if (!isActive)
            return;

        float elapsed = Time.deltaTime;
        remainingDuration -= elapsed;
        timeUntilNextTick -= elapsed;
        UpdateOpacity();

        while (timeUntilNextTick <= 0f && remainingDuration > 0f)
        {
            ApplyTick();
            timeUntilNextTick += tickInterval;
        }

        if (remainingDuration <= 0f)
            ReturnToPool();
    }

    private void ApplyTick()
    {
        int hitCount = Physics2D.OverlapCircle(transform.position, radius, hitFilter, hitBuffer);
        damagedTargets.Clear();
        for (int index = 0; index < hitCount; index++)
        {
            if (!ProjectileHitResolver.TryResolve(
                    hitBuffer[index],
                    targetLayer,
                    damagedTargets,
                    out ICombatTarget target,
                    out ICombatHealth health))
            {
                continue;
            }

            health.ApplyDamage(new DamageRequest(damagePerTick, source, DamageOrigin.Effect));
            if (!target.IsDead && target is MonsterController monster && slowRatio > 0f)
            {
                monster.ApplyStatusEffect(new StatusEffectApplication(
                    StatusEffectType.Slow,
                    tickInterval + 0.1f,
                    slowRatio,
                    source: source));
            }
        }
    }

    private void ReturnToPool()
    {
        if (!isActive)
            return;

        isActive = false;
        poolable.ReturnToPool();
    }

    private void UpdateOpacity()
    {
        float age = Mathf.Max(0f, totalDuration - remainingDuration);
        float fadeIn = fadeInDuration > 0f ? Mathf.Clamp01(age / fadeInDuration) : 1f;
        float fadeOut = fadeOutDuration > 0f
            ? Mathf.Clamp01(Mathf.Max(0f, remainingDuration) / fadeOutDuration)
            : 1f;
        SetOpacity(maximumOpacity * Mathf.Min(fadeIn, fadeOut));
    }

    private void SetOpacity(float alpha)
    {
        if (spriteRenderer == null)
            return;

        Color color = baseColor;
        color.a = Mathf.Clamp01(alpha);
        spriteRenderer.color = color;
    }

    public void OnSpawn() => ResetRuntime();
    public void OnDespawn() => ResetRuntime();

    private void ResetRuntime()
    {
        isActive = false;
        source = null;
        targetLayer = 0;
        radius = 0f;
        damagePerTick = 0f;
        tickInterval = 0f;
        remainingDuration = 0f;
        totalDuration = 0f;
        timeUntilNextTick = 0f;
        slowRatio = 0f;
        damagedTargets.Clear();
        SetOpacity(0f);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (radius > 0f)
            Gizmos.DrawWireSphere(transform.position, radius);
    }
#endif
}
