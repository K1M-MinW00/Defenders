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

    public void ShowUnitDetail(UnitDataSO unit, Color color)
    {
        if (unit == null)
            return;

        rarityColor = color;
        unitIconImage.sprite = unit.icon;
        unitIconImage.color = Color.white;
        titleText.text = $"{unit.displayName}\n{unit.rarity.ToString().ToUpperInvariant()}";
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
        canvasGroup.alpha = 1f;
    }

    public void ShowRevealMode()
    {
        backdropImage.color = Color.clear;
        backdropImage.raycastTarget = false;
        glowImage.color = Color.clear;
        unitIconImage.color = Color.clear;
        titleText.color = Color.clear;
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
        unitIconRect.anchoredPosition = new Vector2(0f, 70f);
        unitIconRect.sizeDelta = new Vector2(320f, 320f);
        unitIconImage = unitIcon.GetComponent<Image>();
        unitIconImage.preserveAspect = true;
        unitIconImage.raycastTarget = false;

        GameObject title = new("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.SetParent(transform, false);
        titleRect.anchorMin = new Vector2(0.18f, 0.16f);
        titleRect.anchorMax = new Vector2(0.82f, 0.36f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
        titleText = title.GetComponent<TextMeshProUGUI>();
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontSize = 44f;
        titleText.fontStyle = FontStyles.Bold;
        titleText.raycastTarget = false;

        GameObject buttonObject = new("Skip_Button", typeof(RectTransform), typeof(Image), typeof(Button));
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.SetParent(transform, false);
        buttonRect.anchorMin = new Vector2(0.78f, 0.04f);
        buttonRect.anchorMax = new Vector2(0.96f, 0.12f);
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

    private void OnDestroy()
    {
        if (skipButton != null)
            skipButton.onClick.RemoveListener(HandleSkip);
    }
}
