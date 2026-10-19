using UnityEngine;

public abstract class ItemDataSO : ScriptableObject
{
    [Header("Basic")]
    public string ItemId;
    public string ItemName;
    public Sprite Icon;
    public Rarity Rarity;
    [TextArea]
    public string Description;

    public string LocalizedName => Localize("name", ItemName);
    public string LocalizedDescription => Localize("description", Description);

    public abstract ItemCategory Category { get; }

    public abstract bool Stackable { get; }

    private string Localize(string suffix, string fallback)
    {
        if (string.IsNullOrWhiteSpace(ItemId))
            return fallback ?? string.Empty;

        string value = LocalizationService.Get(LocalizationTables.Items, $"item.{ItemId}.{suffix}");
        return string.IsNullOrWhiteSpace(value) || value.StartsWith("No translation found for")
            ? fallback ?? string.Empty
            : value;
    }
}

public enum ItemCategory
{
    Material,
    Consumable,
    Equipment
}
