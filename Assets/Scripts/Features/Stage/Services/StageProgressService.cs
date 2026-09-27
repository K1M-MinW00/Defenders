using System.Threading.Tasks;
using UnityEngine;

public class StageProgressService : MonoBehaviour
{
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

        return await UserDataManager.Instance.SaveUserProgressAsync(nextProgress);
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
                clearedStage.stage);
        }
        catch (System.Exception exception)
        {
            Debug.LogError($"ApplyStageClearAsync failed: {exception.Message}");
            return false;
        }

        return await UserDataManager.Instance.SaveUserProgressAsync(nextProgress);
    }

    public Task<bool> ApplyStageFailAsync(StageDataSO failedStage, int clearedWaveCount)
    {
        return RecordWaveClearAsync(failedStage, clearedWaveCount);
    }

    private static bool TryGetCurrentProgress(StageDataSO stage, out UserProgressData progress)
    {
        progress = null;

        if (stage == null)
        {
            Debug.LogError("Stage progress update failed. StageDataSO is null.");
            return false;
        }

        UserDataManager manager = UserDataManager.Instance;
        UserDataRoot userData = manager != null ? manager.UserData : null;
        progress = userData?.Progress;

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
