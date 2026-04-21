using UnityEngine;

public class Knight_IronWill : PassiveSkillBase
{
    [Header("Iron Will")]
    [SerializeField] private int requiredHits = 3;
    [SerializeField] private float duration = 3f;
    [SerializeField] private float damageReductionPercent = 0.25f;

    private int hitCount;
    private float buffEndTime;

    protected override void ResetRuntimeState()
    {
        hitCount = 0;
        buffEndTime = -999f;
    }

    public override void OnBeforeTakeDamage(ref float damage)
    {
        if (owner == null || owner.IsDead)
            return;

        // 버프 적용 중이면 피해 감소
        if (Time.time < buffEndTime)
        {
            damage *= (1f - damageReductionPercent);
            return;
        }
    }

    public override void OnAfterTakeDamage(float finalDamage)
    {
        if (owner == null || owner.IsDead)
            return;

        if (finalDamage <= 0f)
            return;

        hitCount++;

        if (hitCount < requiredHits)
            return;

        hitCount = 0;
        buffEndTime = Time.time + duration;
    }
}