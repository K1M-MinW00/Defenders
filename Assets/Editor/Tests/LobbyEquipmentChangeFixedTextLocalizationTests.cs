using NUnit.Framework;
using UnityEngine.Localization.Settings;

public sealed class LobbyEquipmentChangeFixedTextLocalizationTests
{
    [TestCase("ko", "equipment.slot.main_weapon", "주무기")]
    [TestCase("en", "equipment.slot.main_weapon", "Main Weapon")]
    [TestCase("ko", "equipment.slot.sub_weapon", "보조무기")]
    [TestCase("en", "equipment.slot.sub_weapon", "Secondary Weapon")]
    [TestCase("ko", "equipment.slot.helmet", "투구")]
    [TestCase("en", "equipment.slot.helmet", "Helmet")]
    [TestCase("ko", "equipment.slot.armor", "갑옷")]
    [TestCase("en", "equipment.slot.armor", "Armor")]
    [TestCase("ko", "equipment.stats.title", "장비 스탯")]
    [TestCase("en", "equipment.stats.title", "Equipment Stats")]
    [TestCase("ko", "equipment.modification.title", "개조 효과")]
    [TestCase("en", "equipment.modification.title", "Modification Effects")]
    [TestCase("ko", "equipment.change.title", "장비 변경")]
    [TestCase("en", "equipment.change.title", "Change Equipment")]
    [TestCase("ko", "equipment.change.guide", "장비를 착용하거나 교체할 수 있습니다.")]
    [TestCase("en", "equipment.change.guide", "Equip or replace equipment.")]
    [TestCase("ko", "equipment.change.empty", "현재 착용한 장비가 없습니다.")]
    [TestCase("en", "equipment.change.empty", "No equipment is currently equipped.")]
    [TestCase("ko", "equipment.button.equip", "장착")]
    [TestCase("en", "equipment.button.equip", "Equip")]
    public void EquipmentChangeText_UsesSelectedLocale(string localeCode, string key, string expected)
    {
        LocalizationSettings.InitializationOperation.WaitForCompletion();
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);

        Assert.That(LocalizationService.Get(LocalizationTables.Lobby, key), Is.EqualTo(expected));
    }
}
