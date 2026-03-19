using UnityEngine;

public class MagicImpact : MonoBehaviour
{
    [Header("Impact")]
    [SerializeField] private float radius = .8f;
    [SerializeField] private LayerMask targetLayer;

    private float damage;

    public void Initialize(float damamge)
    {
        this.damage = damamge;
    }

    public void OnAnimationEnd()
    {
        // gameObject.SetActive(false);
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