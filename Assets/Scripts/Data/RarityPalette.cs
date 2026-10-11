using UnityEngine;

public static class RarityPalette
{
    public static readonly Color Normal = new(0.72f, 0.72f, 0.72f, 1f);
    public static readonly Color Rare = new(0.35f, 0.68f, 1f, 1f);
    public static readonly Color Epic = new(0.72f, 0.45f, 1f, 1f);
    public static readonly Color Legend = new(1f, 0.72f, 0.12f, 1f);

    public static Color Get(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Rare => Rare,
            Rarity.Epic => Epic,
            Rarity.Legend => Legend,
            _ => Normal,
        };
    }
}
