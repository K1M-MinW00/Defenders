using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Owns the temporary material selection used by the equipment enhancement UI.
/// It has no Unity or user-data dependency, so selection rules can be tested
/// without opening a scene.
/// </summary>
public sealed class EquipmentEnhancementSelection
{
    private readonly IItemCatalog catalog;
    private readonly Dictionary<string, int> quantities = new(StringComparer.Ordinal);

    public EquipmentEnhancementSelection(IItemCatalog catalog)
    {
        this.catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
    }

    public int Count => quantities.Count;

    public void Clear() => quantities.Clear();

    public int GetQuantity(string itemId) =>
        !string.IsNullOrWhiteSpace(itemId) && quantities.TryGetValue(itemId, out int quantity)
            ? quantity
            : 0;

    public int GetTotalExperience()
    {
        long total = 0;
        foreach ((string itemId, int quantity) in quantities)
            total += (long)GetMaterialValue(itemId) * quantity;

        return total > int.MaxValue ? int.MaxValue : (int)total;
    }

    public bool TryAdd(string itemId, int ownedQuantity, int requiredExperience)
    {
        if (requiredExperience <= 0 || GetTotalExperience() >= requiredExperience)
            return false;

        int value = GetMaterialValue(itemId);
        int selected = GetQuantity(itemId);
        if (value <= 0 || ownedQuantity <= selected)
            return false;

        quantities[itemId] = selected + 1;
        return true;
    }

    public bool TryRemove(string itemId)
    {
        if (!quantities.TryGetValue(itemId, out int selected))
            return false;

        if (selected <= 1)
            quantities.Remove(itemId);
        else
            quantities[itemId] = selected - 1;

        return true;
    }

    public bool TryAutoSelect(IEnumerable<InventoryStackItem> materials, int requiredExperience)
    {
        if (requiredExperience <= 0 || materials == null)
            return false;

        List<MaterialCandidate> candidates = materials
            .Where(item => item != null && item.Count > 0)
            .Select(item => new MaterialCandidate(item.ItemId, GetMaterialValue(item.ItemId), item.Count))
            .Where(item => item.Value > 0)
            .OrderBy(item => item.Value)
            .ToList();

        long availableExperience = candidates.Sum(item => (long)item.Value * item.OwnedQuantity);
        if (availableExperience < requiredExperience)
            return false;

        Dictionary<string, int> next = new(StringComparer.Ordinal);
        int remaining = requiredExperience;
        foreach (MaterialCandidate candidate in candidates)
        {
            int quantity = Math.Min(
                candidate.OwnedQuantity,
                (int)Math.Ceiling((double)remaining / candidate.Value));

            if (quantity > 0)
            {
                next[candidate.ItemId] = quantity;
                remaining -= quantity * candidate.Value;
            }

            if (remaining <= 0)
                break;
        }

        quantities.Clear();
        foreach ((string itemId, int quantity) in next)
            quantities[itemId] = quantity;
        return true;
    }

    public Dictionary<string, int> CreateSnapshot() =>
        new(quantities, StringComparer.Ordinal);

    private int GetMaterialValue(string itemId) =>
        catalog.Get(itemId) is MaterialDataSO material &&
        material.MaterialType == MaterialType.Equipment
            ? Math.Max(0, material.Value)
            : 0;

    private readonly struct MaterialCandidate
    {
        public MaterialCandidate(string itemId, int value, int ownedQuantity)
        {
            ItemId = itemId;
            Value = value;
            OwnedQuantity = ownedQuantity;
        }

        public string ItemId { get; }
        public int Value { get; }
        public int OwnedQuantity { get; }
    }
}
