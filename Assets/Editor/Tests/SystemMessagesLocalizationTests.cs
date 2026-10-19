using NUnit.Framework;
using UnityEditor.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

public sealed class SystemMessagesLocalizationTests
{
    [Test]
    public void SystemMessages_HaveMatchingNonEmptyKoreanAndEnglishEntries()
    {
        StringTableCollection collection =
            LocalizationEditorSettings.GetStringTableCollection(LocalizationTables.SystemMessages);
        Assert.That(collection, Is.Not.Null);

        StringTable korean = collection.GetTable("ko") as StringTable;
        StringTable english = collection.GetTable("en") as StringTable;
        Assert.That(korean, Is.Not.Null);
        Assert.That(english, Is.Not.Null);
        Assert.That(collection.SharedData.Entries.Count, Is.GreaterThanOrEqualTo(48));

        foreach (SharedTableData.SharedTableEntry sharedEntry in collection.SharedData.Entries)
        {
            Assert.That(korean.GetEntry(sharedEntry.Id)?.Value, Is.Not.Empty, $"Missing Korean: {sharedEntry.Key}");
            Assert.That(english.GetEntry(sharedEntry.Id)?.Value, Is.Not.Empty, $"Missing English: {sharedEntry.Key}");
        }
    }

    [TestCase("ko", "현재 소환 배너를 사용할 수 없습니다.")]
    [TestCase("en", "This recruit banner is currently unavailable.")]
    public void LobbyFeedback_UsesSelectedLocale(string localeCode, string expected)
    {
        SelectLocale(localeCode);

        Assert.That(LobbyOperationFeedbackMessages.Get(RecruitUnitsFailure.InvalidBanner), Is.EqualTo(expected));
    }

    [TestCase("ko", "골드가 부족합니다.")]
    [TestCase("en", "Not enough Gold.")]
    public void UnitFeedback_UsesSelectedLocale(string localeCode, string expected)
    {
        SelectLocale(localeCode);

        Assert.That(UnitOperationFeedbackMessages.Get(TrainUnitFailure.InsufficientGold), Is.EqualTo(expected));
    }

    [TestCase("ko", "구매가 완료되었습니다.")]
    [TestCase("en", "Purchase complete.")]
    public void ShopFeedback_UsesSelectedLocale(string localeCode, string expected)
    {
        SelectLocale(localeCode);

        Assert.That(ShopFeedbackMessages.Get(ShopPurchaseResult.Success(0)), Is.EqualTo(expected));
    }

    private static void SelectLocale(string localeCode)
    {
        LocalizationSettings.InitializationOperation.WaitForCompletion();
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
    }
}
