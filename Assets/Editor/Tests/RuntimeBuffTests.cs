using NUnit.Framework;

public sealed class RuntimeBuffTests
{
    [TestCase(BuffDurationType.Timed)]
    [TestCase(BuffDurationType.UntilWaveEnd)]
    public void CompleteWave_RemovesCombatScopedBuffs(BuffDurationType durationType)
    {
        RuntimeBuff buff = Create(durationType);

        Assert.That(buff.CompleteWave(), Is.True);
    }

    [Test]
    public void CompleteWave_PreservesStageBuff()
    {
        RuntimeBuff buff = Create(BuffDurationType.UntilStageEnd);

        Assert.That(buff.CompleteWave(), Is.False);
    }

    [Test]
    public void CompleteWave_DecrementsWaveCountBeforeRemoval()
    {
        RuntimeBuff buff = new(
            "wave",
            StatType.Attack,
            BuffModifyType.Flat,
            1f,
            BuffDurationType.WaveCount,
            remainingWaves: 2);

        Assert.That(buff.CompleteWave(), Is.False);
        Assert.That(buff.RemainingWaves, Is.EqualTo(1));
        Assert.That(buff.CompleteWave(), Is.True);
        Assert.That(buff.RemainingWaves, Is.Zero);
    }

    [Test]
    public void RefreshFrom_UpdatesValueAndRestoresDuration()
    {
        RuntimeBuff buff = Create(BuffDurationType.Timed);
        buff.Tick(0.75f);

        bool statChanged = buff.RefreshFrom(new BuffApplication(
            "test",
            StatType.Attack,
            BuffModifyType.Flat,
            2f,
            BuffDurationType.Timed,
            durationSeconds: 3f));

        Assert.That(statChanged, Is.True);
        Assert.That(buff.Value, Is.EqualTo(2f));
        Assert.That(buff.RemainingTime, Is.EqualTo(3f));
    }

    [Test]
    public void RefreshFrom_SameValueRestoresDurationWithoutStatChange()
    {
        RuntimeBuff buff = Create(BuffDurationType.Timed);
        buff.Tick(0.75f);

        bool statChanged = buff.RefreshFrom(new BuffApplication(
            "test",
            StatType.Attack,
            BuffModifyType.Flat,
            1f,
            BuffDurationType.Timed,
            durationSeconds: 2f));

        Assert.That(statChanged, Is.False);
        Assert.That(buff.RemainingTime, Is.EqualTo(2f));
    }

    [Test]
    public void RefreshFrom_RejectsChangedDefinition()
    {
        RuntimeBuff buff = Create(BuffDurationType.Timed);

        bool refreshed = buff.RefreshFrom(new BuffApplication(
            "test",
            StatType.MaxHp,
            BuffModifyType.Flat,
            2f,
            BuffDurationType.Timed,
            durationSeconds: 3f));

        Assert.That(refreshed, Is.False);
        Assert.That(buff.StatType, Is.EqualTo(StatType.Attack));
        Assert.That(buff.Value, Is.EqualTo(1f));
    }

    private static RuntimeBuff Create(BuffDurationType durationType)
    {
        return new RuntimeBuff(
            "test",
            StatType.Attack,
            BuffModifyType.Flat,
            1f,
            durationType,
            durationSeconds: 1f,
            remainingWaves: 1);
    }
}
