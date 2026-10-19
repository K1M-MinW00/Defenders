using NUnit.Framework;
using UnityEngine.Localization.Settings;

public sealed class LobbyRewardRecruitFixedTextLocalizationTests
{
    [TestCase("ko", "idle.current_rewards", "현재 보상")]
    [TestCase("en", "idle.current_rewards", "Current Rewards")]
    [TestCase("ko", "idle.max_supply_support", "최대 보급품 지원 : 12시간")]
    [TestCase("en", "idle.max_supply_support", "Maximum supply support: 12 hours")]
    [TestCase("ko", "idle.claim.button", "획득")]
    [TestCase("en", "idle.claim.button", "Claim")]
    [TestCase("ko", "wave_reward.claim_all", "전부 수령")]
    [TestCase("en", "wave_reward.claim_all", "Claim All")]
    [TestCase("ko", "recruit.panel.title", "모집")]
    [TestCase("en", "recruit.panel.title", "Recruit Units")]
    [TestCase("ko", "recruit.button.recruit", "모집")]
    [TestCase("en", "recruit.button.recruit", "Recruit")]
    public void FixedText_UsesSelectedLocale(string localeCode, string key, string expected)
    {
        LocalizationSettings.InitializationOperation.WaitForCompletion();
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
        Assert.That(LocalizationService.Get(LocalizationTables.Lobby, key), Is.EqualTo(expected));
    }
}
