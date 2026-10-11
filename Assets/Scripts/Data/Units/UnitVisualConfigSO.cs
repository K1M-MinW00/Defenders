using UnityEngine;

[CreateAssetMenu(fileName = "Unit Visual Config", menuName = "Game/Units/Unit Visual Config")]
public sealed class UnitVisualConfigSO : ScriptableObject
{
    [Header("Promotion")]
    [SerializeField] private Sprite[] promotionSprites;

    [Header("Limit Break")]
    [SerializeField] private Sprite filledStarSprite;
    [SerializeField] private Sprite emptyStarSprite;

    public Sprite FilledStarSprite => filledStarSprite;
    public Sprite EmptyStarSprite => emptyStarSprite;

    public Color GetRarityColor(Rarity rarity)
    {
        return RarityPalette.Get(rarity);
    }

    public Sprite GetPromotionSprite(int promotion)
    {
        if (promotionSprites == null || promotion < 0 || promotion >= promotionSprites.Length)
            return null;

        return promotionSprites[promotion];
    }
}
