using System.Collections.Generic;
using NUnit.Framework;

public sealed class StageUnitSelectionTests
{
    [Test]
    public void ScriptedSelector_ReturnsConfiguredOrder()
    {
        UnitDataSO unitA = UnityEngine.ScriptableObject.CreateInstance<UnitDataSO>();
        UnitDataSO unitB = UnityEngine.ScriptableObject.CreateInstance<UnitDataSO>();
        unitA.unitId = "A";
        unitB.unitId = "B";
        StageUnitInitData[] formation =
        {
            new(unitA, new UserUnitData { UnitId = "A" }),
            new(unitB, new UserUnitData { UnitId = "B" }),
        };
        ScriptedStageUnitSelector selector = new(new[] { "A", "B", "A" });

        Assert.That(selector.Select(formation).UnitData.unitId, Is.EqualTo("A"));
        Assert.That(selector.Select(formation).UnitData.unitId, Is.EqualTo("B"));
        Assert.That(selector.Select(formation).UnitData.unitId, Is.EqualTo("A"));
        Assert.That(selector.Select(formation), Is.Null);
    }

    [Test]
    public void ScriptedSelector_DoesNotConsumeMissingUnit()
    {
        UnitDataSO unitA = UnityEngine.ScriptableObject.CreateInstance<UnitDataSO>();
        unitA.unitId = "A";
        StageUnitInitData[] formation =
        {
            new(unitA, new UserUnitData { UnitId = "A" }),
        };
        ScriptedStageUnitSelector selector = new(new[] { "B" });

        Assert.That(selector.Select(formation), Is.Null);
        Assert.That(selector.RemainingCount, Is.EqualTo(1));
    }

    [Test]
    public void StepGuidedSelector_FixesOnlyFirstSelectionForCurrentStep()
    {
        UnitDataSO unitA = UnityEngine.ScriptableObject.CreateInstance<UnitDataSO>();
        UnitDataSO unitB = UnityEngine.ScriptableObject.CreateInstance<UnitDataSO>();
        unitA.unitId = "A";
        unitB.unitId = "B";
        StageUnitInitData[] formation =
        {
            new(unitA, new UserUnitData { UnitId = "A" }),
            new(unitB, new UserUnitData { UnitId = "B" }),
        };
        string step = "summon";
        StepGuidedStageUnitSelector selector = new(
            () => step,
            new Dictionary<string, string> { ["summon"] = "A" },
            new FixedSelector("B"));

        Assert.That(selector.Select(formation).UnitData.unitId, Is.EqualTo("A"));
        Assert.That(selector.Select(formation).UnitData.unitId, Is.EqualTo("B"));

        step = "other";
        Assert.That(selector.Select(formation).UnitData.unitId, Is.EqualTo("B"));
    }

    private sealed class FixedSelector : IStageUnitSelector
    {
        private readonly string unitId;

        public FixedSelector(string unitId) => this.unitId = unitId;

        public StageUnitInitData Select(IReadOnlyList<StageUnitInitData> formation)
        {
            foreach (StageUnitInitData unit in formation)
            {
                if (unit.UnitData.unitId == unitId)
                    return unit;
            }

            return null;
        }
    }
}
