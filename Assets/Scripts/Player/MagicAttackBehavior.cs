
using UnityEngine;

public class MagicAttackBehavior : RangedAttackBehavior
{
    [Header("Magic")]
    [SerializeField] private MagicImpact magicPrefab;

    public override void OnAttackCast()
    {
        if (!IsAttacking)
            return;

        if (pendingTarget == null)
            return;

        if (magicPrefab == null)
            return;

        Vector3 spawnPos = pendingTarget.transform.position;

        MagicImpact impact = Instantiate(magicPrefab,spawnPos, Quaternion.identity);
        impact.Initialize(Damage);
    }
}
