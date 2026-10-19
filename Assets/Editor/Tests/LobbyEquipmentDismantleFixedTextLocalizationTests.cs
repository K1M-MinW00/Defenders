using NUnit.Framework;
using UnityEngine.Localization.Settings;

public sealed class LobbyEquipmentDismantleFixedTextLocalizationTests
{
    [TestCase("ko", "inventory.dismantle.title", "분해")]
    [TestCase("en", "inventory.dismantle.title", "Dismantle")]
    [TestCase("ko", "inventory.dismantle.guide", "분해할 장비를 선택하세요.")]
    [TestCase("en", "inventory.dismantle.guide", "Select equipment to dismantle.")]
    [TestCase("ko", "inventory.dismantle.rewards", "분해 보상")]
    [TestCase("en", "inventory.dismantle.rewards", "Dismantle Rewards")]
    [TestCase("ko", "inventory.dismantle.rewards_acquired", "분해 보상 획득")]
    [TestCase("en", "inventory.dismantle.rewards_acquired", "Dismantle Rewards Acquired")]
    public void DismantleText_UsesSelectedLocale(string localeCode, string key, string expected)
    {
        LocalizationSettings.InitializationOperation.WaitForCompletion();
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);

        Assert.That(LocalizationService.Get(LocalizationTables.Lobby, key), Is.EqualTo(expected));
    }
}
