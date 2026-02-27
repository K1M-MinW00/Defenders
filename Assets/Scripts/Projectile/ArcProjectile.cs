using UnityEngine;

public class ArcProjectile : MonoBehaviour
{
    private Vector3 startPos;
    private Vector3 endPos;

    private float damage;
    private float flightTime;
    private float arcHeight;

    private float splashRadius;
    private LayerMask targetLayer;

    private float timer;

    public void Initialize(Vector3 target, float damage, float flightTime, float arcHeight, float splashRadius, LayerMask targetLayer)
    {
        this.startPos = transform.position;
        this.endPos = target;

        this.damage = damage;
        this.flightTime = Mathf.Max(0.05f, flightTime);
        this.arcHeight = arcHeight;

        this.splashRadius = splashRadius;
        this.targetLayer = targetLayer;

        timer = 0f;
    }

    private void Update()
    {
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
            Destroy(gameObject); // 추후 Object Pool 반환
        }
    }

    private void Impact()
    {
        // 스플래시
        Collider2D[] hits = Physics2D.OverlapCircleAll(endPos, splashRadius, targetLayer);
        for (int i = 0; i < hits.Length; i++)
        {
            var ui = hits[i].GetComponent<IDamageable>();

            if (ui == null)
                continue;
            ui.TakeDamage(damage);
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