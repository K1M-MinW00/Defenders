using System;
using System.Text;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class StageUnitInfoPanel : MonoBehaviour
{
    private const float SlideDuration = 0.18f;
    private static readonly Color OpenColor = new(0.1f, 0.14f, 0.2f, 0.96f);
    private static readonly Color LockedColor = new(0.12f, 0.12f, 0.13f, 0.96f);

    [SerializeField] private RectTransform panelRect;
    [SerializeField] private Image portraitImage;
    [SerializeField] private TMP_Text unitNameText;
    [SerializeField] private UnitStarIconView starIcon;
    [SerializeField] private SkillCard activeCard;
    [SerializeField] private SkillCard passiveCard;
    [SerializeField] private Vector2 shownAnchoredPosition = new(0f, -24f);

    private Tween slideTween;

    public void Show(UnitController unit)
    {
        if (unit?.UnitData == null || panelRect == null)
            return;

        Render(unit);
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        PlaySlideIn();
    }

    public void Hide()
    {
        StopSlide();
        gameObject.SetActive(false);
    }

    private void Render(UnitController unit)
    {
        UnitDataSO data = unit.UnitData;
        portraitImage.sprite = data.icon;
        portraitImage.color = data.icon != null ? Color.white : Color.clear;
        unitNameText.text = data.displayName;
        starIcon?.SetStar(unit.Star);

        int promotion = unit.SkillController != null ? unit.SkillController.Promotion : 0;
        activeCard.Render(data.activeSkill, promotion, unit.Star >= 3, "잠김 - 3성부터 해금");
        passiveCard.Render(data.passiveSkill, promotion, promotion >= 1, "잠김 - 1진급부터 해금");
    }

    private void PlaySlideIn()
    {
        StopSlide();
        Canvas.ForceUpdateCanvases();
        float startX = shownAnchoredPosition.x - panelRect.rect.width - 24f;
        panelRect.anchoredPosition = new Vector2(startX, shownAnchoredPosition.y);

        Tween createdTween = panelRect
            .DOAnchorPos(shownAnchoredPosition, SlideDuration)
            .SetEase(Ease.OutCubic)
            .BindTo(this, useUnscaledTime: true);

        slideTween = createdTween;
        createdTween.OnComplete(() =>
        {
            if (slideTween == createdTween)
                slideTween = null;
        });
    }

    private void StopSlide()
    {
        TweenLifecycle.Kill(ref slideTween);
    }

    private static string BuildSkillDescription(SkillDataSO skill, int promotion)
    {
        if (skill?.upgrades == null || skill.upgrades.Count == 0)
            return "스킬 정보가 없습니다.";

        StringBuilder builder = new();
        foreach (SkillUpgradeData upgrade in skill.upgrades)
        {
            if (upgrade == null)
                continue;

            if (builder.Length > 0)
                builder.AppendLine();
            if (promotion < upgrade.promotionLevel)
                builder.Append($"[진급 {upgrade.promotionLevel} 효과] ");
            builder.Append(upgrade.description);
        }

        return builder.Length > 0 ? builder.ToString() : "스킬 정보가 없습니다.";
    }

    private void OnDisable() => StopSlide();

    [Serializable]
    public sealed class SkillCard
    {
        [SerializeField] private Image background;
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text name;
        [SerializeField] private TMP_Text description;
        [SerializeField] private TMP_Text lockText;

        public void Render(SkillDataSO skill, int promotion, bool unlocked, string lockedMessage)
        {
            background.color = unlocked ? OpenColor : LockedColor;
            icon.sprite = skill?.icon;
            icon.color = skill?.icon != null
                ? unlocked ? Color.white : new Color(0.45f, 0.45f, 0.45f)
                : Color.clear;
            name.text = skill != null ? skill.skillName : "스킬 없음";
            name.color = unlocked ? Color.white : new Color(0.65f, 0.65f, 0.65f);
            description.text = BuildSkillDescription(skill, promotion);
            description.color = unlocked ? Color.white : new Color(0.62f, 0.62f, 0.62f);
            lockText.gameObject.SetActive(!unlocked);
            lockText.text = unlocked ? string.Empty : lockedMessage;
        }
    }
}
