using System.Linq;
using NUnit.Framework;
using UnityEngine;

public sealed class UnitBalanceDataTests
{
    [Test]
    public void UnitDefinitions_HaveValidAndMonotonicGrowthData()
    {
        UnitDataSO[] units = Resources.LoadAll<UnitDataSO>("GameData/Units");
        Assert.That(units, Is.Not.Empty);

        foreach (UnitDataSO unit in units)
        {
            Assert.That(unit.baseStats.Attack, Is.GreaterThan(0f), $"{unit.name}: Attack");
            Assert.That(unit.baseStats.MaxHp, Is.GreaterThan(0f), $"{unit.name}: MaxHp");
            Assert.That(unit.baseStats.AttackPerSec, Is.GreaterThan(0f), $"{unit.name}: AttackPerSec");
            Assert.That(unit.baseStats.DetectRange, Is.GreaterThan(0f), $"{unit.name}: DetectRange");
            Assert.That(unit.baseStats.EnergyRecovery, Is.GreaterThan(0f), $"{unit.name}: EnergyRecovery");
            Assert.That(unit.attackGrowthValue, Is.GreaterThanOrEqualTo(0), $"{unit.name}: attack growth");
            Assert.That(unit.hpGrowthValue, Is.GreaterThanOrEqualTo(0), $"{unit.name}: hp growth");

            AssertMonotonic(unit.name, "Attack", unit.starAttackMultipliers);
            AssertMonotonic(unit.name, "HP", unit.starHpMultipliers);
            AssertMonotonic(unit.name, "Range", unit.starDetectRangeMultipliers);
            AssertMonotonic(unit.name, "Attack speed", unit.starAttackPerSecondMultipliers);

            Assert.That(unit.limitBreaks, Has.Count.EqualTo(UnitLimitBreakUseCase.MaxLimitBreak),
                $"{unit.name}: limit break count");
            Assert.That(unit.limitBreaks.All(x => x != null && x.value > 0f), Is.True,
                $"{unit.name}: limit break values");
        }
    }

    private static void AssertMonotonic(string unitName, string statName, float[] values)
    {
        Assert.That(values, Has.Length.EqualTo(4), $"{unitName}: {statName} star table");
        Assert.That(values[0], Is.EqualTo(1f).Within(0.001f), $"{unitName}: {statName} 1-star");

        for (int i = 1; i < values.Length; i++)
        {
            Assert.That(values[i], Is.GreaterThanOrEqualTo(values[i - 1]),
                $"{unitName}: {statName} decreases at star {i + 1}");
        }
    }
}
