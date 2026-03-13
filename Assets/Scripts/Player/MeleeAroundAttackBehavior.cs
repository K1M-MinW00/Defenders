using UnityEngine;
public class MeleeAroundAttackBehavior : MeleeAttackBehavior
{
    public override void OnAttackHit()
    {
        if (!isAttacking)
            return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, hitRadius, targetLayer);
        ApplyDamage(hits);
    }
}

