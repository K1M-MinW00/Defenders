using System.Collections.Generic;
using System.Linq;

public static class EquipmentBonusSummaryFormatter
{
    public static IReadOnlyList<string> Build(IEnumerable<EquipmentItemData> equipmentItems)
    {
        if (equipmentItems == null)
            return System.Array.Empty<string>();

        return equipmentItems
            .Where(item => item != null &&
                item.Level >= EquipmentBalanceConfigSO.MaxLevel &&
                item.BonusOption != null)
            .GroupBy(item => (item.BonusOption.StatType, item.BonusOption.ModifierType))
            .OrderBy(group => group.Key.StatType)
            .ThenBy(group => group.Key.ModifierType)
            .Select(group => Format(group.Key.StatType, group.Key.ModifierType,
                group.Sum(item => item.BonusOption.Value)))
            .ToArray();
    }

    private static string Format(EquipmentStatType statType, EquipmentModifierType modifierType, float value)
    {
        string statName = statType switch
        {
            EquipmentStatType.MaxHp => "최대 체력",
            EquipmentStatType.Attack => "공격력",
            EquipmentStatType.CritChance => "치명타 확률",
            _ => "받는 피해 감소",
        };
        string suffix = modifierType == EquipmentModifierType.Percent ? "%" : string.Empty;
        return $"{statName}  +{value:0.#}{suffix}";
    }
}
