using System;
using UnityEngine;

public sealed class GameIconProviderTests
{
    public void ProjectIconSet_IsComplete()
    {
        GameIconSetSO iconSet = Resources.Load<GameIconSetSO>("GameData/Catalogs/GameIconSet");

        Assert(iconSet != null, "Game icon set should be available from Resources.");
        Assert(iconSet.TryValidate(out string error), $"Game icon set should be complete: {error}");
    }

    public void Provider_MapsResourcePresentation()
    {
        GameIconSetSO iconSet = Resources.Load<GameIconSetSO>("GameData/Catalogs/GameIconSet");
        GameIconProvider provider = new(iconSet);

        Assert(provider.GetResourceIcon(RewardType.Gold) == iconSet.Gold, "Gold icon should come from the configured icon set.");
        Assert(provider.GetResourceIcon(RewardType.Gem) == iconSet.Gem, "Gem icon should come from the configured icon set.");
        Assert(provider.GetResourceIcon(RewardType.Fuel) == iconSet.Fuel, "Fuel icon should come from the configured icon set.");
        Assert(provider.GetResourceIcon(RewardType.Experience) != null, "Experience should use its configured icon or a placeholder.");
        Assert(provider.GetResourceIcon(RewardType.Item) == null, "Non-resource reward types should not resolve a resource icon.");
        Assert(provider.GetResourceDisplayName(RewardType.Gold) == "골드", "Gold display name should come from the configured icon set.");
        Assert(provider.GetResourceRarity(RewardType.Gold) == Rarity.Normal, "Gold rarity should come from the configured icon set.");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }
}
