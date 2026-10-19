using NUnit.Framework;
using UnityEngine.Localization.Settings;

public sealed class LobbyMailboxFixedTextLocalizationTests
{
    [TestCase("ko", "mail.title", "우편함")]
    [TestCase("en", "mail.title", "Mailbox")]
    [TestCase("ko", "mail.receive_all", "전부 수령")]
    [TestCase("en", "mail.receive_all", "Claim All")]
    [TestCase("ko", "mail.delete_all", "일괄 삭제")]
    [TestCase("en", "mail.delete_all", "Delete All")]
    public void MailboxFixedText_UsesSelectedLocale(string localeCode, string key, string expected)
    {
        LocalizationSettings.InitializationOperation.WaitForCompletion();
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);

        Assert.That(LocalizationService.Get(LocalizationTables.Lobby, key), Is.EqualTo(expected));
    }
}
