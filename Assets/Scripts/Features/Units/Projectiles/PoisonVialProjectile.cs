using UnityEngine;

[RequireComponent(typeof(Poolable))]
public sealed class PoisonVialProjectile : MonoBehaviour, IPoolable
{
    private Poolable poolable;
    private UnitController source;
    private ICombatTarget target;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float damage;
    private float flightDuration;
    private float arcHeight;
    private float hitRadius;
    private float elapsed;
    private bool isActive;

    private void Awake() => poolable = GetComponent<Poolable>();

    public void Initialize(
        UnitController source,
        ICombatTarget target,
        Vector3 targetPosition,
        float damage,
        float flightDuration,
        float arcHeight,
        float hitRadius)
    {
        this.source = source;
        this.target = target;
        startPosition = transform.position;
        this.targetPosition = targetPosition;
        this.damage = Mathf.Max(0f, damage);
        this.flightDuration = Mathf.Max(0.05f, flightDuration);
        this.arcHeight = Mathf.Max(0f, arcHeight);
        this.hitRadius = Mathf.Max(0.05f, hitRadius);
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

        ResolveImpact();
        ReturnToPool();
    }

    private void ResolveImpact()
    {
        if (!CombatTargetSelector.IsValid(target) || target.CombatHealth == null)
            return;

        if (Vector2.Distance(target.TargetTransform.position, targetPosition) > hitRadius)
            return;

        UnitBasicAttackHitResolver.Apply(source, target, target.CombatHealth, damage);
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
        target = null;
        startPosition = Vector3.zero;
        targetPosition = Vector3.zero;
        damage = 0f;
        flightDuration = 0f;
        arcHeight = 0f;
        hitRadius = 0f;
        elapsed = 0f;
    }
}
