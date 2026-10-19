using NUnit.Framework;
using UnityEditor.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

public sealed class LobbyPopupLocalizationTests
{
    [Test]
    public void LobbyTable_HasMatchingKoreanAndEnglishPopupEntries()
    {
        StringTableCollection collection =
            LocalizationEditorSettings.GetStringTableCollection(LocalizationTables.Lobby);
        Assert.That(collection, Is.Not.Null);

        StringTable korean = collection.GetTable("ko") as StringTable;
        StringTable english = collection.GetTable("en") as StringTable;
        Assert.That(korean, Is.Not.Null);
        Assert.That(english, Is.Not.Null);
        Assert.That(collection.SharedData.Entries.Count, Is.GreaterThanOrEqualTo(10));

        foreach (SharedTableData.SharedTableEntry sharedEntry in collection.SharedData.Entries)
        {
            Assert.That(korean.GetEntry(sharedEntry.Id)?.Value, Is.Not.Empty, $"Missing Korean: {sharedEntry.Key}");
            Assert.That(english.GetEntry(sharedEntry.Id)?.Value, Is.Not.Empty, $"Missing English: {sharedEntry.Key}");
        }
    }

    [TestCase("ko", "젬 1,234개를 사용하여 모집하시겠습니까?")]
    [TestCase("en", "Spend 1,234 Gems to recruit?")]
    public void RecruitConfirmation_FormatsCostForSelectedLocale(string localeCode, string expected)
    {
        SelectLocale(localeCode);

        string message = LocalizationService.Get(
            LocalizationTables.Lobby,
            "recruit.confirm_gems",
            1234);

        Assert.That(message, Is.EqualTo(expected));
    }

    [TestCase("ko", "구매 후 젬 2,500")]
    [TestCase("en", "Gems after purchase: 2,500")]
    public void ShopRemainingGems_FormatsAmountForSelectedLocale(string localeCode, string expected)
    {
        SelectLocale(localeCode);

        string message = LocalizationService.Get(
            LocalizationTables.Lobby,
            "shop.remaining_gems",
            2500L);

        Assert.That(message, Is.EqualTo(expected));
    }

    private static void SelectLocale(string localeCode)
    {
        LocalizationSettings.InitializationOperation.WaitForCompletion();
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
    }
}
