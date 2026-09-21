using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class RecruitPresentationOverlayView : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    private Image backdropImage;
    private Image glowImage;
    private Image unitIconImage;
    private TMP_Text titleText;
    private TMP_Text statsText;
    private Image activeSkillIcon;
    private TMP_Text activeSkillText;
    private Image passiveSkillIcon;
    private TMP_Text passiveSkillText;
    private Button skipButton;
    private Color rarityColor;

    public event Action SkipRequested;

    public static RecruitPresentationOverlayView Create(Transform parent)
    {
        GameObject root = new("RecruitPresentation_Overlay", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.SetParent(parent, false);
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;
        rootRect.SetAsLastSibling();

        RecruitPresentationOverlayView view = root.AddComponent<RecruitPresentationOverlayView>();
        view.BuildRuntimeView();
        root.SetActive(false);
        return view;
    }

    public void ShowUnitDetail(UnitDetailViewState unit, Color color)
    {
        if (unit == null)
            return;

        rarityColor = color;
        unitIconImage.sprite = unit.Icon;
        unitIconImage.color = Color.white;
        titleText.text = $"{unit.DisplayName}\n{unit.Rarity.ToString().ToUpperInvariant()}";
        statsText.text = $"Lv {unit.Level}    공격력 {unit.Attack:0.#}    체력 {unit.MaxHp:0.#}\n" +
                         $"진급 {unit.Promotion}    한계돌파 {unit.LimitBreak}";
        BindSkill(activeSkillIcon, activeSkillText, unit.ActiveSkill, "액티브 스킬");
        BindSkill(passiveSkillIcon, passiveSkillText, unit.PassiveSkill, "패시브 스킬");
        transform.SetAsLastSibling();
        gameObject.SetActive(true);
        SetIntroProgress(0f);
    }

    public void SetIntroProgress(float progress)
    {
        progress = Mathf.Clamp01(progress);
        float eased = 1f - Mathf.Pow(1f - progress, 3f);
        float flash = Mathf.Sin(progress * Mathf.PI);

        backdropImage.raycastTarget = true;
        backdropImage.color = new Color(0.015f, 0.02f, 0.04f, Mathf.Lerp(0.35f, 0.94f, eased));
        glowImage.color = new Color(rarityColor.r, rarityColor.g, rarityColor.b, Mathf.Lerp(0f, 0.78f, flash));
        glowImage.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.2f, 1.25f, eased);
        unitIconImage.color = new Color(1f, 1f, 1f, Mathf.Clamp01((progress - 0.12f) / 0.28f));
        unitIconImage.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.78f, 1f, eased);
        titleText.color = new Color(1f, 1f, 1f, Mathf.Clamp01((progress - 0.3f) / 0.35f));
        statsText.color = new Color(1f, 1f, 1f, Mathf.Clamp01((progress - 0.36f) / 0.3f));
        SetSkillAlpha(activeSkillIcon, activeSkillText, Mathf.Clamp01((progress - 0.42f) / 0.3f));
        SetSkillAlpha(passiveSkillIcon, passiveSkillText, Mathf.Clamp01((progress - 0.42f) / 0.3f));
        canvasGroup.alpha = 1f;
    }

    public void ShowRevealMode()
    {
        backdropImage.color = Color.clear;
        backdropImage.raycastTarget = false;
        glowImage.color = Color.clear;
        unitIconImage.color = Color.clear;
        titleText.color = Color.clear;
        statsText.color = Color.clear;
        SetSkillAlpha(activeSkillIcon, activeSkillText, 0f);
        SetSkillAlpha(passiveSkillIcon, passiveSkillText, 0f);
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void BuildRuntimeView()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        backdropImage = GetComponent<Image>();

        GameObject glow = new("Glow", typeof(RectTransform), typeof(Image));
        RectTransform glowRect = glow.GetComponent<RectTransform>();
        glowRect.SetParent(transform, false);
        glowRect.anchorMin = new Vector2(0.5f, 0.5f);
        glowRect.anchorMax = new Vector2(0.5f, 0.5f);
        glowRect.sizeDelta = new Vector2(460f, 460f);
        glowImage = glow.GetComponent<Image>();
        glowImage.raycastTarget = false;

        GameObject unitIcon = new("Unit_Icon", typeof(RectTransform), typeof(Image));
        RectTransform unitIconRect = unitIcon.GetComponent<RectTransform>();
        unitIconRect.SetParent(transform, false);
        unitIconRect.anchorMin = new Vector2(0.5f, 0.5f);
        unitIconRect.anchorMax = new Vector2(0.5f, 0.5f);
        unitIconRect.anchoredPosition = new Vector2(0f, 125f);
        unitIconRect.sizeDelta = new Vector2(300f, 300f);
        unitIconImage = unitIcon.GetComponent<Image>();
        unitIconImage.preserveAspect = true;
        unitIconImage.raycastTarget = false;

        GameObject title = new("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.SetParent(transform, false);
        titleRect.anchorMin = new Vector2(0.18f, 0.3f);
        titleRect.anchorMax = new Vector2(0.82f, 0.43f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
        titleText = title.GetComponent<TextMeshProUGUI>();
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontSize = 44f;
        titleText.fontStyle = FontStyles.Bold;
        titleText.raycastTarget = false;

        statsText = CreateText(
            "Stats",
            transform,
            new Vector2(0.14f, 0.2f),
            new Vector2(0.86f, 0.3f),
            25f);

        CreateSkillRow(
            "ActiveSkill",
            new Vector2(0.12f, 0.1f),
            new Vector2(0.44f, 0.19f),
            out activeSkillIcon,
            out activeSkillText);

        CreateSkillRow(
            "PassiveSkill",
            new Vector2(0.48f, 0.1f),
            new Vector2(0.8f, 0.19f),
            out passiveSkillIcon,
            out passiveSkillText);

        GameObject buttonObject = new("Skip_Button", typeof(RectTransform), typeof(Image), typeof(Button));
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.SetParent(transform, false);
        buttonRect.anchorMin = new Vector2(0.8f, 0.02f);
        buttonRect.anchorMax = new Vector2(0.96f, 0.08f);
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;
        Image buttonImage = buttonObject.GetComponent<Image>();
        buttonImage.color = new Color(0f, 0f, 0f, 0.72f);
        skipButton = buttonObject.GetComponent<Button>();
        skipButton.targetGraphic = buttonImage;
        skipButton.onClick.AddListener(HandleSkip);

        GameObject label = new("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.SetParent(buttonObject.transform, false);
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        TMP_Text labelText = label.GetComponent<TextMeshProUGUI>();
        labelText.text = "SKIP";
        labelText.alignment = TextAlignmentOptions.Center;
        labelText.fontSize = 24f;
        labelText.fontStyle = FontStyles.Bold;
        labelText.raycastTarget = false;
    }

    private void HandleSkip()
    {
        SkipRequested?.Invoke();
    }

    private void CreateSkillRow(
        string objectName,
        Vector2 anchorMin,
        Vector2 anchorMax,
        out Image icon,
        out TMP_Text label)
    {
        GameObject row = new(objectName, typeof(RectTransform));
        RectTransform rowRect = row.GetComponent<RectTransform>();
        rowRect.SetParent(transform, false);
        rowRect.anchorMin = anchorMin;
        rowRect.anchorMax = anchorMax;
        rowRect.offsetMin = Vector2.zero;
        rowRect.offsetMax = Vector2.zero;

        GameObject iconObject = new("Icon", typeof(RectTransform), typeof(Image));
        RectTransform iconRect = iconObject.GetComponent<RectTransform>();
        iconRect.SetParent(row.transform, false);
        iconRect.anchorMin = new Vector2(0f, 0.05f);
        iconRect.anchorMax = new Vector2(0.25f, 0.95f);
        iconRect.offsetMin = Vector2.zero;
        iconRect.offsetMax = Vector2.zero;
        icon = iconObject.GetComponent<Image>();
        icon.preserveAspect = true;
        icon.raycastTarget = false;

        label = CreateText(
            "Name",
            row.transform,
            new Vector2(0.28f, 0f),
            Vector2.one,
            20f,
            TextAlignmentOptions.MidlineLeft);
    }

    private static TMP_Text CreateText(
        string objectName,
        Transform parent,
        Vector2 anchorMin,
        Vector2 anchorMax,
        float fontSize,
        TextAlignmentOptions alignment = TextAlignmentOptions.Center)
    {
        GameObject textObject = new(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        TMP_Text text = textObject.GetComponent<TextMeshProUGUI>();
        text.alignment = alignment;
        text.fontSize = fontSize;
        text.raycastTarget = false;
        return text;
    }

    private static void BindSkill(Image icon, TMP_Text label, SkillDataSO skill, string fallbackName)
    {
        if (icon != null)
            icon.sprite = skill?.icon;

        if (label != null)
            label.text = skill != null ? skill.skillName : fallbackName;
    }

    private static void SetSkillAlpha(Image icon, TMP_Text label, float alpha)
    {
        if (icon != null)
            icon.color = new Color(1f, 1f, 1f, alpha);

        if (label != null)
            label.color = new Color(1f, 1f, 1f, alpha);
    }

    private void OnDestroy()
    {
        if (skipButton != null)
            skipButton.onClick.RemoveListener(HandleSkip);
    }
}
