using UnityEngine;

public readonly struct BossEncounterBalanceSnapshot
{
    public readonly float RawHp;
    public readonly float ShieldHp;
    public readonly float EffectiveHp;
    public readonly float NormalAttackDps;
    public readonly float EnragedAttackDps;
    public readonly float NormalPhaseDuration;
    public readonly float EnragePhaseDuration;

    public BossEncounterBalanceSnapshot(
        float rawHp,
        float shieldHp,
        float normalAttackDps,
        float enragedAttackDps,
        float normalPhaseDuration,
        float enragePhaseDuration)
    {
        RawHp = rawHp;
        ShieldHp = shieldHp;
        EffectiveHp = rawHp + shieldHp;
        NormalAttackDps = normalAttackDps;
        EnragedAttackDps = enragedAttackDps;
        NormalPhaseDuration = normalPhaseDuration;
        EnragePhaseDuration = enragePhaseDuration;
    }

    public float RequiredDpsToClearBeforeEnrage =>
        NormalPhaseDuration > 0f ? EffectiveHp / NormalPhaseDuration : 0f;
}

public static class BossEncounterBalanceCalculator
{
    // This estimates basic-attack pressure only. Area attacks and summoned monsters are
    // intentionally reported separately by the editor audit because their value depends on formation.
    public static bool TryCalculate(
        StageDataSO stage,
        int progressionIndex,
        WaveData wave,
        MonsterSpawnEntry entry,
        out BossEncounterBalanceSnapshot snapshot)
    {
        snapshot = default;

        if (stage == null || wave == null || entry?.data == null || entry.count <= 0)
            return false;
        if (wave.waveType != WaveType.Boss || wave.bossSettings == null)
            return false;
        if (entry.data.Classification != MonsterClassification.Boss || entry.data.BossDefinition == null)
            return false;

        BossWaveSettings timer = wave.bossSettings;
        if (timer.timeLimit <= 0f || timer.enrageLeadTime < 0f || timer.enrageLeadTime > timer.timeLimit)
            return false;

        StageBalanceMultipliers multipliers = StageBalanceCalculator.Calculate(
            stage,
            progressionIndex,
            wave,
            entry);
        BossDefinitionSO definition = entry.data.BossDefinition;
        float rawHp = entry.data.BaseMaxHp * multipliers.Hp;
        float shieldHp = rawHp * Mathf.Max(0f, definition.shieldMaxHpRatio);
        float normalAttackDps = entry.data.BaseAttackDamage
                                * multipliers.Attack
                                * entry.data.BaseAttackPerSecond;
        float enragedAttackDps = normalAttackDps
                                 * Mathf.Max(0f, definition.enragedDamageMultiplier)
                                 * Mathf.Max(0f, definition.enragedAttackSpeedMultiplier);

        snapshot = new BossEncounterBalanceSnapshot(
            rawHp,
            shieldHp,
            normalAttackDps,
            enragedAttackDps,
            timer.timeLimit - timer.enrageLeadTime,
            timer.enrageLeadTime);
        return true;
    }
}
