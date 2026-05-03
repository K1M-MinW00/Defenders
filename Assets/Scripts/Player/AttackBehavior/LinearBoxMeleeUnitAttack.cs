
using UnityEngine;

public class LinearBoxMeleeUnitAttack : MeleeUnitAttack
{
    [Header("VFX")]
    [SerializeField] private GameObject hitboxPrefab;

    [Header("Hit Box")]
    [SerializeField] private Vector2 boxSize = new Vector2(4f, 0.5f);


    public override void OnAttackHit()
    {
        if (!isAttacking)
            return;

        Vector2 dir = GetAttackDirection();
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        float length = owner.DetectRange;
        boxSize.x = length;
        Vector2 center = (Vector2)owner.transform.position + dir * (length * 0.5f);

        SpawnVFX(hitboxPrefab, center, angle);

        if (CurrentTargetMode == TargetSelectionMode.Single)
        {
            ApplyDamage(currentTarget);
            return;
        }

        int hitCount = OverlapBox(center, boxSize, angle);
        ApplyDamage(hitBuffer, hitCount);
    }


    private void OnDrawGizmosSelected()
    {
        Vector2 dir = Application.isPlaying ? GetAttackDirection() : Vector2.right;
        Vector2 center = (Vector2)transform.position + dir * (boxSize.x * 0.5f);
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