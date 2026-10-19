using NUnit.Framework;

public sealed class LocalizationServiceTests
{
    [TestCase("ko", true)]
    [TestCase("en", true)]
    [TestCase("KO", true)]
    [TestCase("en-US", false)]
    [TestCase("ja", false)]
    [TestCase(null, false)]
    public void IsSupportedLanguageCode_OnlyAcceptsConfiguredCodes(string code, bool expected)
    {
        Assert.That(LocalizationService.IsSupportedLanguageCode(code), Is.EqualTo(expected));
    }

    [TestCase("ko", "ko")]
    [TestCase("ko-KR", "ko")]
    [TestCase("en", "en")]
    [TestCase("en-US", "en")]
    [TestCase("ja", "en")]
    [TestCase(null, "en")]
    public void GetSupportedLanguageCode_NormalizesToSupportedLanguage(string code, string expected)
    {
        Assert.That(LocalizationService.GetSupportedLanguageCode(code), Is.EqualTo(expected));
    }

    [Test]
    public void LocalizationKey_RequiresTableAndEntry()
    {
        Assert.That(new LocalizationKey(LocalizationTables.Common, "ui.settings.language").IsValid, Is.True);
        Assert.That(new LocalizationKey("", "ui.settings.language").IsValid, Is.False);
        Assert.That(new LocalizationKey(LocalizationTables.Common, "").IsValid, Is.False);
    }

    [Test]
    public void LocalizationKey_ToStringContainsTableAndEntry()
    {
        var key = new LocalizationKey(LocalizationTables.Stage, "stage.entry.insufficient_fuel");

        Assert.That(key.ToString(), Is.EqualTo("UI_Stage:stage.entry.insufficient_fuel"));
    }
}
