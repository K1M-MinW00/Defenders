using System;
using System.Linq;

public static class StageProgressRules
{
    public const int StagesPerSector = 10;

    public static bool IsCurrentStage(UserProgressData progress, int sector, int stage)
    {
        return progress != null &&
               progress.CurrentSector == sector &&
               progress.CurrentStage == stage;
    }

    public static StageDataSO ResolveAvailableStage(IStageCatalog catalog, int sector, int stage)
    {
        if (catalog == null) return null;
        StageDataSO exact = catalog.Get(sector, stage);
        if (exact != null) return exact;
        StageDataSO[] ordered = catalog.GetAll().Where(candidate => candidate != null)
            .OrderBy(candidate => candidate.sector).ThenBy(candidate => candidate.stage).ToArray();
        return ordered.FirstOrDefault(candidate => candidate.sector > sector || candidate.sector == sector && candidate.stage > stage)
               ?? ordered.LastOrDefault();
    }

    public static UserProgressData RecordClearedWave(
        UserProgressData current,
        int sector,
        int stage,
        int clearedWaveCount)
    {
        ValidateCurrentStage(current, sector, stage);

        if (clearedWaveCount < 0)
            throw new ArgumentOutOfRangeException(nameof(clearedWaveCount));

        return new UserProgressData
        {
            CurrentSector = current.CurrentSector,
            CurrentStage = current.CurrentStage,
            BestWaveCleared = Math.Max(current.BestWaveCleared, clearedWaveCount),
            WaveRewardProgress = CopyWaveRewardProgress(current.WaveRewardProgress),
        };
    }

    public static UserProgressData AdvanceAfterStageClear(
        UserProgressData current,
        int sector,
        int stage)
    {
        return AdvanceAfterStageClear(current, sector, stage, null);
    }

    public static UserProgressData AdvanceAfterStageClear(
        UserProgressData current,
        int sector,
        int stage,
        IStageCatalog catalog)
    {
        ValidateCurrentStage(current, sector, stage);

        int nextSector;
        int nextStage;
        if (catalog != null)
        {
            StageDataSO next = catalog.GetAll().Where(candidate => candidate != null &&
                    (candidate.sector > sector || candidate.sector == sector && candidate.stage > stage))
                .OrderBy(candidate => candidate.sector).ThenBy(candidate => candidate.stage).FirstOrDefault();
            nextSector = next?.sector ?? current.CurrentSector;
            nextStage = next?.stage ?? current.CurrentStage;
        }
        else
        {
            nextSector = current.CurrentSector;
            nextStage = current.CurrentStage + 1;
            if (nextStage > StagesPerSector)
            {
                nextSector++;
                nextStage = 1;
            }
        }

        return new UserProgressData
        {
            CurrentSector = nextSector,
            CurrentStage = nextStage,
            BestWaveCleared = 0,
            WaveRewardProgress = CopyWaveRewardProgress(current.WaveRewardProgress),
        };
    }

    private static UserWaveRewardProgressData CopyWaveRewardProgress(UserWaveRewardProgressData source) => new()
    {
        Sector = source?.Sector ?? 1,
        Stage = source?.Stage ?? 1,
        ClaimedWaves = source?.ClaimedWaves == null
            ? new System.Collections.Generic.List<int>()
            : new System.Collections.Generic.List<int>(source.ClaimedWaves),
    };

    private static void ValidateCurrentStage(UserProgressData current, int sector, int stage)
    {
        if (current == null)
            throw new ArgumentNullException(nameof(current));

        if (!IsCurrentStage(current, sector, stage))
        {
            throw new InvalidOperationException(
                $"Progress stage mismatch. Current: {current.CurrentSector}-{current.CurrentStage}, " +
                $"Requested: {sector}-{stage}.");
        }
    }
}
