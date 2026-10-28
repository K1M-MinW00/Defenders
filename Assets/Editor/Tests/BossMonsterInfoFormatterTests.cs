using NUnit.Framework;
using UnityEditor;
using UnityEngine.Localization.Settings;

public sealed class BossMonsterInfoFormatterTests
{
    [Test]
    public void ArmoredOrc_ShowsOnlyConciseMechanics()
    {
        SelectLocale("ko");
        string text = BossMonsterInfoFormatter.Build(Load("Boss_Armored_Orc"));

        Assert.That(text, Is.EqualTo("보호막, 광역 공격, 증원 소환, 광폭화"));
        Assert.That(text, Does.Contain("증원 소환"));
        Assert.That(text, Does.Not.Contain("·"));
        Assert.That(text, Does.Not.Contain("%"));
    }

    [Test]
    public void WereWolf_ShowsOnlyConciseMechanicsInEnglish()
    {
        SelectLocale("en");
        string text = BossMonsterInfoFormatter.Build(Load("Boss_WereWolf"));

        Assert.That(text, Is.EqualTo("Shield, Area Attack, Enrage"));
    }

    [Test]
    public void NullDefinition_ReturnsEmptyText()
    {
        Assert.That(BossMonsterInfoFormatter.Build(null), Is.Empty);
    }

    private static BossDefinitionSO Load(string bossId)
    {
        BossDefinitionSO definition = AssetDatabase.LoadAssetAtPath<BossDefinitionSO>(
            $"Assets/GameData/Monsters/Bosses/Definitions/{bossId}_Definition.asset");
        Assert.That(definition, Is.Not.Null);
        return definition;
    }

    private static void SelectLocale(string localeCode)
    {
        LocalizationSettings.InitializationOperation.WaitForCompletion();
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
    }
}
