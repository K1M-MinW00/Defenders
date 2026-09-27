using System;
using System.Collections.Generic;

public sealed class StageCatalog : IStageCatalog
{
    private readonly Dictionary<string, StageDataSO> stagesByKey;

    public StageCatalog(IEnumerable<StageDataSO> stages)
    {
        if (stages == null)
            throw new ArgumentNullException(nameof(stages));

        stagesByKey = new Dictionary<string, StageDataSO>(StringComparer.Ordinal);
        foreach (StageDataSO stage in stages)
        {
            if (stage == null)
                throw new ArgumentException("Stage catalog contains a null asset.", nameof(stages));
            if (stage.sector < 1 || stage.stage < 1)
                throw new ArgumentException($"Invalid stage ID: {stage.name}", nameof(stages));
            if (!stagesByKey.TryAdd(stage.StageKey, stage))
                throw new ArgumentException($"Duplicate stage ID: {stage.StageKey}", nameof(stages));
        }

        if (stagesByKey.Count == 0)
            throw new ArgumentException("Stage catalog is empty.", nameof(stages));
    }

    public StageDataSO Get(int sector, int stage)
    {
        if (sector < 1 || stage < 1)
            return null;

        stagesByKey.TryGetValue($"{sector}-{stage}", out StageDataSO result);
        return result;
    }

    public IReadOnlyCollection<StageDataSO> GetAll() => stagesByKey.Values;
}
