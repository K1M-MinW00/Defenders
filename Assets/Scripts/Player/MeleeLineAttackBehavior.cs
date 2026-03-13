using UnityEngine;

public class MeleeLineAttackBehavior : MeleeAttackBehavior
{
    [Header("Line Hit")]
    [SerializeField] private Vector2 boxSize = new Vector2(1.5f, 0.5f);
    [SerializeField] private float forwardOffset = 1f;

    public override void OnAttackHit()
    {
        if (!isAttacking)
            return;

        Vector2 forward = owner.GetFacingDirection();
        Vector2 center = (Vector2)transform.position + forward * forwardOffset;

        Collider2D[] hits = Physics2D.OverlapBoxAll(center, boxSize, 0, targetLayer);
        ApplyDamage(hits);
    }
}