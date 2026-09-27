using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class StageRewardService : MonoBehaviour
{
    [SerializeField] private EconomyManager economyManager;
    [SerializeField, Min(1)] private int failureFuelReward = 5;

    public int FailureFuelReward => failureFuelReward;

    public void GiveWaveReward(WaveData waveData)
    {
        if (waveData == null)
            return;

        economyManager.ApplyWaveReward(waveData.waveType);
    }

    public Task<StageOutcomeResult> GiveStageClearRewardAsync(StageDataSO stageData)
    {
        return UserDataManager.Instance.CompleteStageAsync(stageData);
    }

    public Task<StageOutcomeResult> GiveStageFailRewardAsync(
        StageDataSO stageData,
        int clearedWaveCount)
    {
        return UserDataManager.Instance.FailStageAsync(
            stageData,
            clearedWaveCount,
            CreateFailureRewards());
    }

    public IReadOnlyList<RewardData> CreateFailureRewards()
    {
        return new[]
        {
            new RewardData { Type = RewardType.Fuel, Amount = failureFuelReward },
        };
    }
}
