using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

public static class InventoryStringTableSetup
{
    private readonly struct Entry
    {
        public Entry(string key, string korean, string english, bool smart = false)
        {
            Key = key;
            Korean = korean;
            English = english;
            Smart = smart;
        }

        public string Key { get; }
        public string Korean { get; }
        public string English { get; }
        public bool Smart { get; }
    }

    private static readonly Entry[] Entries =
    {
        new("item.owned", "보유: {0:N0}", "Owned: {0:N0}", true),
        new("equipment.header", "장비", "Equipment"),
        new("equipment.action.cancel", "취소", "Cancel"),
        new("equipment.action.dismantle", "분해", "Dismantle"),
        new("equipment.action.unequip", "해제", "Unequip"),
        new("equipment.action.max", "최대", "Max"),
        new("equipment.action.enhance", "강화", "Enhance"),
        new("equipment.action.replace", "장비 교체", "Replace"),
        new("equipment.action.confirm", "확인", "Confirm"),
        new("equipment.rarity.legend", "전설", "Legendary"),
        new("equipment.rarity.epic", "에픽", "Epic"),
        new("equipment.rarity.rare", "희귀", "Rare"),
        new("equipment.rarity.normal", "일반", "Normal"),
        new("equipment.stat.max_hp", "최대 체력", "Max HP"),
        new("equipment.stat.attack", "공격력", "Attack"),
        new("equipment.stat.crit_chance", "치명타 확률", "Critical Chance"),
        new("equipment.stat.damage_reduction", "받는 피해 감소", "Damage Reduction"),
        new("equipment.stat.value", "{0}  +{1:N0}", "{0}  +{1:N0}", true),
        new("equipment.stat.bonus", "{0}  +{1:0.#}{2}", "{0}  +{1:0.#}{2}", true),
        new("equipment.dismantle.total_exp", "합산 경험치: {0:N0}", "Total EXP: {0:N0}", true),
        new("equipment.dismantle.failed", "장비 분해에 실패했습니다.", "Failed to dismantle the equipment."),
        new("equipment.equip.failed", "장비를 장착할 수 없습니다.", "Unable to equip this item."),
        new("equipment.unequip.failed", "장비를 해제할 수 없습니다.", "Unable to unequip this item."),
        new("equipment.enhance.failed", "장비 강화에 실패했습니다.", "Failed to enhance the equipment."),
        new("equipment.change.none_equipped", "현재 착용한 장비가 없습니다.", "No equipment is currently equipped."),
        new("equipment.enhance.next_level", "다음 LV.{0}", "Next LV.{0}", true),
        new("unit.promotion", "{0}진급", "Promotion {0}", true),
        new("unit.limit_break.max", "유닛이 이미 최고 품질에 도달했습니다.", "This unit has already reached maximum quality."),
    };

    public static void Configure(Locale korean, Locale english, string tablesDirectory)
    {
        StringTableCollection collection =
            LocalizationEditorSettings.GetStringTableCollection(LocalizationTables.Items);
        if (collection == null)
        {
            collection = LocalizationEditorSettings.CreateStringTableCollection(
                LocalizationTables.Items,
                tablesDirectory,
                new List<Locale> { korean, english });
        }

        StringTable koreanTable = GetOrCreateTable(collection, korean);
        StringTable englishTable = GetOrCreateTable(collection, english);
        foreach (Entry entry in Entries)
        {
            SetEntry(koreanTable, entry.Key, entry.Korean, entry.Smart);
            SetEntry(englishTable, entry.Key, entry.English, entry.Smart);
        }

        ConfigureItemCatalog(koreanTable, englishTable);

        EditorUtility.SetDirty(koreanTable);
        EditorUtility.SetDirty(englishTable);
        EditorUtility.SetDirty(collection.SharedData);
    }

    private static StringTable GetOrCreateTable(StringTableCollection collection, Locale locale) =>
        collection.GetTable(locale.Identifier) as StringTable ??
        collection.AddNewTable(locale.Identifier) as StringTable;

