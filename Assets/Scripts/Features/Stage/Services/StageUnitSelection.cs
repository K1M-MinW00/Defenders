using System;
using System.Collections.Generic;

public interface IStageUnitSelector
{
    StageUnitInitData Select(IReadOnlyList<StageUnitInitData> formation);
}

public sealed class RandomStageUnitSelector : IStageUnitSelector
{
    public StageUnitInitData Select(IReadOnlyList<StageUnitInitData> formation)
    {
        if (formation == null || formation.Count == 0)
            return null;

        return formation[UnityEngine.Random.Range(0, formation.Count)];
    }
}

public sealed class ScriptedStageUnitSelector : IStageUnitSelector
{
    private readonly Queue<string> unitIds;

    public ScriptedStageUnitSelector(IEnumerable<string> unitIds)
    {
        if (unitIds == null)
            throw new ArgumentNullException(nameof(unitIds));

        this.unitIds = new Queue<string>();
        foreach (string unitId in unitIds)
        {
            if (string.IsNullOrWhiteSpace(unitId))
                throw new ArgumentException("Scripted unit IDs cannot contain an empty value.", nameof(unitIds));

            this.unitIds.Enqueue(unitId);
        }

        if (this.unitIds.Count == 0)
            throw new ArgumentException("At least one scripted unit ID is required.", nameof(unitIds));
    }

    public int RemainingCount => unitIds.Count;

    public StageUnitInitData Select(IReadOnlyList<StageUnitInitData> formation)
    {
        if (formation == null || formation.Count == 0 || unitIds.Count == 0)
            return null;

        string nextUnitId = unitIds.Peek();
        for (int i = 0; i < formation.Count; i++)
        {
            StageUnitInitData candidate = formation[i];
            if (candidate?.UnitData?.unitId != nextUnitId)
                continue;

            unitIds.Dequeue();
            return candidate;
        }

        return null;
    }
}

public sealed class StepGuidedStageUnitSelector : IStageUnitSelector
{
    private readonly Func<string> currentStep;
    private readonly IReadOnlyDictionary<string, string> guidedUnitIds;
    private readonly IStageUnitSelector fallback;
    private string servedStep;

    public StepGuidedStageUnitSelector(
        Func<string> currentStep,
        IReadOnlyDictionary<string, string> guidedUnitIds,
        IStageUnitSelector fallback = null)
    {
        this.currentStep = currentStep ?? throw new ArgumentNullException(nameof(currentStep));
        this.guidedUnitIds = guidedUnitIds ?? throw new ArgumentNullException(nameof(guidedUnitIds));
        this.fallback = fallback ?? new RandomStageUnitSelector();
    }

    public StageUnitInitData Select(IReadOnlyList<StageUnitInitData> formation)
    {
        if (formation == null || formation.Count == 0)
            return null;

        string step = currentStep();
        if (!string.Equals(step, servedStep, StringComparison.Ordinal) &&
            guidedUnitIds.TryGetValue(step ?? string.Empty, out string unitId))
        {
            for (int i = 0; i < formation.Count; i++)
            {
                StageUnitInitData candidate = formation[i];
                if (candidate?.UnitData?.unitId != unitId)
                    continue;

                servedStep = step;
                return candidate;
            }
        }

        return fallback.Select(formation);
    }
}
