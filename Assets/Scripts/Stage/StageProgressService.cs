using System.Threading.Tasks;
using UnityEngine;

public class StageProgressService : MonoBehaviour
{
    [SerializeField] private int stagesPerSector = 5;

    public async Task ApplyStageClearAsync(StageDataSO clearedStage)
    {
        if (clearedStage == null)
        {
            Debug.LogError("ApplyStageClearAsync failed. StageDataSO is null.");
            return;
        }

        UserDataRoot userData = UserDataManager.Instance.Data;

        if (userData == null || userData.Progress == null)
        {
            Debug.LogError("ApplyStageClearAsync failed. UserData or Progress is null.");
            return;
        }

        UserProgressData progress = userData.Progress;

        // 현재 진행 중인 스테이지와 방금 클리어한 스테이지가 일치할 때만 전진
        if (progress.CurrentSector != clearedStage.sector ||
            progress.CurrentStage != clearedStage.stage)
        {
            Debug.LogWarning(
                $"Stage clear ignored. Current: {progress.CurrentSector}-{progress.CurrentStage}, Cleared: {clearedStage.StageKey}"
            );
            return;
        }

        AdvanceProgress(progress);

        await UserDataManager.Instance.SaveUserProgressAsync(progress);
    }

    public Task ApplyStageFailAsync(StageDataSO failedStage, int clearedWaveCount)
    {
        // 현재 기획에서는 실패 시 진행도 저장 없음
        return Task.CompletedTask;
    }

    private void AdvanceProgress(UserProgressData progress)
    {
        if (progress.CurrentStage < stagesPerSector)
        {
            progress.CurrentStage++;
        }
        else
        {
            progress.CurrentSector++;
            progress.CurrentStage = 1;
        }
    }
}