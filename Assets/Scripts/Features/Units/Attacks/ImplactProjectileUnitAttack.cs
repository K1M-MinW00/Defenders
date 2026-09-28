using UnityEngine;

public class ImplactProjectileUnitAttack : RangedUnitAttack
{
    [Header("Magic")]
    [SerializeField] private MagicImpact magicPrefab;

    public override void OnAttackHit()
    {
        if (!TryEnterHitPhase())
            return;

        if (currentTarget == null)
        {
            CancelAttack();
            return;
        }

        Vector3 spawnPos = currentTarget.TargetTransform.position;

        MagicImpact impact = owner.PoolManager.Spawn(magicPrefab,spawnPos, Quaternion.identity,PoolCategory.Projectile);
       
        if(impact != null)
            impact.Initialize(Damage, targetLayer, owner);

        OnAttackFinished();
    }
}
