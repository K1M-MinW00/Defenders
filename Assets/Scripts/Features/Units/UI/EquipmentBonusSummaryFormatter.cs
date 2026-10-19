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
        return EquipmentLocalizedText.BonusValue(statType, modifierType, value);
    }
}