    private static void SetEntry(StringTable table, string key, string value, bool smart)
    {
        StringTableEntry entry = table.GetEntry(key) ?? table.AddEntry(key, value);
        entry.Value = value;
        entry.IsSmart = smart;
    }

    private static void ConfigureItemCatalog(StringTable koreanTable, StringTable englishTable)
    {
        foreach (string guid in AssetDatabase.FindAssets("t:ItemDataSO", new[] { "Assets/Resources/GameData/Items" }))
        {
            ItemDataSO item = AssetDatabase.LoadAssetAtPath<ItemDataSO>(AssetDatabase.GUIDToAssetPath(guid));
            if (item == null || string.IsNullOrWhiteSpace(item.ItemId))
                continue;

            SetEntry(koreanTable, $"item.{item.ItemId}.name", item.ItemName, false);
            SetEntry(koreanTable, $"item.{item.ItemId}.description", item.Description, false);
            SetEntry(englishTable, $"item.{item.ItemId}.name", GetEnglishName(item.ItemId), false);
            SetEntry(englishTable, $"item.{item.ItemId}.description", GetEnglishDescription(item.ItemId), false);
        }
    }

    private static string GetEnglishName(string itemId)
    {
        return itemId switch
        {
            "summon_ticket_normal" => "Mercenary Recruitment Ticket",
            "summon_ticket_special" => "Hero Recruitment Ticket",
            "material_exp_1" => "Essence of Growth",
            "material_exp_2" => "Concentrated Essence of Growth",
            "material_exp_3" => "Transcendent Essence",
            "material_prom_1" => "Training Insignia",
            "material_prom_2" => "Valor Insignia",
            "material_prom_3" => "Hero Insignia",
            "material_prom_4" => "Transcendence Insignia",
            "equipment_stone_rare" => "Magic Stone",
            "equipment_stone_epic" => "Abyssal Magic Stone",
            "equipment_stone_legend" => "Primordial Magic Stone",
            _ when itemId.StartsWith("equipment_main_weapon_") => EquipmentRarity(itemId) + " Longsword",
            _ when itemId.StartsWith("equipment_sub_weapon_") => EquipmentRarity(itemId) + " Shield",
            _ when itemId.StartsWith("equipment_helmet_") => EquipmentRarity(itemId) + " Helmet",
            _ when itemId.StartsWith("equipment_armor_") => EquipmentRarity(itemId) + " Armor",
            _ => itemId,
        };
    }

    private static string GetEnglishDescription(string itemId)
    {
        return itemId switch
        {
            "summon_ticket_normal" => "A ticket used to recruit a new mercenary.",
            "summon_ticket_special" => "A special ticket used to recruit exceptional heroes.",
            "material_exp_1" => "A blue crystal containing condensed life energy. Used to train units.",
            "material_exp_2" => "A highly concentrated crystal of life energy. Used to train units.",
            "material_exp_3" => "A golden essence created through ancient alchemy. Used to train units.",
            "material_prom_1" => "An insignia awarded to warriors who complete their basic training.",
            "material_prom_2" => "An insignia awarded to warriors who prove their valor in battle.",
            "material_prom_3" => "An insignia reserved for heroes of extraordinary achievement.",
            "material_prom_4" => "An insignia granted to those who transcend mortal limits.",
            "equipment_stone_rare" => "Ore infused with blue magic. Used to enhance equipment.",
            "equipment_stone_epic" => "Violet ore found deep underground. Used to enhance equipment.",
            "equipment_stone_legend" => "Golden ore crystallized from primordial magic. Used to enhance equipment.",
            _ when itemId.StartsWith("equipment_") => $"A {EquipmentRarity(itemId).ToLowerInvariant()} piece of equipment forged for battle.",
            _ => string.Empty,
        };
    }

    private static string EquipmentRarity(string itemId)
    {
        if (itemId.EndsWith("_legend")) return "Celestial";
        if (itemId.EndsWith("_epic")) return "Enchanted";
        return "Steel";
    }
}
