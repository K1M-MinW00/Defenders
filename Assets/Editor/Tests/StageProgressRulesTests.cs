using NUnit.Framework;

public sealed class StageProgressRulesTests
{
    [Test]
    public void RecordClearedWave_KeepsHighestRecord()
    {
        UserProgressData current = CreateProgress(1, 3, 5);

        UserProgressData result = StageProgressRules.RecordClearedWave(current, 1, 3, 3);

        Assert.That(result.BestWaveCleared, Is.EqualTo(5));
        Assert.That(current.BestWaveCleared, Is.EqualTo(5));
    }

    [Test]
    public void RecordClearedWave_UpdatesHigherRecord()
    {
        UserProgressData current = CreateProgress(1, 3, 5);

        UserProgressData result = StageProgressRules.RecordClearedWave(current, 1, 3, 6);

        Assert.That(result.BestWaveCleared, Is.EqualTo(6));
    }

    [Test]
    public void AdvanceAfterStageClear_AdvancesStageAndResetsWaveRecord()
    {
        UserProgressData current = CreateProgress(1, 3, 5);

        UserProgressData result = StageProgressRules.AdvanceAfterStageClear(current, 1, 3);

        Assert.That(result.CurrentSector, Is.EqualTo(1));
        Assert.That(result.CurrentStage, Is.EqualTo(4));
        Assert.That(result.BestWaveCleared, Is.Zero);
    }

    [Test]
    public void AdvanceAfterSectorFinalStage_AdvancesSector()
    {
        UserProgressData current = CreateProgress(1, StageProgressRules.StagesPerSector, 9);

        UserProgressData result = StageProgressRules.AdvanceAfterStageClear(
            current,
            1,
            StageProgressRules.StagesPerSector);

        Assert.That(result.CurrentSector, Is.EqualTo(2));
        Assert.That(result.CurrentStage, Is.EqualTo(1));
        Assert.That(result.BestWaveCleared, Is.Zero);
    }

    [Test]
    public void ProgressUpdate_RejectsDifferentStage()
    {
        UserProgressData current = CreateProgress(1, 3, 5);

        Assert.Throws<System.InvalidOperationException>(
            () => StageProgressRules.RecordClearedWave(current, 1, 2, 6));
    }

    private static UserProgressData CreateProgress(int sector, int stage, int bestWaveCleared)
    {
        return new UserProgressData
        {
            CurrentSector = sector,
            CurrentStage = stage,
            BestWaveCleared = bestWaveCleared,
        };
    }
}
