using NUnit.Framework;
using UnityEngine;

public sealed class CombatStatisticsTrackerTests
{
    [TestCase(0f, "0")]
    [TestCase(999f, "999")]
    [TestCase(1000f, "1K")]
    [TestCase(1200f, "1.2K")]
    [TestCase(1250f, "1.3K")]
    [TestCase(10000f, "10K")]
    public void RowValue_UsesCompactThousandsFormat(float value, string expected)
    {
        Assert.That(StageCombatStatisticsRowUI.FormatValue(value), Is.EqualTo(expected));
    }

    private CombatStatisticsTracker tracker;
    private GameObject firstObject;
    private GameObject secondObject;

    [SetUp]
    public void SetUp()
    {
        tracker = new CombatStatisticsTracker();
        tracker.Initialize(null);
        tracker.BeginCombat();
        firstObject = new GameObject("First Unit");
        secondObject = new GameObject("Second Unit");
    }

    [TearDown]
    public void TearDown()
    {
        tracker.Dispose();
        Object.DestroyImmediate(firstObject);
        Object.DestroyImmediate(secondObject);
    }

    [Test]
    public void GetTop_SortsEachStatisticIndependently()
    {
        UnitController first = firstObject.AddComponent<UnitController>();
        UnitController second = secondObject.AddComponent<UnitController>();

        CombatStatisticsEvents.ReportDamageDealt(first, 30f);
        CombatStatisticsEvents.ReportDamageDealt(second, 80f);
        CombatStatisticsEvents.ReportHealingDone(first, 50f);
        CombatStatisticsEvents.ReportHealingDone(second, 10f);
        CombatStatisticsEvents.ReportDamageTaken(first, 5f);
        CombatStatisticsEvents.ReportDamageTaken(second, 25f);

        Assert.That(tracker.GetTop(CombatStatisticType.DamageDealt, 3)[0].Unit, Is.SameAs(second));
        Assert.That(tracker.GetTop(CombatStatisticType.HealingDone, 3)[0].Unit, Is.SameAs(first));
        Assert.That(tracker.GetTop(CombatStatisticType.DamageTaken, 3)[0].Unit, Is.SameAs(second));
        Assert.That(tracker.GetMaximum(CombatStatisticType.DamageDealt), Is.EqualTo(80f));
        Assert.That(tracker.GetMaximum(CombatStatisticType.HealingDone), Is.EqualTo(50f));
        Assert.That(tracker.GetMaximum(CombatStatisticType.DamageTaken), Is.EqualTo(25f));
    }

    [Test]
    public void BeginCombat_ClearsPreviousWaveValues()
    {
        UnitController unit = firstObject.AddComponent<UnitController>();
        CombatStatisticsEvents.ReportDamageDealt(unit, 100f);
        Assert.That(tracker.GetTop(CombatStatisticType.DamageDealt, 3), Has.Count.EqualTo(1));

        tracker.BeginCombat();

        Assert.That(tracker.GetTop(CombatStatisticType.DamageDealt, 3), Is.Empty);
    }

    [Test]
    public void GetTop_EqualValues_DoesNotRequireInitializedUnitRuntime()
    {
        UnitController first = firstObject.AddComponent<UnitController>();
        UnitController second = secondObject.AddComponent<UnitController>();

        CombatStatisticsEvents.ReportDamageDealt(first, 10f);
        CombatStatisticsEvents.ReportDamageDealt(second, 10f);

        Assert.That(
            () => tracker.GetTop(CombatStatisticType.DamageDealt, 3),
            Throws.Nothing);
        Assert.That(tracker.GetTop(CombatStatisticType.DamageDealt, 3), Has.Count.EqualTo(2));
    }
}
