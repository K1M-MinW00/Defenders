using System;
using UnityEngine;

public sealed class GameIconProvider : IGameIconProvider
{
    private readonly GameIconSetSO iconSet;

    public GameIconProvider(GameIconSetSO iconSet)
    {
        this.iconSet = iconSet != null
            ? iconSet
            : throw new ArgumentNullException(nameof(iconSet));

        if (!iconSet.TryValidate(out string error))
            throw new ArgumentException(error, nameof(iconSet));
    }

    public Sprite GetResourceIcon(RewardType type)
    {
        return type switch
        {
            RewardType.Gold => iconSet.Gold,
            RewardType.Gem => iconSet.Gem,
            RewardType.Fuel => iconSet.Fuel,
            RewardType.Experience => iconSet.Experience,
            RewardType.ResearchMaterial => iconSet.ResearchMaterial,
            _ => null,
        };
    }

    public Sprite GetRarityFrame(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Normal => iconSet.NormalRarityFrame,
            Rarity.Rare => iconSet.RareRarityFrame,
            Rarity.Legend => iconSet.LegendRarityFrame,
            _ => iconSet.NormalRarityFrame,
        };
    }
}
