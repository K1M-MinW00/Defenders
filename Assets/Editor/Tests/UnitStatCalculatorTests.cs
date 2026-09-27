using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public sealed class UnitStatCalculatorTests
{
    [Test]
    public void Calculate_AppliesLevelAndPersistentGrowth()
    {
        UnitDataSO definition = ScriptableObject.CreateInstance<UnitDataSO>();
        definition.baseStats = new UnitStats(100f, 500f, 1f, 3f, 0f, 0f, 0f);
        definition.attackGrowthValue = 5;
        definition.hpGrowthValue = 20;
        definition.limitBreaks = new List<LimitBreakData>
        {
            new() { statType = StatType.Attack, value = 10f },
        };
        UserUnitData userUnit = new() { Level = 2, Promotion = 0, LimitBreak = 1 };

        try
        {
            UnitStats result = UnitStatCalculator.Calculate(definition, userUnit);

            Assert.That(result.Attack, Is.EqualTo(115.5f).Within(0.001f));
            Assert.That(result.MaxHp, Is.EqualTo(520f));
        }
        finally
        {
            Object.DestroyImmediate(definition);
        }
    }
}
