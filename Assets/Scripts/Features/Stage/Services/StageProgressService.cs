using System.Threading.Tasks;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class StageProgressService : MonoBehaviour
{
    private IStageUserDataGateway userDataGateway;

    public bool Configure(IStageUserDataGateway gateway)
    {
        if (gateway?.UserData?.Progress == null || !gateway.IsLoaded)
        {
            Debug.LogError("[StageProgressService] Loaded user progress is required.", this);
            return false;
        }

        userDataGateway = gateway;
        return true;
    }

    public async Task<bool> RecordWaveClearAsync(StageDataSO stage, int clearedWaveCount)
    {
        if (!TryGetCurrentProgress(stage, out UserProgressData currentProgress))
            return false;

        UserProgressData nextProgress;

        try
        {
            nextProgress = StageProgressRules.RecordClearedWave(
                currentProgress,
                stage.sector,
                stage.stage,
                clearedWaveCount);
        }
        catch (System.Exception exception)
        {
            Debug.LogError($"RecordWaveClearAsync failed: {exception.Message}");
            return false;
        }

        if (nextProgress.BestWaveCleared == currentProgress.BestWaveCleared)
            return true;

        return await userDataGateway.SaveUserProgressAsync(nextProgress);
    }

    private bool TryGetCurrentProgress(StageDataSO stage, out UserProgressData progress)
    {
        progress = null;

        if (stage == null)
        {
            Debug.LogError("Stage progress update failed. StageDataSO is null.");
            return false;
        }

        progress = userDataGateway?.UserData?.Progress;

        if (progress == null)
        {
            Debug.LogError("Stage progress update failed. UserData or Progress is null.");
            return false;
        }

        if (StageProgressRules.IsCurrentStage(progress, stage.sector, stage.stage))
            return true;

        Debug.LogWarning(
            $"Stage progress update ignored. Current: {progress.CurrentSector}-{progress.CurrentStage}, " +
            $"Requested: {stage.StageKey}");
        return false;
    }
}
