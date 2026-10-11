using System.Collections.Generic;

public static class StageResultPresenter
{
    public static StageResultPresentation Create(bool isClear, StageDataSO stage, int clearedWaveCount, IReadOnlyList<RewardData> rewards)
    {
        int totalWaveCount = stage?.waves?.Count ?? 0;
        int safeClearedCount = System.Math.Clamp(clearedWaveCount, 0, totalWaveCount);
        string stageLabel = stage != null ? $"스테이지 {stage.StageKey}" : "스테이지";
        return new StageResultPresentation(isClear, stageLabel, safeClearedCount, totalWaveCount, rewards);
    }
}
