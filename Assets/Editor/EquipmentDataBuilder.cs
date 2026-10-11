using System.IO;
using UnityEditor;
using UnityEngine;

public static class EquipmentDataBuilder
{
    private const string OutputFolder = "Assets/Resources/GameData/Items/Equipments";
    private const string PlaceholderIconPath = "Assets/Art/UI/Generated/Lobby/icon_equipment.png";

    [MenuItem("Tools/Game Data/Rebuild Equipment Definitions")]
    public static void Build()
    {
        Directory.CreateDirectory(OutputFolder);
        Sprite icon = AssetDatabase.LoadAssetAtPath<Sprite>(PlaceholderIconPath);

        CreateSet("helmet", "투구", EquipmentType.Helmet, EquipmentStatType.MaxHp,
            new[] { 50f, 150f, 250f }, icon);
        CreateSet("armor", "갑옷", EquipmentType.Armor, EquipmentStatType.MaxHp,
            new[] { 100f, 200f, 300f }, icon);
        CreateSet("main_weapon", "주 무기", EquipmentType.MainWeapon, EquipmentStatType.Attack,
            new[] { 100f, 150f, 250f }, icon);
        CreateSet("sub_weapon", "보조 무기", EquipmentType.SubWeapon, EquipmentStatType.Attack,
            new[] { 50f, 100f, 150f }, icon);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[EquipmentDataBuilder] Rebuilt 12 equipment definitions.");
    }

    private static void CreateSet(string idPrefix, string displayName, EquipmentType type,
        EquipmentStatType statType, float[] values, Sprite icon)
    {
        Rarity[] rarities = { Rarity.Rare, Rarity.Epic, Rarity.Legend };
        for (int i = 0; i < rarities.Length; i++)
        {
            string rarityName = rarities[i].ToString().ToLowerInvariant();
            string path = $"{OutputFolder}/equipment_{idPrefix}_{rarityName}.asset";
            EquipmentDataSO data = AssetDatabase.LoadAssetAtPath<EquipmentDataSO>(path);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<EquipmentDataSO>();
                AssetDatabase.CreateAsset(data, path);
            }

            data.ItemId = $"equipment_{idPrefix}_{rarityName}";
            data.ItemName = $"{rarities[i]} {displayName}";
            data.Icon = icon;
            data.Rarity = rarities[i];
            data.Description = $"{displayName} 장비. 0강 기준 {GetStatName(statType)} +{values[i]:0}";
            data.EquipmentType = type;
            data.BaseStatType = statType;
            data.BaseStatValue = values[i];
            EditorUtility.SetDirty(data);
        }
    }

    private static string GetStatName(EquipmentStatType type) =>
        type == EquipmentStatType.MaxHp ? "최대 체력" : "공격력";
}
