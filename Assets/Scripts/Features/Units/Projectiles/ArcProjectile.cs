using UnityEngine;

public class ArcProjectile : MonoBehaviour, IPoolable
{
    private Poolable poolable;
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

        timer = 0f;
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
    }

    private void Impact()
    {
        // 스플래시
        Collider2D[] hits = Physics2D.OverlapCircleAll(endPos, splashRadius, targetLayer);
        for (int i = 0; i < hits.Length; i++)
        {
            ICombatHealth combatHealth = hits[i].GetComponent<ICombatHealth>();

            if (combatHealth == null)
                continue;

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
