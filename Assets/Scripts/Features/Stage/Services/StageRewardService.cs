using System.Threading.Tasks;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class StageRewardService : MonoBehaviour
{
    [SerializeField] private EconomyManager economyManager;
    [SerializeField] private StageCurrencyRewardVFX currencyRewardVfx;
    private int fusionCount;
    private int fourStarFusionCount;
    private IStageUserDataGateway userDataGateway;

    public bool Configure(IStageUserDataGateway gateway)
    {
        if (gateway?.UserData == null || !gateway.IsLoaded)
        {
            Debug.LogError("[StageRewardService] Loaded user data manager is required.", this);
            return false;
        }

        userDataGateway = gateway;
        return true;
    }

    public void SetQuestSessionStats(int fusions, int fourStarFusions)
    {
        fusionCount = Mathf.Max(0, fusions);
        fourStarFusionCount = Mathf.Max(0, fourStarFusions);
    }

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
        return userDataGateway != null
            ? userDataGateway.CompleteStageAsync(stageData, fusionCount, fourStarFusionCount)
            : Task.FromResult(StageOutcomeResult.Fail(StageOutcomeFailure.InvalidRequest));
    }

    public Task<StageOutcomeResult> GiveStageFailRewardAsync(
        StageDataSO stageData,
        int clearedWaveCount,
        int entryFuelCost)
    {
        return userDataGateway != null
            ? userDataGateway.FailStageAsync(
                stageData,
                clearedWaveCount,
                entryFuelCost,
                fusionCount,
                fourStarFusionCount)
            : Task.FromResult(StageOutcomeResult.Fail(StageOutcomeFailure.InvalidRequest));
    }
}
