using UnityEngine;

public readonly struct RewardPresentation
{
    public Sprite Icon { get; }
    public Sprite Frame { get; }
    public string DisplayName { get; }
    public bool ShowAmount { get; }

    public RewardPresentation(Sprite icon, Sprite frame, string displayName, bool showAmount)
    {
        Icon = icon;
        Frame = frame;
        DisplayName = displayName;
        ShowAmount = showAmount;
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
                icons.GetRarityFrame(Rarity.Normal),
                GetResourceName(reward.Type),
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
                icons.GetRarityFrame(item.Rarity),
                item.ItemName,
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
                icons.GetRarityFrame(unit.rarity),
                unit.displayName,
                false);
            return true;
        }

        return false;
    }

    private static string GetResourceName(RewardType type)
    {
        return type switch
        {
            RewardType.Gold => "골드",
            RewardType.Gem => "Gem",
            RewardType.Fuel => "연료",
            RewardType.Experience => "경험치",
            RewardType.ResearchMaterial => "연구 재료",
            _ => type.ToString(),
        };
    }
}
