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

    public abstract ItemCategory Category { get; }

    public abstract bool Stackable { get; }
}

public enum ItemCategory
{
    Material,
    Consumable,
    Equipment
} 