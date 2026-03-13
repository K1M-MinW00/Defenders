using UnityEngine;

public class MeleeArcAttackBehavior : MeleeAttackBehavior
{
    [Header("Arc Hit")]
    [SerializeField] private float forwardOffset = .8f;

    public override void OnAttackHit()
    {
        if (!isAttacking)
            return;

        Vector2 forward = owner.GetFacingDirection();
        Vector2 center = (Vector2)transform.position + forward * forwardOffset;
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, hitRadius, targetLayer);
        ApplyDamage(hits);
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 forward = owner.GetFacingDirection();
        Vector2 center = (Vector2)transform.position + forward * forwardOffset;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, hitRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, center);
    }
}