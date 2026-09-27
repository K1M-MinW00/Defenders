using System;
using System.Collections.Generic;
using NUnit.Framework;

public sealed class StageWaveSequenceTests
{
    [Test]
    public void NewSequence_StartsAtFirstWave()
    {
        WaveData first = new();
        WaveData second = new();
        StageWaveSequence sequence = new(new List<WaveData> { first, second });

        Assert.That(sequence.CurrentIndex, Is.Zero);
        Assert.That(sequence.Current, Is.SameAs(first));
        Assert.That(sequence.ClearedWaveCount, Is.EqualTo(1));
        Assert.That(sequence.IsFinalWave, Is.False);
    }

    [Test]
    public void MoveNext_AdvancesUntilFinalWave()
    {
        WaveData first = new();
        WaveData second = new();
        StageWaveSequence sequence = new(new List<WaveData> { first, second });

        Assert.That(sequence.TryMoveNext(), Is.True);
        Assert.That(sequence.CurrentIndex, Is.EqualTo(1));
        Assert.That(sequence.Current, Is.SameAs(second));
        Assert.That(sequence.IsFinalWave, Is.True);
        Assert.That(sequence.TryMoveNext(), Is.False);
        Assert.That(sequence.CurrentIndex, Is.EqualTo(1));
    }

    [Test]
    public void Constructor_RejectsMissingWaves()
    {
        Assert.Throws<ArgumentNullException>(() => new StageWaveSequence(null));
        Assert.Throws<ArgumentException>(() => new StageWaveSequence(Array.Empty<WaveData>()));
    }
}
