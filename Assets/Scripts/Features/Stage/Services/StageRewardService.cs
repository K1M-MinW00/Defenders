using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class StageRewardService : MonoBehaviour
{
    [SerializeField] private EconomyManager economyManager;

    public void GiveWaveReward(WaveData waveData)
    {
        if (waveData == null)
            return;

        if (!economyManager.ApplyWaveReward(waveData.waveType))
            Debug.LogError("Failed to apply wave gold reward.", this);
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
            clearedWaveCount);
    }
}
