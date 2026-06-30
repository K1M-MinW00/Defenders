using UnityEngine;

public class Knight_IronWill : PassiveSkillBase
{
    [Header("Iron Will")]
    [SerializeField] private int requiredHits = 4;
    [SerializeField] private int upgrade_requiredHits = 3;

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
        if (!CanUsePassive())
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

        int required = skillController.HasPassiveUpgrade2 ? upgrade_requiredHits : requiredHits;

        if (hitCount < required)
            return;

        hitCount = 0;
        buffEndTime = Time.time + duration;
    }
}