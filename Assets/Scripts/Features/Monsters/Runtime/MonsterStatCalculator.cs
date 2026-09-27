using UnityEngine;

public static class MonsterStatCalculator
{
    private const float MinimumAttackPerSecond = 0.01f;

    public static MonsterStats Calculate(MonsterDataSO data)
    {
        return Calculate(data?.CreateRuntimeStats());
    }

    public static MonsterStats Calculate(MonsterStats source)
    {
        MonsterStats stats = source?.CreateRuntimeCopy() ?? new MonsterStats();
        stats.maxHp = Mathf.Max(1f, stats.maxHp);
        stats.moveSpeed = Mathf.Max(0f, stats.moveSpeed);
        stats.atkDamage = Mathf.Max(0f, stats.atkDamage);
        stats.atkRange = Mathf.Max(0f, stats.atkRange);
        stats.atkPerSec = Mathf.Max(MinimumAttackPerSecond, stats.atkPerSec);
        return stats;
    }
}
