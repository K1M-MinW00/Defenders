using NUnit.Framework;
using UnityEditor;

public sealed class BossStatusEffectProfileTests
{
    [Test]
    public void ArmoredOrc_UsesStunAndDefenseDownOnly()
    {
        BossAreaAttackStatusSettings settings = Load("Boss_Armored_Orc");

        Assert.That(settings.EnabledEffectCount, Is.EqualTo(2));
        Assert.That(settings.enableStun, Is.True);
        Assert.That(settings.enableDefenseDown, Is.True);
        Assert.That(settings.enableSlow, Is.False);
        Assert.That(settings.enablePoison, Is.False);
        Assert.That(settings.enableBurn, Is.False);
    }

    [Test]
    public void WereWolf_UsesSlowAndBurnOnly()
    {
        BossAreaAttackStatusSettings settings = Load("Boss_WereWolf");

        Assert.That(settings.EnabledEffectCount, Is.EqualTo(2));
        Assert.That(settings.enableSlow, Is.True);
        Assert.That(settings.enableBurn, Is.True);
        Assert.That(settings.enableStun, Is.False);
        Assert.That(settings.enablePoison, Is.False);
        Assert.That(settings.enableDefenseDown, Is.False);
    }

    [TestCase("Boss_Elite_Orc")]
    [TestCase("Boss_Armored_Orc")]
    [TestCase("Boss_Armored_Skeleton")]
    [TestCase("Boss_WereWolf")]
    public void CandidateProfile_IsValidAndKeepsEffectCountReadable(string bossId)
    {
        BossDefinitionSO definition = AssetDatabase.LoadAssetAtPath<BossDefinitionSO>(
            $"Assets/GameData/Bosses/{bossId}_Definition.asset");

        Assert.That(definition, Is.Not.Null);
        Assert.That(definition.TryValidate(out string error), Is.True, error);
        Assert.That(definition.areaAttackStatusEffects.EnabledEffectCount, Is.InRange(1, 2));
    }

    [TestCase("Boss_Elite_Orc")]
    [TestCase("Boss_Armored_Orc")]
    [TestCase("Boss_Armored_Skeleton")]
    [TestCase("Boss_WereWolf")]
    public void CandidateProfile_HasUnassignedOptionalBossAudioSlots(string bossId)
    {
        BossDefinitionSO definition = AssetDatabase.LoadAssetAtPath<BossDefinitionSO>(
            $"Assets/GameData/Bosses/{bossId}_Definition.asset");

        Assert.That(definition, Is.Not.Null);
        Assert.That(definition.audio, Is.Not.Null);
        Assert.That(definition.audio.areaAttackTelegraph.IsValid, Is.False);
        Assert.That(definition.audio.areaAttackImpact.IsValid, Is.False);
        Assert.That(definition.audio.shieldActivated.IsValid, Is.False);
        Assert.That(definition.audio.enrage.IsValid, Is.False);
    }

    private static BossAreaAttackStatusSettings Load(string bossId)
    {
        BossDefinitionSO definition = AssetDatabase.LoadAssetAtPath<BossDefinitionSO>(
            $"Assets/GameData/Bosses/{bossId}_Definition.asset");
        Assert.That(definition, Is.Not.Null);
        return definition.areaAttackStatusEffects;
    }

    [Test]
    public void ArmoredOrc_SummonsNormalOrcsWithSafeLimits()
    {
        BossDefinitionSO definition = AssetDatabase.LoadAssetAtPath<BossDefinitionSO>(
            "Assets/GameData/Bosses/Boss_Armored_Orc_Definition.asset");

        Assert.That(definition.summon.enabled, Is.True);
        Assert.That(definition.summon.count, Is.EqualTo(2));
        Assert.That(definition.summon.maxAliveSummons, Is.EqualTo(4));
        Assert.That(definition.summon.candidates, Has.Count.EqualTo(1));
        Assert.That(definition.summon.candidates[0].monsterId, Is.EqualTo("Orc"));
        Assert.That(definition.summon.TryValidate(out string error), Is.True, error);
    }

    [Test]
    public void ArmoredSkeleton_IsImmuneToPoison()
    {
        BossDefinitionSO definition = AssetDatabase.LoadAssetAtPath<BossDefinitionSO>(
            "Assets/GameData/Bosses/Boss_Armored_Skeleton_Definition.asset");

        Assert.That(definition.statusResistance.poison, Is.EqualTo(1f));
        Assert.That(definition.statusResistance.stun, Is.EqualTo(0.75f));
    }
}
