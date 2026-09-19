using UnityEngine;

[CreateAssetMenu(fileName = "Unit Visual Config", menuName = "Game/Units/Unit Visual Config")]
public sealed class UnitVisualConfigSO : ScriptableObject
{
    [Header("Rarity Colors")]
    [SerializeField] private Color normalColor = Color.blue;
    [SerializeField] private Color rareColor = new Color(0.627451f, 0.12549f, 0.941176f, 1f);
    [SerializeField] private Color legendColor = new Color(1f, 0.921569f, 0.015686f, 1f);

    [Header("Promotion")]
    [SerializeField] private Sprite[] promotionSprites;

    [Header("Limit Break")]
    [SerializeField] private Sprite filledStarSprite;
    [SerializeField] private Sprite emptyStarSprite;

    public Sprite FilledStarSprite => filledStarSprite;
    public Sprite EmptyStarSprite => emptyStarSprite;

    public Color GetRarityColor(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Normal => normalColor,
            Rarity.Rare => rareColor,
            Rarity.Legend => legendColor,
            _ => Color.white,
        };
    }

    public Sprite GetPromotionSprite(int promotion)
    {
        if (promotionSprites == null || promotion < 0 || promotion >= promotionSprites.Length)
            return null;

        return promotionSprites[promotion];
    }
}
