using UnityEngine;

public readonly struct RewardPresentation
{
    public Sprite Icon { get; }
    public Rarity Rarity { get; }
    public string DisplayName { get; }
    public bool Countable { get; }

    public RewardPresentation(Sprite icon, Rarity rarity, string displayName, bool countable)
    {
        Icon = icon;
        Rarity = rarity;
        DisplayName = displayName;
        Countable = countable;
    }
}

public static class RewardPresentationResolver
{
    public static bool TryResolve(
        RewardData reward,
        IGameIconProvider icons,
        out RewardPresentation presentation)
    {
        presentation = default;
        if (reward == null || icons == null)
            return false;

        if (reward.Type is RewardType.Gold or RewardType.Gem or RewardType.Fuel or RewardType.Experience or RewardType.ResearchMaterial)
        {
            presentation = new RewardPresentation(
                icons.GetResourceIcon(reward.Type),
                icons.GetResourceRarity(reward.Type),
                GetLocalizedResourceName(reward.Type),
                true);
            return true;
        }

        if (reward.Type is RewardType.Item or RewardType.Equipment)
        {
            ItemDataSO item = GameConfig.Items?.Get(reward.Id);
            if (item == null)
                return false;

            presentation = new RewardPresentation(
                item.Icon,
                item.Rarity,
                item.LocalizedName,
                reward.Type == RewardType.Item && item.Stackable);
            return true;
        }

        if (reward.Type == RewardType.Unit)
        {
            UnitDataSO unit = GameConfig.Units?.Get(reward.Id);
            if (unit == null)
                return false;

            presentation = new RewardPresentation(
                unit.icon,
                unit.rarity,
                unit.LocalizedName,
                false);
            return true;
        }

        return false;
    }

    private static string GetLocalizedResourceName(RewardType type)
    {
        string key = type switch
        {
            RewardType.Gold => "resource.gold",
            RewardType.Gem => "resource.gem",
            RewardType.Fuel => "resource.fuel",
            RewardType.Experience => "resource.experience",
            RewardType.ResearchMaterial => "resource.research_material",
            _ => string.Empty,
        };

        return LocalizationService.Get(LocalizationTables.Common, key);
    }
}
