using UnityEngine;

[RequireComponent(typeof(Poolable))]
public sealed class MagmaMawMagmaBallProjectile : MonoBehaviour, IPoolable
{
    [SerializeField, Min(0.1f)] private float lifetime = 5f;
    [SerializeField] private MagmaMawImpactEffect impactEffectPrefab;

    private Poolable poolable;
    private MonsterController source;
    private Vector3 targetPosition;
    private float damage;
    private float speed;
    private float radius;
    private float burnDuration;
    private float burnDamagePerTick;
    private bool active;

    private void Awake()
    {
        poolable = GetComponent<Poolable>();
    }

    public void Initialize(
        MonsterController owner,
        Vector3 destination,
        float impactDamage,
        float travelSpeed,
        float impactRadius,
        float appliedBurnDuration,
        float appliedBurnDamagePerTick)
    {
        source = owner;
        targetPosition = destination;
        damage = Mathf.Max(0f, impactDamage);
        speed = Mathf.Max(0.1f, travelSpeed);
        radius = Mathf.Max(0.1f, impactRadius);
        burnDuration = Mathf.Max(0f, appliedBurnDuration);
        burnDamagePerTick = Mathf.Max(0f, appliedBurnDamagePerTick);
        active = true;

        CancelInvoke(nameof(ReturnToPool));
        Invoke(nameof(ReturnToPool), lifetime);
    }

    private void Update()
    {
        if (!active)
            return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            speed * Time.deltaTime);

        if ((transform.position - targetPosition).sqrMagnitude <= 0.0025f)
            Impact();
    }

    private void Impact()
    {
        if (!active)
            return;

        active = false;
        SpawnImpactEffect();

        if (source?.UnitRoster?.Units != null)
        {
            float radiusSquared = radius * radius;
            foreach (UnitController unit in source.UnitRoster.Units)
            {
                if (unit == null || unit.IsDead ||
                    (unit.transform.position - targetPosition).sqrMagnitude > radiusSquared)
                    continue;

                unit.CombatHealth.ApplyDamage(
                    new DamageRequest(damage, source, DamageOrigin.Skill));

                if (!unit.IsDead && burnDuration > 0f && burnDamagePerTick > 0f)
                {
                    unit.StatusEffects?.Apply(StatusEffectApplication.Burn(
                        burnDuration,
                        burnDamagePerTick,
                        source,
                        0.5f));
                }
            }
        }

        ReturnToPool();
    }

    private void SpawnImpactEffect()
    {
        if (impactEffectPrefab == null || source?.PoolManager == null)
            return;

        source.PoolManager.Spawn(
            impactEffectPrefab,
            targetPosition,
            Quaternion.identity,
            PoolCategory.Effect);
    }

    private void ReturnToPool()
    {
        if (!active && !gameObject.activeInHierarchy)
            return;

        active = false;
        poolable?.ReturnToPool();
    }

    public void OnSpawn()
    {
        active = false;
        CancelInvoke(nameof(ReturnToPool));
    }

    public void OnDespawn()
    {
        active = false;
        CancelInvoke(nameof(ReturnToPool));
        source = null;
        targetPosition = default;
        damage = 0f;
        speed = 0f;
        radius = 0f;
        burnDuration = 0f;
        burnDamagePerTick = 0f;
    }
}
