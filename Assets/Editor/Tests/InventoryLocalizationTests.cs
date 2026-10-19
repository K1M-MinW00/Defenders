using NUnit.Framework;
using UnityEditor;
using UnityEngine.Localization.Settings;

public sealed class InventoryLocalizationTests
{
    [TestCase("ko", "보유: 1,250")]
    [TestCase("en", "Owned: 1,250")]
    public void OwnedCount_FormatsAmount(string localeCode, string expected)
    {
        SelectLocale(localeCode);
        Assert.That(Localize("item.owned", 1250), Is.EqualTo(expected));
    }

    [TestCase("ko", "합산 경험치: 3,500")]
    [TestCase("en", "Total EXP: 3,500")]
    public void DismantleExperience_FormatsAmount(string localeCode, string expected)
    {
        SelectLocale(localeCode);
        Assert.That(Localize("equipment.dismantle.total_exp", 3500), Is.EqualTo(expected));
    }

    [TestCase("ko", "장비 교체")]
    [TestCase("en", "Replace")]
    public void ReplaceAction_UsesSelectedLocale(string localeCode, string expected)
    {
        SelectLocale(localeCode);
        Assert.That(Localize("equipment.action.replace"), Is.EqualTo(expected));
    }

    [TestCase("ko", "치명타 확률")]
    [TestCase("en", "Critical Chance")]
    public void CriticalChance_UsesSelectedLocale(string localeCode, string expected)
    {
        SelectLocale(localeCode);
        Assert.That(Localize("equipment.stat.crit_chance"), Is.EqualTo(expected));
    }

    [TestCase("ko", "전설")]
    [TestCase("en", "Legendary")]
    public void LegendaryRarity_UsesSelectedLocale(string localeCode, string expected)
    {
        SelectLocale(localeCode);
        Assert.That(Localize("equipment.rarity.legend"), Is.EqualTo(expected));
    }

    [TestCase("ko", "장비 강화에 실패했습니다.")]
    [TestCase("en", "Failed to enhance the equipment.")]
    public void EnhancementFailure_UsesSelectedLocale(string localeCode, string expected)
    {
        SelectLocale(localeCode);
        Assert.That(Localize("equipment.enhance.failed"), Is.EqualTo(expected));
    }

    [TestCase("ko", "성장의 정수")]
    [TestCase("en", "Essence of Growth")]
    public void ItemName_UsesSelectedLocale(string localeCode, string expected)
    {
        SelectLocale(localeCode);
        ItemDataSO item = AssetDatabase.LoadAssetAtPath<ItemDataSO>(
            "Assets/Resources/GameData/Items/Materials/material_exp_1.asset");
        Assert.That(item.LocalizedName, Is.EqualTo(expected));
    }

    [TestCase("ko")]
    [TestCase("en")]
    public void ItemDescription_UsesSelectedLocale(string localeCode)
    {
        SelectLocale(localeCode);
        ItemDataSO item = AssetDatabase.LoadAssetAtPath<ItemDataSO>(
            "Assets/Resources/GameData/Items/Consumables/summon_ticket_1.asset");
        Assert.That(item.LocalizedDescription, Is.Not.Empty);
        if (localeCode == "en")
            Assert.That(item.LocalizedDescription, Is.EqualTo("A ticket used to recruit a new mercenary."));
    }

    [Test]
    public void StackableItemCount_ContainsOnlyFormattedNumber()
    {
        ItemDataSO item = AssetDatabase.LoadAssetAtPath<ItemDataSO>(
            "Assets/Resources/GameData/Items/Materials/material_exp_1.asset");
        Assert.That(ItemDetailPanelView.FormatCount(item, 1250), Is.EqualTo("1,250"));
    }

    private static string Localize(string key, params object[] arguments) =>
        LocalizationService.Get(LocalizationTables.Items, key, arguments);

    private static void SelectLocale(string localeCode)
    {
        LocalizationSettings.InitializationOperation.WaitForCompletion();
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
    }
}
