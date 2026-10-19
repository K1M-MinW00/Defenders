using NUnit.Framework;
using UnityEngine.Localization.Settings;

public sealed class LobbyLabLocalizationTests
{
    [TestCase("ko", "lab.title", "연구소")]
    [TestCase("en", "lab.title", "Lab")]
    [TestCase("ko", "lab.develop", "기술 개발")]
    [TestCase("en", "lab.develop", "Develop Technology")]
    public void FixedLabText_UsesSelectedLocale(string localeCode, string key, string expected)
    {
        SelectLocale(localeCode);
        Assert.That(Localize(key), Is.EqualTo(expected));
    }

    [TestCase("ko", "보유 7/10  다음 슬롯 레벨 15")]
    [TestCase("en", "Owned 7/10  Next slot at Lv. 15")]
    public void Capacity_FormatsCurrentAndNextSlotLevel(string localeCode, string expected)
    {
        SelectLocale(localeCode);
        Assert.That(Localize("lab.capacity", 7, 10, 15), Is.EqualTo(expected));
    }

    [TestCase("ko", "재화가 부족합니다.")]
    [TestCase("en", "Not enough research materials.")]
    public void InsufficientCurrency_UsesSelectedLocale(string localeCode, string expected)
    {
        SelectLocale(localeCode);
        Assert.That(Localize("lab.failure.insufficient_currency"), Is.EqualTo(expected));
    }

    [TestCase("ko", "계정 레벨을 더 올리면 새로운 기술을 개발할 수 있습니다.")]
    [TestCase("en", "Increase your account level to develop new technologies.")]
    public void LevelLocked_UsesSelectedLocale(string localeCode, string expected)
    {
        SelectLocale(localeCode);
        Assert.That(Localize("lab.failure.level_locked"), Is.EqualTo(expected));
    }

    [TestCase("ko", "모든 기술 카드를 획득했습니다.")]
    [TestCase("en", "You have acquired all technology cards.")]
    public void AllCardsAcquired_UsesSelectedLocale(string localeCode, string expected)
    {
        SelectLocale(localeCode);
        Assert.That(Localize("lab.failure.all_cards_acquired"), Is.EqualTo(expected));
    }

    [TestCase("ko", "기술 개발을 진행할 수 없습니다.")]
    [TestCase("en", "Unable to develop a technology.")]
    public void Unavailable_UsesSelectedLocale(string localeCode, string expected)
    {
        SelectLocale(localeCode);
        Assert.That(Localize("lab.failure.unavailable"), Is.EqualTo(expected));
    }

    private static string Localize(string key, params object[] arguments) =>
        LocalizationService.Get(LocalizationTables.Lobby, key, arguments);

    private static void SelectLocale(string localeCode)
    {
        LocalizationSettings.InitializationOperation.WaitForCompletion();
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
    }
}
