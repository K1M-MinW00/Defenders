using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public sealed class WaveMilestoneRewardRulesTests
{
    [Test]
    public void CurrentStage_UnlocksOnlyPersistedBestWave()
    {
        UserProgressData progress = new() { CurrentSector = 1, CurrentStage = 1, BestWaveCleared = 4 };

        Assert.That(WaveMilestoneRewardRules.IsUnlocked(progress, 1, 1, 4), Is.True);
        Assert.That(WaveMilestoneRewardRules.IsUnlocked(progress, 1, 1, 5), Is.False);
    }

    [Test]
    public void ClearedStage_RemainsFullyUnlockedAfterProgressAdvances()
    {
        UserProgressData progress = new() { CurrentSector = 1, CurrentStage = 2, BestWaveCleared = 0 };

        Assert.That(WaveMilestoneRewardRules.IsUnlocked(progress, 1, 1, 999), Is.True);
    }

    [Test]
    public void RewardCursor_KeepsEarlierUnclaimedStageVisible()
    {
        StageDataSO first = CreateStage(1, 1, 3);
        StageDataSO second = CreateStage(1, 2, 2);
        UserProgressData progress = new()
        {
            CurrentSector = 1,
            CurrentStage = 2,
            WaveRewardProgress = new UserWaveRewardProgressData { Sector = 1, Stage = 1 },
        };

        try
        {
            StageCatalog catalog = new(new[] { second, first });
            Assert.That(WaveMilestoneRewardRules.ResolveRewardStage(progress, catalog), Is.SameAs(first));
        }
        finally
        {
            Object.DestroyImmediate(first);
            Object.DestroyImmediate(second);
        }
    }

    private static StageDataSO CreateStage(int sector, int stageNumber, int requiredWave)
    {
        StageDataSO stage = ScriptableObject.CreateInstance<StageDataSO>();
        stage.sector = sector;
        stage.stage = stageNumber;
        stage.milestoneRewards = new List<WaveMilestoneReward>
        {
            new() { requiredWave = requiredWave, rewards = new List<RewardData> { new() { Type = RewardType.Gold, Amount = 1 } } },
        };
        return stage;
    }
}
