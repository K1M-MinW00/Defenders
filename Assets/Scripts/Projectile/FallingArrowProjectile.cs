using UnityEngine;

public class ArrowRainFallingArrow : MonoBehaviour
{
    [Header("Fall")]
    [SerializeField] private float fallSpeed = 10f;
    [SerializeField] private GameObject impactEffectPrefab;
    [SerializeField] private float lifeTime = 2f;
    private Vector2 targetPoint;
    private float damage;
    private float hitRadius;
    private LayerMask enemyLayer;
    private bool initialized;

    public void Initialize(Vector2 targetPoint, float damage, float hitRadius, LayerMask enemyLayer)
    {
        this.targetPoint = targetPoint;
        this.damage = damage;
        this.hitRadius = hitRadius;
        this.enemyLayer = enemyLayer;
        initialized = true;

        Vector2 dir = (targetPoint - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // 화살이 아래로 떨어지는 방향을 바라보게
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void Update()
    {
        if (!initialized)
            return;

        Vector2 current = transform.position;
        Vector2 next = Vector2.MoveTowards(current, targetPoint, fallSpeed * Time.deltaTime);
        transform.position = next;

        if ((next - targetPoint).sqrMagnitude <= 0.001f)
        {
            Impact();
        }
    }

    private void Impact()
    {
        if (impactEffectPrefab != null)
            Instantiate(impactEffectPrefab, targetPoint, Quaternion.identity);

        Collider2D[] hits = Physics2D.OverlapCircleAll(targetPoint, hitRadius, enemyLayer);

        if (hits != null && hits.Length > 0)
        {
            foreach (Collider2D hit in hits)
            {
                if (hit.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(damage);
                }
            }
        }

        Destroy(gameObject, lifeTime);
        initialized = false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(targetPoint, hitRadius);
    }
#endif
}