using UnityEngine;
using System.Collections.Generic;

public class ConeAttackBehavior : MeleeAttackBehavior
{
    [Header("VFX")]
    [SerializeField] private GameObject hitboxPrefab;

    [Header("Cone Hit")]
    [SerializeField] private float attackAngle = 110f;
    [SerializeField] private float hitRadius = 3f;

    private GameObject spawnedVFX;
    private Vector2 dir;

    public override void OnAttackHit()
    {
        if (!isAttacking)
            return;

        if (currentTarget == null || currentTarget.Health.IsDead)
            return;

        dir = GetAttackDirection();
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        Vector2 center = currentTarget.transform.position;
        SpawnVfx(center, angle);

        if (CurrentTargetMode == TargetSelectionMode.Single)
            ApplyDamage(currentTarget);

        else
        {
            hitRadius = owner.DetectRange;
            Collider2D[] hits = Physics2D.OverlapCircleAll(owner.transform.position, hitRadius, targetLayer);
            ApplyDamage(hits);
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

    private void SpawnVfx(Vector2 center, float angle)
    {
        if (hitboxPrefab == null)
            return;

        spawnedVFX = Instantiate(hitboxPrefab, center, Quaternion.Euler(0f, 0f, angle));
    }

    protected override void ApplyDamage(Collider2D[] hits)
    {
        if (hits == null || hits.Length == 0) 
            return;
        
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
                {
                    float damage = Damage;
                    var target = hit.GetComponent<MonsterController>();
                    owner.SkillController.NotifyAttackHit(target, ref damage);
                    damageable.TakeDamage(damage);
                }
            }
        }
    }


    private void OnDrawGizmosSelected()
    {
        Vector2 ownerPos = transform.position;

        hitRadius = Application.isPlaying ? owner.DetectRange : hitRadius;
        Vector2 dir = Application.isPlaying ? GetAttackDirection() : Vector2.right;
        Vector2 center = ownerPos;

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