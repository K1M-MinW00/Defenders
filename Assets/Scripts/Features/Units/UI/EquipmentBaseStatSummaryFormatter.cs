using System;
using System.Collections.Generic;
using System.Linq;

public static class EquipmentBaseStatSummaryFormatter
{
    public static IReadOnlyList<string> Build(
        IEnumerable<EquipmentItemData> equipmentItems,
        IItemCatalog catalog,
        EquipmentBalanceConfigSO balance)
    {
        if (equipmentItems == null || catalog == null || balance == null)
            return Array.Empty<string>();

        return equipmentItems
            .Where(item => item != null)
            .Select(item => new
            {
                Definition = catalog.Get(item.ItemId) as EquipmentDataSO,
                Item = item,
            })
            .Where(entry => entry.Definition != null)
            .GroupBy(entry => entry.Definition.BaseStatType)
            .OrderBy(group => group.Key)
            .Select(group => Format(
                group.Key,
                group.Sum(entry => balance.GetBaseStatValue(entry.Definition, entry.Item.Level))))
            .ToArray();
    }

    private static string Format(EquipmentStatType statType, float value)
    {
        string statName = statType switch
        {
            EquipmentStatType.MaxHp => "최대 체력",
            EquipmentStatType.Attack => "공격력",
            EquipmentStatType.CritChance => "치명타 확률",
            _ => "받는 피해 감소",
        };

        return $"{statName}  +{value:0.#}";
    }
}
