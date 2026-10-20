using System.Linq;
using NUnit.Framework;
using UnityEditor;

public sealed class TutorialStageDataTests
{
    private const string TutorialStagePath =
        "Assets/GameData/Tutorial/Tutorial_FirstBattle.asset";

    [Test]
    public void TutorialStage_IsValidAndContainsRequiredWaveTypes()
    {
        StageDataSO stage = AssetDatabase.LoadAssetAtPath<StageDataSO>(TutorialStagePath);

        Assert.That(stage, Is.Not.Null);
        Assert.That(stage.DisplayName, Is.EqualTo("Tutorial"));
        Assert.That(stage.TryValidate(out string error), Is.True, error);
        Assert.That(stage.waves.Select(wave => wave.waveType), Is.EqualTo(new[]
        {
            WaveType.Normal,
            WaveType.Normal,
            WaveType.Elite,
            WaveType.Normal,
            WaveType.Boss,
        }));
    }

    [Test]
    public void TutorialStage_UsesLowRiskCombatTuning()
    {
        StageDataSO stage = AssetDatabase.LoadAssetAtPath<StageDataSO>(TutorialStagePath);

        Assert.That(stage.hpMultiplier, Is.LessThanOrEqualTo(0.35f));
        Assert.That(stage.attackMultiplier, Is.LessThanOrEqualTo(0.1f));
        Assert.That(stage.waves.SelectMany(wave => wave.subWaves)
            .SelectMany(subWave => subWave.spawnEntries)
            .All(entry => entry.count == 1), Is.True);

        float[] openingHp = stage.waves.Take(3)
            .Select(wave => wave.hpMultiplier)
            .ToArray();
        float[] openingAttack = stage.waves.Take(3)
            .Select(wave => wave.attackMultiplier)
            .ToArray();

        Assert.That(openingHp, Is.EqualTo(new[] { 0.25f, 0.35f, 0.45f }));
        Assert.That(openingAttack, Is.All.LessThanOrEqualTo(0.15f));
    }

    [Test]
    public void TutorialStage_StartsWithExactlyOneSummonCost()
    {
        StageDataSO stage = AssetDatabase.LoadAssetAtPath<StageDataSO>(TutorialStagePath);

        Assert.That(stage.economyConfig, Is.Not.Null);
        Assert.That(stage.economyConfig.initialGold, Is.EqualTo(5));
        Assert.That(stage.economyConfig.summonUnit, Is.EqualTo(5));
        Assert.That(stage.economyConfig.normalReward, Is.EqualTo(15));
        Assert.That(stage.economyConfig.GetPopulationIncreaseCost(
            stage.economyConfig.initialPopulationLimit), Is.GreaterThan(5));
    }

    [Test]
    public void TutorialStage_RewardsOneAccountAndUnitLevelUp()
    {
        StageDataSO stage = AssetDatabase.LoadAssetAtPath<StageDataSO>(TutorialStagePath);
        MaterialDataSO trainingMaterial = AssetDatabase.LoadAssetAtPath<MaterialDataSO>(
            "Assets/Resources/GameData/Items/Materials/material_exp_1.asset");

        RewardData accountExp = stage.clearRewards.Single(reward => reward.Type == RewardType.Experience);
        RewardData gold = stage.clearRewards.Single(reward => reward.Type == RewardType.Gold);
        RewardData unitExp = stage.clearRewards.Single(reward =>
            reward.Type == RewardType.Item && reward.Id == "material_exp_1");

        Assert.That(accountExp.Amount, Is.EqualTo(100));
        Assert.That(trainingMaterial.Value * unitExp.Amount, Is.EqualTo(UnitExpTable.GetRequiredExp(1)));
        Assert.That(gold.Amount, Is.EqualTo(UnitExpTable.GetRequiredExp(1)));
    }
}
