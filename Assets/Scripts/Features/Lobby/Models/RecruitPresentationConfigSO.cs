using UnityEngine;

[CreateAssetMenu(fileName = "Recruit Presentation Config", menuName = "Game/Gacha/Recruit Presentation Config")]
public sealed class RecruitPresentationConfigSO : ScriptableObject
{
    [Header("Timing")]
    [SerializeField, Min(0f)] private float newUnitDetailDuration = 2f;
    [SerializeField, Min(0f)] private float revealInterval = 0.18f;
    [SerializeField, Min(0f)] private float cardRevealDuration = 0.22f;

    public float NewUnitDetailDuration => newUnitDetailDuration;
    public float RevealInterval => revealInterval;
    public float CardRevealDuration => cardRevealDuration;

    public Color GetRarityColor(Rarity rarity)
    {
        return RarityPalette.Get(rarity);
    }

}
