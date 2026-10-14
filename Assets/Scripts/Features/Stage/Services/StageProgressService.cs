using System.Threading.Tasks;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class StageProgressService : MonoBehaviour
{
    private UserDataManager userDataManager;

    public bool Configure(UserDataManager manager)
    {
        if (manager?.UserData?.Progress == null || !manager.IsLoaded)
        {
            Debug.LogError("[StageProgressService] Loaded user progress is required.", this);
            return false;
        }

        userDataManager = manager;
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

        return await userDataManager.SaveUserProgressAsync(nextProgress);
    }

    public async Task<bool> ApplyStageClearAsync(StageDataSO clearedStage)
    {
        if (!TryGetCurrentProgress(clearedStage, out UserProgressData currentProgress))
            return false;

        UserProgressData nextProgress;

        try
        {
            nextProgress = StageProgressRules.AdvanceAfterStageClear(
                currentProgress,
                clearedStage.sector,
                clearedStage.stage,
                GameConfig.Stages);
        }
        catch (System.Exception exception)
        {
            Debug.LogError($"ApplyStageClearAsync failed: {exception.Message}");
            return false;
        }

        return await userDataManager.SaveUserProgressAsync(nextProgress);
    }

    public Task<bool> ApplyStageFailAsync(StageDataSO failedStage, int clearedWaveCount)
    {
        return RecordWaveClearAsync(failedStage, clearedWaveCount);
    }

    private bool TryGetCurrentProgress(StageDataSO stage, out UserProgressData progress)
    {
        progress = null;

        if (stage == null)
        {
            Debug.LogError("Stage progress update failed. StageDataSO is null.");
            return false;
        }

        progress = userDataManager?.UserData?.Progress;

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
