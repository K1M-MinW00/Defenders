
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
        {
            OnAttackFinished();
            return;
        }

        Vector3 spawnPos = pendingTarget.transform.position;

        MagicImpact impact = Instantiate(magicPrefab,spawnPos, Quaternion.identity);
        impact.Initialize(Damage, targetLayer);

        OnAttackFinished();
    }
}
