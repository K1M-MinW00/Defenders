using NUnit.Framework;
using UnityEngine.Localization.Settings;

public sealed class LobbyRecruitLocalizationTests
{
    [TestCase("ko", "recruit.title", "유닛 모집")]
    [TestCase("en", "recruit.title", "Unit Recruitment")]
    [TestCase("ko", "recruit.tab.normal", "일반\n모집")]
    [TestCase("en", "recruit.tab.normal", "Normal\nRecruitment")]
    [TestCase("ko", "recruit.tab.special", "특별\n모집")]
    [TestCase("en", "recruit.tab.special", "Special\nRecruitment")]
    [TestCase("ko", "recruit.rate_preview", "확률 미리보기")]
    [TestCase("en", "recruit.rate_preview", "View Rates")]
    [TestCase("ko", "recruit.rate.title", "확률 정보")]
    [TestCase("en", "recruit.rate.title", "Rate Information")]
    [TestCase("ko", "recruit.result.title", "모집 결과")]
    [TestCase("en", "recruit.result.title", "Recruitment Results")]
    [TestCase("ko", "recruit.presentation.new_unit", "새로운 유닛")]
    [TestCase("en", "recruit.presentation.new_unit", "New Unit")]
    [TestCase("ko", "recruit.presentation.skip", "건너뛰기")]
    [TestCase("en", "recruit.presentation.skip", "Skip")]
    [TestCase("ko", "recruit.rarity.normal", "노말")]
    [TestCase("en", "recruit.rarity.normal", "Normal")]
    [TestCase("ko", "recruit.rarity.rare", "레어")]
    [TestCase("en", "recruit.rarity.rare", "Rare")]
    [TestCase("ko", "recruit.rarity.epic", "에픽")]
    [TestCase("en", "recruit.rarity.epic", "Epic")]
    [TestCase("ko", "recruit.rarity.legend", "전설")]
    [TestCase("en", "recruit.rarity.legend", "Legendary")]
    [TestCase("ko", "recruit.button.use", "사용하기")]
    [TestCase("en", "recruit.button.use", "Use")]
    [TestCase("ko", "recruit.button.back", "돌아가기")]
    [TestCase("en", "recruit.button.back", "Back")]
    public void FixedRecruitText_UsesSelectedLocale(string localeCode, string key, string expected)
    {
        SelectLocale(localeCode);

        Assert.That(Localize(key), Is.EqualTo(expected));
    }

    [TestCase("ko", "앞으로 12회 모집 안에 전설 유닛 확정 획득")]
    [TestCase("en", "Legendary unit guaranteed within 12 recruits")]
    public void PityNotice_UsesSelectedLocale(string localeCode, string expected)
    {
        SelectLocale(localeCode);

        Assert.That(Localize("recruit.pity", 12), Is.EqualTo(expected));
    }

    [TestCase("ko", "전설 유닛")]
    [TestCase("en", "Legendary Units")]
    public void LegendaryRateLabel_UsesSelectedLocale(string localeCode, string expected)
    {
        SelectLocale(localeCode);

        Assert.That(Localize("recruit.rate.legend_units"), Is.EqualTo(expected));
    }

    [TestCase("ko", "수호자 (픽업)")]
    [TestCase("en", "Guardian (Rate Up)")]
    public void PickupLabel_FormatsUnitName(string localeCode, string expected)
    {
        SelectLocale(localeCode);
        string unitName = localeCode == "ko" ? "수호자" : "Guardian";

        Assert.That(Localize("recruit.rate.pickup", unitName), Is.EqualTo(expected));
    }

    private static string Localize(string key, params object[] arguments) =>
        LocalizationService.Get(LocalizationTables.Lobby, key, arguments);

    private static void SelectLocale(string localeCode)
    {
        LocalizationSettings.InitializationOperation.WaitForCompletion();
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
    }
}
