using System.Collections.Generic;
using NUnit.Framework;

public sealed class UnitTrainingPreviewCalculatorTests
{
    [OneTimeSetUp]
    public void InitializeDatabase()
    {
        ItemDatabase.Initialize();
    }

    [Test]
    public void Calculate_AppliesLevelUpAndGoldCost()
    {
        UserUnitData unit = new()
        {
            Level = 1,
            Exp = 0,
        };
        Dictionary<string, int> materials = new()
        {
            ["material_exp_1"] = 20,
        };

        UnitTrainingPreview preview = UnitTrainingPreviewCalculator.Calculate(
            unit,
            UnitExpTable.MaxLevel,
            materials,
            200);

        Assert.That(preview.Level, Is.EqualTo(2));
        Assert.That(preview.Exp, Is.Zero);
        Assert.That(preview.TotalExp, Is.EqualTo(200));
        Assert.That(preview.GoldCost, Is.EqualTo(200));
        Assert.That(preview.CanAfford, Is.True);
    }

    [Test]
    public void Calculate_DisablesAffordabilityWhenGoldIsInsufficient()
    {
        UserUnitData unit = new()
        {
            Level = 1,
            Exp = 0,
        };
        Dictionary<string, int> materials = new()
        {
            ["material_exp_1"] = 20,
        };

        UnitTrainingPreview preview = UnitTrainingPreviewCalculator.Calculate(
            unit,
            UnitExpTable.MaxLevel,
            materials,
            199);

        Assert.That(preview.CanAfford, Is.False);
    }
}
