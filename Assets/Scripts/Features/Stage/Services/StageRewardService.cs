using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class StageRewardService : MonoBehaviour
{
    [SerializeField] private EconomyManager economyManager;
    [SerializeField] private StageCurrencyRewardVFX currencyRewardVfx;

    public async Task<bool> GiveWaveRewardAsync(WaveData waveData)
    {
        if (waveData == null)
            return false;

        if (!economyManager.TryGetWaveRewardBreakdown(
                waveData.waveType,
                out int waveReward,
                out int bonusReward))
        {
            Debug.LogError("Failed to calculate wave gold reward.", this);
            return false;
        }

        if (currencyRewardVfx != null && waveReward > 0)
            await currencyRewardVfx.PlayWaveRewardAsync(waveReward);

        if (!economyManager.TryAddGold(waveReward))
        {
            Debug.LogError("Failed to apply wave clear reward.", this);
            return false;
        }

        if (bonusReward <= 0)
            return true;

        if (currencyRewardVfx != null)
            await currencyRewardVfx.PlayBonusRewardAsync(bonusReward);

        if (!economyManager.TryAddGold(bonusReward))
        {
            Debug.LogError("Failed to apply interest bonus reward.", this);
            return false;
        }

        return true;
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
