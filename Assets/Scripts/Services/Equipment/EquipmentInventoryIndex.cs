using System;
using System.Collections.Generic;
using System.Linq;

public sealed class EquipmentInventoryIndex
{
    public const int UnequippedCapacity = 100;

    private readonly Dictionary<string, EquipmentItemData> equipmentById;
    private readonly HashSet<string> equippedIds;

    public EquipmentInventoryIndex(UserInventoryData inventory, UserRosterData roster)
    {
        equipmentById = inventory?.Equipments?
            .Where(item => item != null && !string.IsNullOrWhiteSpace(item.UniqueId))
            .GroupBy(item => item.UniqueId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal)
            ?? new Dictionary<string, EquipmentItemData>(StringComparer.Ordinal);

        equippedIds = new HashSet<string>(StringComparer.Ordinal);
        if (roster?.OwnedUnits == null)
            return;

        foreach (UserUnitData unit in roster.OwnedUnits)
        {
            if (unit?.Equipment == null)
                continue;

            foreach (string equipmentId in unit.Equipment.EnumerateIds())
                equippedIds.Add(equipmentId);
        }
    }

    public int UnequippedCount => equipmentById.Count - equippedIds.Count(id => equipmentById.ContainsKey(id));
    public bool IsFull => UnequippedCount >= UnequippedCapacity;
    public bool IsEquipped(string uniqueId) => !string.IsNullOrWhiteSpace(uniqueId) && equippedIds.Contains(uniqueId);
    public bool TryGet(string uniqueId, out EquipmentItemData equipment) => equipmentById.TryGetValue(uniqueId, out equipment);

    public IReadOnlyList<EquipmentItemData> GetUnequipped() => equipmentById.Values
        .Where(item => !equippedIds.Contains(item.UniqueId))
        .ToList();
}
