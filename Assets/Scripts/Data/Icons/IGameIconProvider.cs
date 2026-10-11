using UnityEngine;

public interface IGameIconProvider
{
    Sprite GetResourceIcon(RewardType type);
    string GetResourceDisplayName(RewardType type);
    Rarity GetResourceRarity(RewardType type);
}
