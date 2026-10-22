using System.Linq;
using NUnit.Framework;
using UnityEditor;

public sealed class BossEncounterBalanceCalculatorTests
{
    [TestCase("Assets/Resources/GameData/Stages/Stage_1_5.asset")]
    [TestCase("Assets/Resources/GameData/Stages/Stage_2_5.asset")]
    public void ChapterBossSnapshot_MatchesRuntimeBalancePipeline(string path)
    {
        StageDataSO stage = AssetDatabase.LoadAssetAtPath<StageDataSO>(path);
        StageDataSO[] stages = LoadOrderedStages();
        int progressionIndex = System.Array.IndexOf(stages, stage);
        WaveData wave = stage.waves.Last();
        MonsterSpawnEntry entry = FindBossEntry(wave);

        Assert.That(BossEncounterBalanceCalculator.TryCalculate(
            stage, progressionIndex, wave, entry, out BossEncounterBalanceSnapshot snapshot), Is.True);

        StageBalanceMultipliers multipliers = StageBalanceCalculator.Calculate(
            stage, progressionIndex, wave, entry);
        BossDefinitionSO definition = entry.data.BossDefinition;
        float expectedHp = entry.data.BaseMaxHp * multipliers.Hp;
        float expectedDps = entry.data.BaseAttackDamage * multipliers.Attack * entry.data.BaseAttackPerSecond;

        Assert.That(snapshot.RawHp, Is.EqualTo(expectedHp).Within(0.01f));
        Assert.That(snapshot.ShieldHp, Is.EqualTo(expectedHp * definition.shieldMaxHpRatio).Within(0.01f));
        Assert.That(snapshot.NormalAttackDps, Is.EqualTo(expectedDps).Within(0.01f));
        Assert.That(snapshot.EnragedAttackDps, Is.EqualTo(
            expectedDps * definition.enragedDamageMultiplier * definition.enragedAttackSpeedMultiplier).Within(0.01f));
        Assert.That(snapshot.NormalPhaseDuration, Is.EqualTo(60f));
        Assert.That(snapshot.EnragePhaseDuration, Is.EqualTo(30f));
    }

    [Test]
    public void ChapterBosses_PreserveNormalizedTankAndFastAttackerArchetypes()
    {
        StageDataSO firstStage = AssetDatabase.LoadAssetAtPath<StageDataSO>(
            "Assets/Resources/GameData/Stages/Stage_1_5.asset");
        StageDataSO secondStage = AssetDatabase.LoadAssetAtPath<StageDataSO>(
            "Assets/Resources/GameData/Stages/Stage_2_5.asset");
        MonsterSpawnEntry first = FindBossEntry(firstStage.waves.Last());
        MonsterSpawnEntry second = FindBossEntry(secondStage.waves.Last());
        float firstNormalizedHp = first.data.BaseMaxHp * firstStage.waves.Last().hpMultiplier * first.hpMultiplier;
        float secondNormalizedHp = second.data.BaseMaxHp * secondStage.waves.Last().hpMultiplier * second.hpMultiplier;
        float firstNormalizedDps = first.data.BaseAttackDamage * firstStage.waves.Last().attackMultiplier
                                   * first.attackMultiplier * first.data.BaseAttackPerSecond;
        float secondNormalizedDps = second.data.BaseAttackDamage * secondStage.waves.Last().attackMultiplier
                                    * second.attackMultiplier * second.data.BaseAttackPerSecond;

        Assert.That(firstNormalizedHp, Is.GreaterThan(secondNormalizedHp),
            "Before global progression scaling, chapter 1 should retain the tankier base profile.");
        Assert.That(secondNormalizedDps, Is.GreaterThan(firstNormalizedDps),
            "Before global progression scaling, chapter 2 should retain the faster damage profile.");
    }

    [Test]
    public void InvalidEncounter_IsRejected()
    {
        Assert.That(BossEncounterBalanceCalculator.TryCalculate(
            null, 0, null, null, out BossEncounterBalanceSnapshot _), Is.False);
    }

    private static StageDataSO[] LoadOrderedStages()
    {
        return AssetDatabase.FindAssets("t:StageDataSO", new[] { "Assets/Resources/GameData/Stages" })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<StageDataSO>)
            .Where(stage => stage != null)
            .OrderBy(stage => stage.sector)
            .ThenBy(stage => stage.stage)
            .ToArray();
    }

    private static MonsterSpawnEntry FindBossEntry(WaveData wave)
    {
        return wave.subWaves
            .SelectMany(subWave => subWave.spawnEntries)
            .Single(entry => entry.data.Classification == MonsterClassification.Boss);
    }
}
