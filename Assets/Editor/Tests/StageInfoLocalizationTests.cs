using NUnit.Framework;
using UnityEngine.Localization.Settings;

public sealed class StageInfoLocalizationTests
{
    [TestCase("ko", "잠김 - 3성부터 해금")]
    [TestCase("en", "Locked - Unlocks at 3 Stars")]
    public void ActiveSkillLock_UsesSelectedLocale(string localeCode, string expected)
    { SelectLocale(localeCode); Assert.That(Localize("unit_info.active_locked"), Is.EqualTo(expected)); }

    [TestCase("ko", "[진급 2 효과] ")]
    [TestCase("en", "[Promotion 2 Effect] ")]
    public void PromotionEffect_FormatsTier(string localeCode, string expected)
    { SelectLocale(localeCode); Assert.That(Localize("unit_info.promotion_effect", 2), Is.EqualTo(expected)); }

    [TestCase("ko", "스킬 정보가 없습니다.")]
    [TestCase("en", "No skill information available.")]
    public void MissingSkillInfo_UsesSelectedLocale(string localeCode, string expected)
    { SelectLocale(localeCode); Assert.That(Localize("unit_info.no_skill_info"), Is.EqualTo(expected)); }

    [TestCase("ko", "빠르게 돌진하는 근접 오크 전사")]
    [TestCase("en", "A melee orc warrior that charges swiftly.")]
    public void OrcDescription_UsesSelectedLocale(string localeCode, string expected)
    { SelectLocale(localeCode); Assert.That(Localize("monster.description.Orc"), Is.EqualTo(expected)); }

    [TestCase("ko", "빠르고 거센 연속 공격을 가하는 늑대인간")]
    [TestCase("en", "A werewolf that unleashes fast, ferocious combinations.")]
    public void WerewolfDescription_UsesSelectedLocale(string localeCode, string expected)
    { SelectLocale(localeCode); Assert.That(Localize("monster.description.WereWolf"), Is.EqualTo(expected)); }

    private static string Localize(string key, params object[] arguments) =>
        LocalizationService.Get(LocalizationTables.Stage, key, arguments);

    private static void SelectLocale(string localeCode)
    {
        LocalizationSettings.InitializationOperation.WaitForCompletion();
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
    }
}
