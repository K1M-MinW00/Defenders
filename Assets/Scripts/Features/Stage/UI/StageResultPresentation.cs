using System.Collections.Generic;

public readonly struct StageResultPresentation
{
    public bool IsClear { get; }
    public string StageLabel { get; }
    public int ClearedWaveCount { get; }
    public int TotalWaveCount { get; }
    public IReadOnlyList<RewardData> Rewards { get; }

    public StageResultPresentation(bool isClear, string stageLabel, int clearedWaveCount, int totalWaveCount, IReadOnlyList<RewardData> rewards)
    {
        IsClear = isClear;
        StageLabel = stageLabel;
        ClearedWaveCount = clearedWaveCount;
        TotalWaveCount = totalWaveCount;
        Rewards = rewards ?? System.Array.Empty<RewardData>();
    }
}
