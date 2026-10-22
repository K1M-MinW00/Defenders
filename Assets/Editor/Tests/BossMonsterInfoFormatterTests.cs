using NUnit.Framework;
using UnityEditor;
using UnityEngine.Localization.Settings;

public sealed class BossMonsterInfoFormatterTests
{
    [Test]
    public void ArmoredOrc_ShowsConfiguredEffectsAndNoResistance()
    {
        SelectLocale("ko");
        string text = BossMonsterInfoFormatter.Build(Load("Boss_Armored_Orc"));

        Assert.That(text, Does.Contain("기절"));
        Assert.That(text, Does.Contain("방어력 감소"));
        Assert.That(text, Does.Contain("증원 소환"));
        Assert.That(text, Does.Not.Contain("• 중독:"));
        Assert.That(text, Does.Not.Contain("• 화상:"));
        Assert.That(text, Does.Not.Contain("• 감속:"));
        Assert.That(text, Does.Contain("상태 저항"));
        Assert.That(text, Does.Contain("기절 75%"));
        Assert.That(text, Does.Contain("방어력 감소 50%"));
    }

    [Test]
    public void WereWolf_ShowsConfiguredEffectsInEnglish()
    {
        SelectLocale("en");
        string text = BossMonsterInfoFormatter.Build(Load("Boss_WereWolf"));

        Assert.That(text, Does.Contain("Slow"));
        Assert.That(text, Does.Contain("Burn"));
        Assert.That(text, Does.Not.Contain("• Stun:"));
        Assert.That(text, Does.Contain("Status Resistance"));
        Assert.That(text, Does.Contain("Slow 75%"));
    }

    [Test]
    public void NullDefinition_ReturnsEmptyText()
    {
        Assert.That(BossMonsterInfoFormatter.Build(null), Is.Empty);
    }

    private static BossDefinitionSO Load(string bossId)
    {
        BossDefinitionSO definition = AssetDatabase.LoadAssetAtPath<BossDefinitionSO>(
            $"Assets/GameData/Bosses/{bossId}_Definition.asset");
        Assert.That(definition, Is.Not.Null);
        return definition;
    }

    private static void SelectLocale(string localeCode)
    {
        LocalizationSettings.InitializationOperation.WaitForCompletion();
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
    }
}
