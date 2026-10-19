using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

public static class LabCardStringTableSetup
{
    private readonly struct Translation
    {
        public Translation(string name, string description) { Name = name; Description = description; }
        public string Name { get; }
        public string Description { get; }
    }

    private static readonly Dictionary<string, Translation> English = new()
    {
        ["attack"] = new("Firepower Amplification", "Increases allied units' Attack."),
        ["attack_speed"] = new("Neural Acceleration", "Increases allied units' Attack Speed."),
        ["critical_chance"] = new("Precision Aiming", "Increases allied units' Critical Chance."),
        ["critical_damage"] = new("Weakness Analysis", "Increases allied units' Critical Damage."),
        ["damage_reduction"] = new("Reinforced Armor", "Reduces damage taken by allied units."),
        ["detection_range"] = new("Wide-Area Detection", "Increases allied units' Detection Range."),
        ["energy_recovery"] = new("Energy Circulation", "Increases allied units' Energy Recovery."),
        ["idle_reward"] = new("Automated Research", "Increases idle reward gains."),
        ["max_hp"] = new("Biological Enhancement", "Increases allied units' maximum HP."),
        ["max_units"] = new("Command Structure", "Increases the maximum unit count at the start of battle."),
        ["population_discount"] = new("Command Automation", "Reduces the Minerals required to expand the maximum unit count."),
        ["reroll_discount"] = new("Recycling Process", "Reduces the Minerals required to reroll units."),
        ["sell_price"] = new("Resource Recovery", "Increases the Minerals refunded when selling units."),
        ["starting_minerals"] = new("Initial Supplies", "Increases starting Minerals."),
        ["wave_reward"] = new("Field Mining", "Increases Minerals gained upon completing a wave."),
    };

    public static void Configure(Locale korean, Locale english, string tablesDirectory)
    {
        StringTableCollection collection = LocalizationEditorSettings.GetStringTableCollection(LocalizationTables.Relics) ??
            LocalizationEditorSettings.CreateStringTableCollection(
                LocalizationTables.Relics, tablesDirectory, new List<Locale> { korean, english });
        StringTable ko = collection.GetTable(korean.Identifier) as StringTable ?? collection.AddNewTable(korean.Identifier) as StringTable;
        StringTable en = collection.GetTable(english.Identifier) as StringTable ?? collection.AddNewTable(english.Identifier) as StringTable;

        foreach (string guid in AssetDatabase.FindAssets("t:LabCardDataSO", new[] { "Assets/Resources/GameData/LabCards" }))
        {
            LabCardDataSO card = AssetDatabase.LoadAssetAtPath<LabCardDataSO>(AssetDatabase.GUIDToAssetPath(guid));
            if (card == null) continue;
            Translation translated = English.TryGetValue(card.CardId, out Translation value)
                ? value
                : new Translation(card.DisplayName, card.Description);
            Set(ko, $"lab.{card.CardId}.name", card.DisplayName);
            Set(ko, $"lab.{card.CardId}.description", card.Description);
            Set(en, $"lab.{card.CardId}.name", translated.Name);
            Set(en, $"lab.{card.CardId}.description", translated.Description);
        }

        EditorUtility.SetDirty(ko); EditorUtility.SetDirty(en); EditorUtility.SetDirty(collection.SharedData);
    }

    private static void Set(StringTable table, string key, string value)
    {
        StringTableEntry entry = table.GetEntry(key) ?? table.AddEntry(key, value);
        entry.Value = value;
        entry.IsSmart = false;
    }
}
