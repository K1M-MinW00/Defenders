using UnityEngine;
using System.Collections.Generic;

public class ConeMeleeUnitAttack : MeleeUnitAttack
{
    [Header("VFX")]
    [SerializeField] private GameObject hitboxPrefab;

    [Header("Cone Hit")]
    [SerializeField] private float attackAngle = 110f;

    private float hitRadius;
    private Vector2 dir;

    public override void OnAttackHit()
    {
        if (!isAttacking)
            return;

        dir = GetAttackDirection();

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        hitRadius = owner.DetectRange;

        Vector2 origin = owner.transform.position;
        Vector2 center = origin + dir * (hitRadius * 0.5f);

        SpawnVFX(hitboxPrefab, center, angle);

        if (CurrentTargetMode == TargetSelectionMode.Single)
        {
            ApplyDamage(currentTarget);
            return;
        }

        int hitCount = OverlapCircle(origin, hitRadius);
        ApplyDamage(hitBuffer,hitCount, origin, dir);
    }

    private void ApplyDamage(Collider2D[] hits, int hitCount, Vector2 origin, Vector2 dir)
    {
        if (hitCount <= 0)
            return;

        damagedTargets.Clear();

        float halfAngle = attackAngle * 0.5f;
        float cosThreshold = Mathf.Cos(halfAngle * Mathf.Deg2Rad);

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = hits[i];

            if (hit == null)
                continue;

            Vector2 toTarget = (Vector2)hit.transform.position - origin;
            float sqrDistance = toTarget.sqrMagnitude;

            if (sqrDistance<= 0.0001f)
                continue;

            Vector2 targetDir = toTarget.normalized;
            float dot = Vector2.Dot(dir, targetDir);

            if (dot < cosThreshold)
                continue;

            if (!hit.TryGetComponent(out IDamageable damageable))
                continue;

            if (!damagedTargets.Add(damageable))
                continue;

            MonsterController target = hit.GetComponent<MonsterController>();

            if (target != null && target.Health.IsDead)
                continue;

            float damage = Damage;
            owner.SkillController.NotifyAttackHit(target, ref damage);
            damageable.TakeDamage(damage);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 ownerPos = transform.position;

        hitRadius = Application.isPlaying ? owner.DetectRange : hitRadius;
        Vector2 dir = Application.isPlaying ? GetAttackDirection() : Vector2.right;
       
        // 1. 실제 후보 수집 범위 (OverlapCircleAll)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(ownerPos, hitRadius);

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
        Gizmos.DrawSphere(ownerPos, 0.05f);
    }
}