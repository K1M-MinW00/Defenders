using UnityEngine;

public class MagicImpact : MonoBehaviour
{
    [Header("Impact")]
    [SerializeField] private LayerMask targetLayer;

    private float damage;

    public void Initialize(float damamge, LayerMask target)
    {
        this.damage = damamge;
        targetLayer = target;
    }

    public void OnAnimationEnd()
    {
        // TODO - object pool 반환
        Destroy(gameObject);
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