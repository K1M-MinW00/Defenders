using UnityEngine;

public static class GameIconDatabase
{
    private static GameIconSetSO iconSet;

    public static void Initialize()
    {
        if (iconSet != null)
            return;

        iconSet = Resources.Load<GameIconSetSO>("Database/GameIconSet");

        if (iconSet == null)
        {
            Debug.LogError("GameIconSetSO Not Found");
        }
    }

    public static Sprite GetResourceIcon(RewardType type)
    {
        switch (type)
        {
            case RewardType.Gold:
                return iconSet.GoldIcon;

            case RewardType.Gem:
                return iconSet.GemIcon;

            case RewardType.Fuel:
                return iconSet.FuelIcon;
        }

        return null;
    }

    public static Sprite GetRarityFrame(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Normal:
                return iconSet.NormalFrame;

            case Rarity.Rare:
                return iconSet.RareFrame;

            case Rarity.Legend:
                return iconSet.LegendFrame;
        }

        return iconSet.NormalFrame;
    }
}