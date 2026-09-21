using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Recruit Presentation Config", menuName = "Game/Gacha/Recruit Presentation Config")]
public sealed class RecruitPresentationConfigSO : ScriptableObject
{
    [Serializable]
    private struct RarityPresentation
    {
        public Rarity rarity;
        public string animatorTrigger;
        public AudioClip introClip;
    }

    [Header("Timing")]
    [SerializeField, Min(0f)] private float introDuration = 0.65f;
    [SerializeField, Min(0f)] private float revealInterval = 0.18f;

    [Header("Rarity Presentation")]
    [SerializeField] private RarityPresentation[] rarityPresentations;

    public float IntroDuration => introDuration;
    public float RevealInterval => revealInterval;

    public string GetAnimatorTrigger(Rarity rarity)
    {
        if (rarityPresentations == null)
            return string.Empty;

        foreach (RarityPresentation presentation in rarityPresentations)
        {
            if (presentation.rarity == rarity)
                return presentation.animatorTrigger;
        }

        return string.Empty;
    }

    public AudioClip GetIntroClip(Rarity rarity)
    {
        if (rarityPresentations == null)
            return null;

        foreach (RarityPresentation presentation in rarityPresentations)
        {
            if (presentation.rarity == rarity)
                return presentation.introClip;
        }

        return null;
    }
}
