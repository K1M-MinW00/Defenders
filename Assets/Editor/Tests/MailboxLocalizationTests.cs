using NUnit.Framework;
using UnityEngine.Localization.Settings;

public sealed class MailboxLocalizationTests
{
    [TestCase("ko", "3일 후 만료")]
    [TestCase("en", "Expires in 3 days")]
    public void ExpirationDays_FormatsCount(string localeCode, string expected)
    {
        SelectLocale(localeCode);
        Assert.That(Localize("mail.expires_days", 3), Is.EqualTo(expected));
    }

    [TestCase("ko", "곧 만료")]
    [TestCase("en", "Expires soon")]
    public void ExpiringSoon_UsesSelectedLocale(string localeCode, string expected)
    {
        SelectLocale(localeCode);
        Assert.That(Localize("mail.expires_soon"), Is.EqualTo(expected));
    }

    [TestCase("ko", "출시 기념 보상")]
    [TestCase("en", "Launch Celebration Reward")]
    public void LocalizedTitle_UsesRequestedLanguage(string localeCode, string expected)
    {
        var mail = new MailData
        {
            Title = "Legacy Title",
            TitleKo = "출시 기념 보상",
            TitleEn = "Launch Celebration Reward",
        };

        Assert.That(MailTitleResolver.Resolve(mail, localeCode), Is.EqualTo(expected));
    }

    [Test]
    public void LocalizedTitle_FallsBackToLegacyTitle()
    {
        var mail = new MailData { Title = "Legacy Title" };
        Assert.That(MailTitleResolver.Resolve(mail, "en"), Is.EqualTo("Legacy Title"));
    }

    private static string Localize(string key, params object[] arguments) =>
        LocalizationService.Get(LocalizationTables.Lobby, key, arguments);

    private static void SelectLocale(string localeCode)
    {
        LocalizationSettings.InitializationOperation.WaitForCompletion();
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
    }
}
