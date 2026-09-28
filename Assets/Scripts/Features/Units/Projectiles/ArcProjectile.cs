using UnityEngine;

public class ArcProjectile : MonoBehaviour, IPoolable
{
    [SerializeField] private int hitBufferSize = 32;

    private Poolable poolable;
    private Collider2D[] hitBuffer;
    private ContactFilter2D hitFilter;
    private readonly System.Collections.Generic.HashSet<ICombatHealth> damagedTargets = new();
    private Vector3 startPos;
    private Vector3 endPos;

    private DamageRequest damageRequest;
    private float flightTime;
    private float arcHeight;

    private float splashRadius;
    private LayerMask targetLayer;

    private float timer;
    private bool isActive;

    private void Awake()
    {
        poolable = GetComponent<Poolable>();
        if (poolable == null)
            poolable = gameObject.AddComponent<Poolable>();

        hitBuffer = new Collider2D[hitBufferSize];
        hitFilter = new ContactFilter2D
        {
            useLayerMask = true,
            useTriggers = true
        };
    }

    public void Initialize(Vector3 target, float damage, float flightTime, float arcHeight, float splashRadius, LayerMask targetLayer, ICombatTarget source)
    {
        this.startPos = transform.position;
        this.endPos = target;

        damageRequest = new DamageRequest(damage, source, DamageOrigin.Skill);
        this.flightTime = Mathf.Max(0.05f, flightTime);
        this.arcHeight = arcHeight;

        this.splashRadius = splashRadius;
        this.targetLayer = targetLayer;
        hitFilter.SetLayerMask(targetLayer);

        timer = 0f;
        damagedTargets.Clear();
        isActive = true;
    }

    private void Update()
    {
        if (!isActive)
            return;

        timer += Time.deltaTime;
        float u = Mathf.Clamp01(timer / flightTime);

        // 기본 선형 보간
        Vector3 pos = Vector3.Lerp(startPos, endPos, u);

        // 포물선 높이(중앙에서 최대)
        float height = arcHeight * 4f * u * (1f - u);
        pos.y += height;

        transform.position = pos;

        if (u >= 1f)
        {
            Impact();
            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        if (!isActive)
            return;

        poolable.ReturnToPool();
    }

    public void OnSpawn()
    {
        isActive = false;
    }

    public void OnDespawn()
    {
        isActive = false;
        startPos = Vector3.zero;
        endPos = Vector3.zero;
        damageRequest = default;
        flightTime = 0f;
        arcHeight = 0f;
        splashRadius = 0f;
        targetLayer = 0;
        timer = 0f;
        damagedTargets.Clear();
    }

    private void Impact()
    {
        int hitCount = Physics2D.OverlapCircle(
            endPos,
            splashRadius,
            hitFilter,
            hitBuffer);

        damagedTargets.Clear();
        for (int i = 0; i < hitCount; i++)
        {
            if (!ProjectileHitResolver.TryResolve(
                    hitBuffer[i],
                    targetLayer,
                    damagedTargets,
                    out ICombatHealth combatHealth))
            {
                continue;
            }

            combatHealth.ApplyDamage(damageRequest);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (splashRadius > 0f)
            Gizmos.DrawWireSphere(endPos, splashRadius);
    }
#endif
}
