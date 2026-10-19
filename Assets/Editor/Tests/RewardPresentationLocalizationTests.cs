using NUnit.Framework;
using UnityEngine;
using UnityEngine.Localization.Settings;

public sealed class RewardPresentationLocalizationTests
{
    [TestCase("ko", RewardType.Gold, "골드")]
    [TestCase("en", RewardType.Gold, "Gold")]
    [TestCase("ko", RewardType.Fuel, "연료")]
    [TestCase("en", RewardType.Fuel, "Fuel")]
    [TestCase("ko", RewardType.ResearchMaterial, "연구 재료")]
    [TestCase("en", RewardType.ResearchMaterial, "Research Materials")]
    public void ResourceReward_UsesSelectedLocale(string localeCode, RewardType type, string expected)
    {
        LocalizationSettings.InitializationOperation.WaitForCompletion();
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);

        bool resolved = RewardPresentationResolver.TryResolve(
            new RewardData { Type = type, Amount = 1 },
            new StubIconProvider(),
            out RewardPresentation presentation);

        Assert.That(resolved, Is.True);
        Assert.That(presentation.DisplayName, Is.EqualTo(expected));
    }

    private sealed class StubIconProvider : IGameIconProvider
    {
        public Sprite GetResourceIcon(RewardType type) => null;
        public string GetResourceDisplayName(RewardType type) => "unlocalized";
        public Rarity GetResourceRarity(RewardType type) => Rarity.Normal;
    }
}
