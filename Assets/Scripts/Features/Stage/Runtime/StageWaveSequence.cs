using System;
using System.Collections.Generic;

public sealed class StageWaveSequence
{
    private readonly IReadOnlyList<WaveData> waves;

    public int CurrentIndex { get; private set; }
    public int Count => waves.Count;
    public WaveData Current => waves[CurrentIndex];
    public int ClearedWaveCount => CurrentIndex + 1;
    public bool IsFinalWave => CurrentIndex == waves.Count - 1;

    public StageWaveSequence(IReadOnlyList<WaveData> waves)
    {
        this.waves = waves ?? throw new ArgumentNullException(nameof(waves));
        if (waves.Count == 0)
            throw new ArgumentException("A stage must contain at least one wave.", nameof(waves));

        CurrentIndex = 0;
    }

    public bool TryMoveNext()
    {
        if (IsFinalWave)
            return false;

        CurrentIndex++;
        return true;
    }
}
