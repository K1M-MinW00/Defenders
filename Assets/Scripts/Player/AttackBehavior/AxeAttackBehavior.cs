using UnityEngine;
using System.Collections.Generic;

public class AxeAttackBehavior : MeleeAttackBehavior
{
    [Header("VFX")]
    [SerializeField] private GameObject slashHitboxPrefab;

    [Header("Fan Hit")]
    [SerializeField] private float hitRadius = 1.1f;
    [SerializeField] private float attackAngle = 110f;

    private GameObject spawnedVFX;
    private Vector2 dir;

    public override void OnAttackHit()
    {
        if (!isAttacking)
            return;

        if (currentTarget == null || currentTarget.Health.IsDead)
            return;

        dir = GetAttackDirection();
        Vector2 center = currentTarget.transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        SpawnVfx(center, angle);

        if (targetMode == TargetSelectionMode.Single)
            ApplyDamage(currentTarget);

        else
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, hitRadius, targetLayer);
            ApplyDamage(hits);
        }
    }

    private void SpawnVfx(Vector2 center, float angle)
    {
        if (slashHitboxPrefab == null)
            return;

        spawnedVFX = Instantiate(slashHitboxPrefab, center, Quaternion.Euler(0f, 0f, angle));
    }

    protected override void ApplyDamage(Collider2D[] hits)
    {
        HashSet<IDamageable> damagedTargets = new();

        foreach (var hit in hits)
        {
            Vector2 toTarget = ((Vector2)hit.transform.position - (Vector2)transform.position).normalized;
            float angle = Vector2.Angle(dir, toTarget);

            if (angle > attackAngle * 0.5f)
                continue;

            if (hit.TryGetComponent<IDamageable>(out var damageable))
            {
                if (damagedTargets.Add(damageable))
                    damageable.TakeDamage(Damage);
            }
        }
    }

    public override void OnAttackFinished()
    {
        base.OnAttackFinished();

        if (spawnedVFX != null)
        {
            Destroy(spawnedVFX);
            spawnedVFX = null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 ownerPos = transform.position;

        if (Application.isPlaying)
        {
            dir = currentTarget != null ? GetAttackDirection() : dir;
            if (dir.sqrMagnitude < 0.0001f)
                dir = Vector2.right;
        }
        else
        {
            dir = Vector2.right;
        }

        Vector2 center;
        if (Application.isPlaying)
        {
            center = currentTarget != null ? transform.position : ownerPos;
        }
        else
        {
            center = ownerPos ;
        }

        // 1. 실제 후보 수집 범위 (OverlapCircleAll)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center, hitRadius);

        // 2. owner 기준 실제 공격 방향
        Gizmos.color = Color.red;
        Gizmos.DrawLine(ownerPos, ownerPos + dir * hitRadius);

        // 3. 부채꼴 각도 경계선
        Vector2 leftDir = Quaternion.Euler(0f, 0f, -attackAngle * 0.5f) * dir;
        Vector2 rightDir = Quaternion.Euler(0f, 0f, attackAngle * 0.5f) * dir;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(ownerPos, ownerPos + leftDir.normalized * hitRadius);
        Gizmos.DrawLine(ownerPos, ownerPos + rightDir.normalized * hitRadius);

        // 4. 중심점 표시
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(center, 0.05f);
    }
}