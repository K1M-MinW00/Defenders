using System;
using System.Collections.Generic;
using System.Linq;

public enum InventoryMutationFailure
{
    None,
    InvalidInventory,
    InvalidItem,
    InvalidAmount,
    InsufficientItems,
    Overflow,
}

public readonly struct InventoryMutationResult
{
    public bool Succeeded => Failure == InventoryMutationFailure.None;
    public InventoryMutationFailure Failure { get; }

    private InventoryMutationResult(InventoryMutationFailure failure)
    {
        Failure = failure;
    }

    public static InventoryMutationResult Success() => new(InventoryMutationFailure.None);
    public static InventoryMutationResult Fail(InventoryMutationFailure failure) => new(failure);
}

/// <summary>
/// Applies validated item changes to an inventory instance.
/// Persistence and user-data commits remain the responsibility of the calling use case.
/// </summary>
public sealed class InventoryMutationService
{
    private readonly IItemCatalog catalog;

    public InventoryMutationService(IItemCatalog catalog)
    {
        this.catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
    }

    public int GetCount(UserInventoryData inventory, string itemId)
    {
        if (inventory == null || string.IsNullOrWhiteSpace(itemId))
            return 0;

        ItemDataSO definition = catalog.Get(itemId);
        if (definition == null)
            return 0;

        if (definition.Category == ItemCategory.Equipment)
            return inventory.Equipments?.Count(item => item != null && item.ItemId == itemId) ?? 0;

        List<InventoryStackItem> stacks = GetStacks(inventory, definition.Category);
        return stacks?.FirstOrDefault(item => item != null && item.ItemId == itemId)?.Count ?? 0;
    }

    public InventoryMutationResult Add(UserInventoryData inventory, string itemId, int amount)
    {
        InventoryMutationResult validation = Validate(inventory, itemId, amount, out ItemDataSO definition);
        if (!validation.Succeeded)
            return validation;

        if (definition.Category == ItemCategory.Equipment)
        {
            if (inventory.Equipments == null)
                return InventoryMutationResult.Fail(InventoryMutationFailure.InvalidInventory);

            for (int i = 0; i < amount; i++)
            {
                inventory.Equipments.Add(new EquipmentItemData
                {
                    UniqueId = Guid.NewGuid().ToString(),
                    ItemId = itemId,
                    Level = 1,
                });
            }

            return InventoryMutationResult.Success();
        }

        if (!definition.Stackable)
            return InventoryMutationResult.Fail(InventoryMutationFailure.InvalidItem);

        List<InventoryStackItem> stacks = GetStacks(inventory, definition.Category);
        if (stacks == null)
            return InventoryMutationResult.Fail(InventoryMutationFailure.InvalidInventory);

        InventoryStackItem owned = stacks.FirstOrDefault(item => item != null && item.ItemId == itemId);
        if (owned == null)
        {
            stacks.Add(new InventoryStackItem { ItemId = itemId, Count = amount });
            return InventoryMutationResult.Success();
        }

        try
        {
            owned.Count = checked(owned.Count + amount);
            return InventoryMutationResult.Success();
        }
        catch (OverflowException)
        {
            return InventoryMutationResult.Fail(InventoryMutationFailure.Overflow);
        }
    }

    public InventoryMutationResult Consume(UserInventoryData inventory, string itemId, int amount)
    {
        InventoryMutationResult validation = Validate(inventory, itemId, amount, out ItemDataSO definition);
        if (!validation.Succeeded)
            return validation;

        if (!definition.Stackable || definition.Category == ItemCategory.Equipment)
            return InventoryMutationResult.Fail(InventoryMutationFailure.InvalidItem);

        List<InventoryStackItem> stacks = GetStacks(inventory, definition.Category);
        if (stacks == null)
            return InventoryMutationResult.Fail(InventoryMutationFailure.InvalidInventory);

        InventoryStackItem owned = stacks.FirstOrDefault(item => item != null && item.ItemId == itemId);
        if (owned == null || owned.Count < amount)
            return InventoryMutationResult.Fail(InventoryMutationFailure.InsufficientItems);

        owned.Count -= amount;
        if (owned.Count == 0)
            stacks.Remove(owned);

        return InventoryMutationResult.Success();
    }

    public InventoryMutationResult ConsumeBatch(
        UserInventoryData inventory,
        IReadOnlyDictionary<string, int> items)
    {
        if (inventory == null)
            return InventoryMutationResult.Fail(InventoryMutationFailure.InvalidInventory);
        if (items == null || items.Count == 0)
            return InventoryMutationResult.Fail(InventoryMutationFailure.InvalidAmount);

        foreach (KeyValuePair<string, int> pair in items)
        {
            InventoryMutationResult validation = Validate(inventory, pair.Key, pair.Value, out ItemDataSO definition);
            if (!validation.Succeeded)
                return validation;
            if (!definition.Stackable || definition.Category == ItemCategory.Equipment)
                return InventoryMutationResult.Fail(InventoryMutationFailure.InvalidItem);
            if (GetCount(inventory, pair.Key) < pair.Value)
                return InventoryMutationResult.Fail(InventoryMutationFailure.InsufficientItems);
        }

        foreach (KeyValuePair<string, int> pair in items)
            Consume(inventory, pair.Key, pair.Value);

        return InventoryMutationResult.Success();
    }

    private InventoryMutationResult Validate(
        UserInventoryData inventory,
        string itemId,
        int amount,
        out ItemDataSO definition)
    {
        definition = null;
        if (inventory == null)
            return InventoryMutationResult.Fail(InventoryMutationFailure.InvalidInventory);
        if (string.IsNullOrWhiteSpace(itemId))
            return InventoryMutationResult.Fail(InventoryMutationFailure.InvalidItem);
        if (amount <= 0)
            return InventoryMutationResult.Fail(InventoryMutationFailure.InvalidAmount);

        definition = catalog.Get(itemId);
        return definition == null
            ? InventoryMutationResult.Fail(InventoryMutationFailure.InvalidItem)
            : InventoryMutationResult.Success();
    }

    private static List<InventoryStackItem> GetStacks(UserInventoryData inventory, ItemCategory category)
    {
        return category switch
        {
            ItemCategory.Material => inventory.Materials,
            ItemCategory.Consumable => inventory.Consumables,
            _ => null,
        };
    }
}
