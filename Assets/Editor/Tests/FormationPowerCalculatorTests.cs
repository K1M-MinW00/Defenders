using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public sealed class FormationPowerCalculatorTests
{
    [Test]
    public void Calculate_UsesOnlySelectedOwnedUnits()
    {
        UnitDataSO selected = CreateUnit("selected", attack: 20f, hp: 200f);
        UnitDataSO unselected = CreateUnit("unselected", attack: 100f, hp: 1000f);
        try
        {
            UnitCatalog catalog = new(new[] { selected, unselected });
            UserRosterData roster = new()
            {
                OwnedUnits = new List<UserUnitData>
                {
                    new() { UnitId = "selected", Level = 1 },
                    new() { UnitId = "unselected", Level = 1 },
                },
                SelectedUnitIds = new List<string> { "selected" },
            };

            Assert.That(
                FormationPowerCalculator.Calculate(roster, catalog),
                Is.EqualTo(FormationPowerCalculator.CalculateUnit(selected, roster.OwnedUnits[0])));
        }
        finally
        {
            Object.DestroyImmediate(selected);
            Object.DestroyImmediate(unselected);
        }
    }

    [Test]
    public void Calculate_IncreasesWhenSelectedUnitLevelIncreases()
    {
        UnitDataSO definition = CreateUnit("unit", attack: 20f, hp: 200f);
        definition.attackGrowthValue = 2;
        definition.hpGrowthValue = 10;
        try
        {
            UserUnitData unit = new() { UnitId = "unit", Level = 1 };
            int levelOnePower = FormationPowerCalculator.CalculateUnit(definition, unit);
            unit.Level = 10;
            int levelTenPower = FormationPowerCalculator.CalculateUnit(definition, unit);

            Assert.That(levelTenPower, Is.GreaterThan(levelOnePower));
        }
        finally
        {
            Object.DestroyImmediate(definition);
        }
    }

    private static UnitDataSO CreateUnit(string id, float attack, float hp)
    {
        UnitDataSO unit = ScriptableObject.CreateInstance<UnitDataSO>();
        unit.unitId = id;
        unit.maxLevel = 50;
        unit.baseStats = new UnitStats(attack, hp, 1f, 3f, 0f, 1.5f, 10f);
        return unit;
    }
}
