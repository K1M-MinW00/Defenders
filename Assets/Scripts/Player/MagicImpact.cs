using UnityEngine;

public class MagicImpact : MonoBehaviour
{
    [Header("Impact")]
    [SerializeField] private float radius = .8f;
    [SerializeField] private LayerMask targetLayer;

    private float damage;

    public void Initialize(float damamge)
    {
        this.damage= damamge;

        //ApplyDamage();
    }

    private void ApplyDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius,targetLayer);

        foreach(var hit in hits)
        {
            if (!hit.TryGetComponent<IDamageable>(out var damageable))
                continue;

            damageable.TakeDamage(damage);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & targetLayer) == 0)
            return;

        if (collision.TryGetComponent<IDamageable>(out IDamageable damageable))
        {
            damageable.TakeDamage(damage);

        }
    }
}