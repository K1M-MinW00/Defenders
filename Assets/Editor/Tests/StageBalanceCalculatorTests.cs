using NUnit.Framework;
using UnityEngine;

public sealed class StageBalanceCalculatorTests
{
    [Test]
    public void ProgressionIndex_IsDerivedFromStageOrder()
    {
        StageDataSO stage12 = ScriptableObject.CreateInstance<StageDataSO>();
        StageDataSO stage21 = ScriptableObject.CreateInstance<StageDataSO>();
        StageDataSO stage11 = ScriptableObject.CreateInstance<StageDataSO>();
        stage12.sector = 1; stage12.stage = 2;
        stage21.sector = 2; stage21.stage = 1;
        stage11.sector = 1; stage11.stage = 1;

        Assert.That(StageBalanceCalculator.GetProgressionIndex(stage11, new[] { stage12, stage21, stage11 }), Is.EqualTo(0));
        Assert.That(StageBalanceCalculator.GetProgressionIndex(stage12, new[] { stage12, stage21, stage11 }), Is.EqualTo(1));
        Assert.That(StageBalanceCalculator.GetProgressionIndex(stage21, new[] { stage12, stage21, stage11 }), Is.EqualTo(2));

        Object.DestroyImmediate(stage11);
        Object.DestroyImmediate(stage12);
        Object.DestroyImmediate(stage21);
    }

    [Test]
    public void Calculate_CombinesProgressionPresetAndOverrides()
    {
        StageBalanceConfigSO config = ScriptableObject.CreateInstance<StageBalanceConfigSO>();
        config.hpGrowthPerStage = 0.1f;
        config.attackGrowthPerStage = 0.05f;
        config.presets.Add(new StageThreatPreset { type = StageThreatType.Durable, hpMultiplier = 1.2f, attackMultiplier = 0.9f, pressureMultiplier = 0.8f });
        StageDataSO stage = ScriptableObject.CreateInstance<StageDataSO>();
        stage.threatType = StageThreatType.Durable;
        stage.hpMultiplier = 1.1f;
        stage.attackMultiplier = 1.2f;
        stage.pressureMultiplier = 1.25f;
        var wave = new WaveData { hpMultiplier = 1.3f, attackMultiplier = 0.8f, pressureMultiplier = 1.1f };
        var entry = new MonsterSpawnEntry { hpMultiplier = 0.9f, attackMultiplier = 1.15f };

        StageBalanceMultipliers result = StageBalanceCalculator.Calculate(stage, 2, wave, entry, config);

        Assert.That(result.Hp, Is.EqualTo(Mathf.Pow(1.1f, 2) * 1.2f * 1.1f * 1.3f * 0.9f).Within(0.0001f));
        Assert.That(result.Attack, Is.EqualTo(Mathf.Pow(1.05f, 2) * 0.9f * 1.2f * 0.8f * 1.15f).Within(0.0001f));
        Assert.That(result.Pressure, Is.EqualTo(0.8f * 1.25f * 1.1f).Within(0.0001f));

        Object.DestroyImmediate(stage);
        Object.DestroyImmediate(config);
    }
}
