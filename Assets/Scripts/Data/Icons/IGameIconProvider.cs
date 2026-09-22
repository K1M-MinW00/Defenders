using UnityEngine;

public interface IGameIconProvider
{
    Sprite GetResourceIcon(RewardType type);
    Sprite GetRarityFrame(Rarity rarity);
}
