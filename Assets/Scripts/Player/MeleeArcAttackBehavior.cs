using UnityEngine;

public class MeleeArcAttackBehavior : MeleeAttackBehavior
{
    [Header("Arc Hit")]
    [SerializeField] private SwordAura swordAura;
    [SerializeField] private float spawnOffset = .6f;
    [SerializeField] private float projectileSpeed = 8f;
    [SerializeField] private float projectileLifeTime = .8f;
    [SerializeField] private int pierceCount = 1;
    [SerializeField] private float projectileRadius = .35f;
    [SerializeField] private float maxTravelDistance = 3f;

    [SerializeField] private float forwardOffset = .8f;

    public override void OnAttackHit()
    {
        if (!isAttacking)
            return;

        Vector2 dir = GetAttackDirection();
        Vector2 spawnPos = (Vector2)transform.position + dir * spawnOffset;

        SwordAura aura = Instantiate(swordAura, spawnPos, Quaternion.identity);
        aura.Initialize(Damage,dir,projectileSpeed,projectileLifeTime,maxTravelDistance,projectileRadius,pierceCount,targetLayer );
        
        //Vector2 forward = owner.GetFacingDirection();
        //Vector2 center = (Vector2)transform.position + forward * forwardOffset;
        //Collider2D[] hits = Physics2D.OverlapCircleAll(center, hitRadius, targetLayer);
        //ApplyDamage(hits);
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 direction = Application.isPlaying && owner != null ? GetAttackDirection() : Vector2.right;

        Vector2 spawnPosition = (Vector2)transform.position + direction * spawnOffset;
        Vector2 endPosition = spawnPosition + direction * maxTravelDistance;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(spawnPosition, projectileRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(spawnPosition, endPosition);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(endPosition, projectileRadius);
    }
}