using System;
using System.Collections.Generic;

public sealed class ItemCatalog : IItemCatalog
{
    private readonly Dictionary<string, ItemDataSO> itemsById;

    public ItemCatalog(IEnumerable<ItemDataSO> items)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        itemsById = new Dictionary<string, ItemDataSO>(StringComparer.Ordinal);
        foreach (ItemDataSO item in items)
        {
            if (item == null)
                throw new ArgumentException("Item catalog contains a null asset.", nameof(items));
            if (string.IsNullOrWhiteSpace(item.ItemId))
                throw new ArgumentException($"Item ID is empty: {item.name}", nameof(items));
            if (!itemsById.TryAdd(item.ItemId, item))
                throw new ArgumentException($"Duplicate item ID: {item.ItemId}", nameof(items));
        }

        if (itemsById.Count == 0)
            throw new ArgumentException("Item catalog is empty.", nameof(items));
    }

    public ItemDataSO Get(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
            return null;

        itemsById.TryGetValue(itemId, out ItemDataSO item);
        return item;
    }

    public IReadOnlyCollection<ItemDataSO> GetAll() => itemsById.Values;
}
