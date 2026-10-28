using NUnit.Framework;
using UnityEditor;

public sealed class SkillStatusEffectProfileTests
{
    [TestCase("Archer_Active", StatusEffectType.Slow, 3f, 0.35f, SkillStatusMagnitudeMode.Fixed)]
    [TestCase("Werebear_Active", StatusEffectType.Stun, 1.5f, 1f, SkillStatusMagnitudeMode.Fixed)]
    [TestCase("SoldierR_Active", StatusEffectType.Poison, 5f, 0.1f, SkillStatusMagnitudeMode.SourceAttackRatio)]
    [TestCase("Wizard_Active", StatusEffectType.Burn, 4f, 0.15f, SkillStatusMagnitudeMode.SourceAttackRatio)]
    [TestCase("KnightTemplar_Active", StatusEffectType.DefenseDown, 5f, 25f, SkillStatusMagnitudeMode.Fixed)]
    public void ActiveSkillProfile_HasExpectedStatusEffect(
        string assetName,
        StatusEffectType expectedType,
        float expectedDuration,
        float expectedMagnitude,
        SkillStatusMagnitudeMode expectedMode)
    {
        string[] matchingGuids = AssetDatabase.FindAssets(
            $"{assetName} t:SkillDataSO",
            new[] { "Assets/GameData/Units" });
        Assert.That(matchingGuids, Has.Length.EqualTo(1));

        SkillDataSO data = AssetDatabase.LoadAssetAtPath<SkillDataSO>(
            AssetDatabase.GUIDToAssetPath(matchingGuids[0]));

        Assert.That(data, Is.Not.Null);
        Assert.That(data.onHitStatusEffect, Is.Not.Null);
        Assert.That(data.onHitStatusEffect.enabled, Is.True);
        Assert.That(data.onHitStatusEffect.type, Is.EqualTo(expectedType));
        Assert.That(data.onHitStatusEffect.chance, Is.EqualTo(1f));
        Assert.That(data.onHitStatusEffect.duration, Is.EqualTo(expectedDuration));
        Assert.That(data.onHitStatusEffect.magnitude, Is.EqualTo(expectedMagnitude));
        Assert.That(data.onHitStatusEffect.magnitudeMode, Is.EqualTo(expectedMode));
    }
}
