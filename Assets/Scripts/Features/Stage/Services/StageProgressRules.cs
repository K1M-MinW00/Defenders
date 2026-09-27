using System;

public static class StageProgressRules
{
    public const int StagesPerSector = 10;

    public static bool IsCurrentStage(UserProgressData progress, int sector, int stage)
    {
        return progress != null &&
               progress.CurrentSector == sector &&
               progress.CurrentStage == stage;
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
        };
    }

    public static UserProgressData AdvanceAfterStageClear(
        UserProgressData current,
        int sector,
        int stage)
    {
        ValidateCurrentStage(current, sector, stage);

        int nextSector = current.CurrentSector;
        int nextStage = current.CurrentStage + 1;

        if (nextStage > StagesPerSector)
        {
            nextSector++;
            nextStage = 1;
        }

        return new UserProgressData
        {
            CurrentSector = nextSector,
            CurrentStage = nextStage,
            BestWaveCleared = 0,
        };
    }

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
