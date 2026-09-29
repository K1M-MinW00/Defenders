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
