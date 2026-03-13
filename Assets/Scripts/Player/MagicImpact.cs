using UnityEngine;

public class MagicImpact : MonoBehaviour
{
    [Header("Impact")]
    [SerializeField] private float radius = .8f;
    [SerializeField] private LayerMask targetLayer;

    private float damage;
    private bool initialized;

    public void Initialize(float damamge)
    {
        this.damage= damamge;
        initialized = true;

        ApplyDamage();
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
    
}