using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public readonly struct StageBalanceMultipliers
{
    public readonly float Hp;
    public readonly float Attack;
    public readonly float Pressure;

    public StageBalanceMultipliers(float hp, float attack, float pressure)
    {
        Hp = Mathf.Max(0.01f, hp);
        Attack = Mathf.Max(0.01f, attack);
        Pressure = Mathf.Max(0.01f, pressure);
    }
}

public static class StageBalanceCalculator
{
    public static int GetProgressionIndex(StageDataSO target, IEnumerable<StageDataSO> stages)
    {
        if (target == null || stages == null)
            return 0;

        StageDataSO[] ordered = stages
            .Where(stage => stage != null)
            .OrderBy(stage => stage.sector)
            .ThenBy(stage => stage.stage)
            .ToArray();

        int index = System.Array.IndexOf(ordered, target);
        if (index >= 0)
            return index;

        return ordered.Count(stage => stage.sector < target.sector ||
                                      stage.sector == target.sector && stage.stage < target.stage);
    }

    public static StageBalanceMultipliers Calculate(
        StageDataSO stage,
        int progressionIndex,
        WaveData wave = null,
        MonsterSpawnEntry entry = null,
        StageBalanceConfigSO config = null)
    {
        config ??= StageBalanceConfigDatabase.Get();
        float hpGrowth = config != null ? config.hpGrowthPerStage : 0f;
        float attackGrowth = config != null ? config.attackGrowthPerStage : 0f;
        StageThreatPreset preset = config?.GetPreset(stage != null ? stage.threatType : StageThreatType.Balanced);

        float hp = Mathf.Pow(1f + hpGrowth, Mathf.Max(0, progressionIndex));
        float attack = Mathf.Pow(1f + attackGrowth, Mathf.Max(0, progressionIndex));
        float pressure = 1f;

        if (preset != null)
        {
            hp *= PositiveOrOne(preset.hpMultiplier);
            attack *= PositiveOrOne(preset.attackMultiplier);
            pressure *= PositiveOrOne(preset.pressureMultiplier);
        }

        if (stage != null)
        {
            hp *= PositiveOrOne(stage.hpMultiplier);
            attack *= PositiveOrOne(stage.attackMultiplier);
            pressure *= PositiveOrOne(stage.pressureMultiplier);
        }

        if (wave != null)
        {
            hp *= PositiveOrOne(wave.hpMultiplier);
            attack *= PositiveOrOne(wave.attackMultiplier);
            pressure *= PositiveOrOne(wave.pressureMultiplier);
        }

        if (entry != null)
        {
            hp *= PositiveOrOne(entry.hpMultiplier);
            attack *= PositiveOrOne(entry.attackMultiplier);
        }

        return new StageBalanceMultipliers(hp, attack, pressure);
    }

    public static float CalculateThreat(
        MonsterSpawnEntry entry,
        StageBalanceMultipliers multipliers)
    {
        if (entry?.data == null || entry.count <= 0)
            return 0f;

        float hp = Mathf.Max(1f, entry.data.BaseMaxHp * multipliers.Hp);
        float dps = Mathf.Max(0.1f, entry.data.BaseAttackDamage * entry.data.BaseAttackPerSecond * multipliers.Attack);
        float cadence = 1f / Mathf.Max(0.1f, entry.interval + 0.25f);
        return entry.count * entry.data.ThreatCost * Mathf.Sqrt(hp * dps) * cadence * multipliers.Pressure;
    }

    private static float PositiveOrOne(float value) => value > 0f ? value : 1f;
}
