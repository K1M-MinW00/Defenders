using System.Collections.Generic;
using System.Linq;

public static class WaveMilestoneRewardRules
{
    public static bool IsUnlocked(UserProgressData progress, int sector, int stage, int requiredWave)
    {
        if (progress == null || requiredWave < 1)
            return false;

        int comparison = Compare(sector, stage, progress.CurrentSector, progress.CurrentStage);
        return comparison < 0 || comparison == 0 && progress.BestWaveCleared >= requiredWave;
    }

    public static bool IsClaimed(UserWaveRewardProgressData progress, int requiredWave) =>
        progress?.ClaimedWaves?.Contains(requiredWave) == true;

    public static bool HasClaimableReward(UserProgressData progress, IStageCatalog catalog)
    {
        StageDataSO stage = ResolveRewardStage(progress, catalog);
        if (stage == null)
            return false;

        return stage.milestoneRewards.Any(milestone =>
            milestone != null &&
            IsUnlocked(progress, stage.sector, stage.stage, milestone.requiredWave) &&
            !IsClaimed(progress.WaveRewardProgress, milestone.requiredWave));
    }

    public static StageDataSO ResolveRewardStage(UserProgressData progress, IStageCatalog catalog)
    {
        if (progress?.WaveRewardProgress == null || catalog == null)
            return null;

        UserWaveRewardProgressData cursor = progress.WaveRewardProgress;
        return catalog.GetAll()
            .Where(stage => stage != null && stage.milestoneRewards?.Count > 0)
            .OrderBy(stage => stage.sector).ThenBy(stage => stage.stage)
            .FirstOrDefault(stage => Compare(stage.sector, stage.stage, cursor.Sector, cursor.Stage) >= 0);
    }

    public static StageDataSO GetNextRewardStage(StageDataSO current, IStageCatalog catalog)
    {
        if (current == null || catalog == null)
            return null;

        return catalog.GetAll()
            .Where(stage => stage != null && stage.milestoneRewards?.Count > 0 &&
                Compare(stage.sector, stage.stage, current.sector, current.stage) > 0)
            .OrderBy(stage => stage.sector).ThenBy(stage => stage.stage)
            .FirstOrDefault();
    }

    public static int Compare(int leftSector, int leftStage, int rightSector, int rightStage)
    {
        int sectorComparison = leftSector.CompareTo(rightSector);
        return sectorComparison != 0 ? sectorComparison : leftStage.CompareTo(rightStage);
    }
}
