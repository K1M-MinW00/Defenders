using System.Linq;
using NUnit.Framework;
using UnityEditor;

public sealed class UnitSkillAnimationValidationTests
{
    [Test]
    public void ActiveSkillUnits_HaveRequiredAnimationEvents()
    {
        GameDataValidationReport report = new();
        UnitDataSO[] units = AssetDatabase.FindAssets("t:UnitDataSO")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<UnitDataSO>)
            .Where(unit => unit != null)
            .ToArray();

        foreach (UnitDataSO unit in units)
            GameDataProjectValidator.ValidateUnitSkillAnimation(unit, report);

        Assert.That(report.HasErrors, Is.False, report.Format());
    }
}
