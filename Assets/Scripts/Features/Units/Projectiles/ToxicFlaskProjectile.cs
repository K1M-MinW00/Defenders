using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Poolable))]
public sealed class ToxicFlaskProjectile : MonoBehaviour, IPoolable
{
    [SerializeField, Min(1)] private int hitBufferSize = 32;

    private readonly HashSet<ICombatHealth> damagedTargets = new();
    private Poolable poolable;
    private Collider2D[] hitBuffer;
    private ContactFilter2D hitFilter;
    private UnitController source;
    private StagePoolManager poolManager;
    private ToxicPoolArea poolPrefab;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private LayerMask targetLayer;
    private float impactDamage;
    private float impactRadius;
    private float flightDuration;
    private float arcHeight;
    private float poolDuration;
    private float poolRadius;
    private float poolDamagePerTick;
    private float poolTickInterval;
    private float poolSlowRatio;
    private float elapsed;
    private bool isActive;

    private void Awake()
    {
        poolable = GetComponent<Poolable>();
        hitBuffer = new Collider2D[Mathf.Max(1, hitBufferSize)];
        hitFilter = new ContactFilter2D { useLayerMask = true, useTriggers = true };
    }

    public void Initialize(
        UnitController source,
        Vector3 targetPosition,
        ToxicPoolArea poolPrefab,
        float impactDamage,
        float impactRadius,
        float flightDuration,
        float arcHeight,
        float poolDuration,
        float poolRadius,
        float poolDamagePerTick,
        float poolTickInterval,
        float poolSlowRatio,
        LayerMask targetLayer)
    {
        this.source = source;
        poolManager = source != null ? source.PoolManager : null;
        this.poolPrefab = poolPrefab;
        startPosition = transform.position;
        this.targetPosition = targetPosition;
        this.impactDamage = Mathf.Max(0f, impactDamage);
        this.impactRadius = Mathf.Max(0.05f, impactRadius);
        this.flightDuration = Mathf.Max(0.05f, flightDuration);
        this.arcHeight = Mathf.Max(0f, arcHeight);
        this.poolDuration = Mathf.Max(0.05f, poolDuration);
        this.poolRadius = Mathf.Max(0.05f, poolRadius);
        this.poolDamagePerTick = Mathf.Max(0f, poolDamagePerTick);
        this.poolTickInterval = Mathf.Max(0.05f, poolTickInterval);
        this.poolSlowRatio = Mathf.Clamp01(poolSlowRatio);
        this.targetLayer = targetLayer;
        hitFilter.SetLayerMask(targetLayer);
        elapsed = 0f;
        isActive = true;
    }

    private void Update()
    {
        if (!isActive)
            return;

        elapsed += Time.deltaTime;
        float progress = Mathf.Clamp01(elapsed / flightDuration);
        Vector3 position = Vector3.Lerp(startPosition, targetPosition, progress);
        position.y += arcHeight * 4f * progress * (1f - progress);
        transform.position = position;

        if (progress < 1f)
            return;

        Impact();
        ReturnToPool();
    }

    private void Impact()
    {
        int hitCount = Physics2D.OverlapCircle(targetPosition, impactRadius, hitFilter, hitBuffer);
        damagedTargets.Clear();
        for (int index = 0; index < hitCount; index++)
        {
            if (!ProjectileHitResolver.TryResolve(
                    hitBuffer[index],
                    targetLayer,
                    damagedTargets,
                    out ICombatHealth health))
            {
                continue;
            }

            health.ApplyDamage(new DamageRequest(impactDamage, source, DamageOrigin.Skill));
        }

        if (poolManager == null || poolPrefab == null)
            return;

        ToxicPoolArea pool = poolManager.Spawn(
            poolPrefab,
            targetPosition,
            Quaternion.identity,
            PoolCategory.Effect);
        pool?.Initialize(
            source,
            poolRadius,
            poolDamagePerTick,
            poolTickInterval,
            poolDuration,
            poolSlowRatio,
            targetLayer);
    }

    private void ReturnToPool()
    {
        if (!isActive)
            return;

        isActive = false;
        poolable.ReturnToPool();
    }

    public void OnSpawn() => ResetRuntime();
    public void OnDespawn() => ResetRuntime();

    private void ResetRuntime()
    {
        isActive = false;
        source = null;
        poolManager = null;
        poolPrefab = null;
        startPosition = Vector3.zero;
        targetPosition = Vector3.zero;
        targetLayer = 0;
        impactDamage = 0f;
        impactRadius = 0f;
        flightDuration = 0f;
        arcHeight = 0f;
        poolDuration = 0f;
        poolRadius = 0f;
        poolDamagePerTick = 0f;
        poolTickInterval = 0f;
        poolSlowRatio = 0f;
        elapsed = 0f;
        damagedTargets.Clear();
    }
}
