using UnityEngine;

public class MagicAttackBehavior : RangedAttackBehavior
{
    [Header("Magic")]
    [SerializeField] private MagicImpact magicPrefab;

    public override void OnAttackHit()
    {
        if (!IsAttacking)
            return;

        if (currentTarget == null)
        {
            OnAttackFinished();
            return;
        }

        Vector3 spawnPos = currentTarget.transform.position;

        MagicImpact impact = Instantiate(magicPrefab,spawnPos, Quaternion.identity);
        impact.Initialize(Damage, targetLayer);

        OnAttackFinished();
    }
}
