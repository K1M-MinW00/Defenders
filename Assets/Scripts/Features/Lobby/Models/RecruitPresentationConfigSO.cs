using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Recruit Presentation Config", menuName = "Game/Gacha/Recruit Presentation Config")]
public sealed class RecruitPresentationConfigSO : ScriptableObject
{
    [Serializable]
    private struct RarityPresentation
    {
        public Rarity rarity;
        public Color color;
    }

    [Header("Timing")]
    [SerializeField, Min(0f)] private float newUnitDetailDuration = 2f;
    [SerializeField, Min(0f)] private float revealInterval = 0.18f;
    [SerializeField, Min(0f)] private float cardRevealDuration = 0.22f;

    [Header("Rarity Presentation")]
    [SerializeField] private RarityPresentation[] rarityPresentations;

    public float NewUnitDetailDuration => newUnitDetailDuration;
    public float RevealInterval => revealInterval;
    public float CardRevealDuration => cardRevealDuration;

    public Color GetRarityColor(Rarity rarity)
    {
        if (rarityPresentations != null)
        {
            foreach (RarityPresentation presentation in rarityPresentations)
            {
                if (presentation.rarity == rarity && presentation.color.a > 0f)
                    return presentation.color;
            }
        }

        return rarity switch
        {
            Rarity.Normal => new Color(0.08f, 0.28f, 1f, 1f),
            Rarity.Rare => new Color(0.63f, 0.13f, 0.94f, 1f),
            Rarity.Legend => new Color(1f, 0.92f, 0.02f, 1f),
            _ => Color.white,
        };
    }

}
