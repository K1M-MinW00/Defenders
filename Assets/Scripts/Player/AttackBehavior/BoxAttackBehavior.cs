using UnityEngine;

public class BoxAttackBehavior : MeleeAttackBehavior
{
    [Header("VFX")]
    [SerializeField] private GameObject hitboxPrefab;

    [Header("Hit")]
    [SerializeField] private Vector2 boxSize = new Vector2(1.0f, 0.8f);

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
            Collider2D[] hits = Physics2D.OverlapBoxAll(center, boxSize, angle, targetLayer);
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

    private void OnDrawGizmosSelected()
    {
        Vector2 dir = Application.isPlaying ? GetAttackDirection() : Vector2.right;
        Vector2 center = (Vector2)transform.position + dir;
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