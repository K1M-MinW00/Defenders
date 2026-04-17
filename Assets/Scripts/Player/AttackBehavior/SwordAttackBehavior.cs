using UnityEngine;

public class SwordAttackBehavior : MeleeAttackBehavior
{
    [Header("VFX")]
    [SerializeField] private GameObject slashHitboxPrefab;

    [Header("Hit")]
    [SerializeField] private Vector2 boxSize = new Vector2(1.0f, 0.8f);
    [SerializeField] private float forwardOffset = 0.4f;

    private GameObject spawnedVFX;

    public override void OnAttackHit()
    {
        if (!isAttacking)
            return;

        if (currentTarget == null || currentTarget.Health.IsDead)
            return;

        Vector2 dir = GetAttackDirection();
        Vector2 center = currentTarget.transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        SpawnVfx(center, angle);

        if (targetMode == TargetSelectionMode.Single)
            ApplyDamage(currentTarget);

        else
        {
            Collider2D[] hits = Physics2D.OverlapBoxAll(center, boxSize, angle, targetLayer);
            ApplyDamage(hits);
        }
    }


    private void SpawnVfx(Vector2 center, float angle)
    {
        if (slashHitboxPrefab == null)
            return;

        spawnedVFX = Instantiate(slashHitboxPrefab, center, Quaternion.Euler(0f, 0f, angle));
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
        Vector2 dir = Application.isPlaying ? GetAttackDirection() : Vector2.right;
        Vector2 center = (Vector2)transform.position + dir * forwardOffset;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(center, Quaternion.Euler(0f, 0f, angle), Vector3.one);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(Vector3.zero, boxSize);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(Vector3.zero, Vector3.right * boxSize.x * 0.5f);

        Gizmos.matrix = oldMatrix;

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(center, 0.05f);
    }
}