using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class RecruitPresentationOverlayView : MonoBehaviour
{
    private const int MaxRayCount = 16;
    private const int MaxStarCount = 10;

    private readonly List<Image> rays = new();
    private readonly List<Image> stars = new();
    private readonly List<RectTransform> bands = new();
    private readonly List<Vector2> bandPositions = new();

    private Image rootBlocker;
    private GameObject introContent;
    private Image backdropImage;
    private Image unitIconImage;
    private Image namePanelImage;
    private TMP_Text newUnitText;
    private TMP_Text unitNameText;
    private TMP_Text rarityText;
    private Button skipButton;
    private Color rarityColor;
    private float effectStrength;

    public event Action SkipRequested;

    public static RecruitPresentationOverlayView Create(Transform parent)
    {
        GameObject root = new("RecruitPresentation_Overlay", typeof(RectTransform), typeof(Image));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.SetParent(parent, false);
        Stretch(rootRect);
        rootRect.SetAsLastSibling();

        RecruitPresentationOverlayView view = root.AddComponent<RecruitPresentationOverlayView>();
        view.BuildRuntimeView();
        root.SetActive(false);
        return view;
    }

    public void ShowUnitIntro(UnitDataSO unit, Color color)
    {
        if (unit == null)
            return;

        rarityColor = color;
        ConfigureRarity(unit.rarity);
        unitIconImage.sprite = unit.icon;
        unitNameText.text = unit.displayName;
        rarityText.text = unit.rarity.ToString().ToUpperInvariant();

        transform.SetAsLastSibling();
        gameObject.SetActive(true);
        introContent.SetActive(true);
        rootBlocker.raycastTarget = false;
        SetIntroProgress(0f);
    }

    public void SetIntroProgress(float progress)
    {
        progress = Mathf.Clamp01(progress);
        float eased = 1f - Mathf.Pow(1f - progress, 3f);
        float pulse = Mathf.Sin(progress * Mathf.PI);
        float contentAlpha = Mathf.Clamp01(progress / 0.22f);

        backdropImage.color = GetBackdropColor(rarityColor, Mathf.Lerp(0.72f, 0.98f, eased));
        unitIconImage.color = new Color(1f, 1f, 1f, contentAlpha);
        unitIconImage.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.55f, 1.04f, eased);
        newUnitText.color = new Color(rarityColor.r, rarityColor.g, rarityColor.b, contentAlpha);
        unitNameText.color = new Color(1f, 1f, 1f, contentAlpha);
        rarityText.color = new Color(rarityColor.r, rarityColor.g, rarityColor.b, contentAlpha);
        namePanelImage.color = new Color(0.025f, 0.04f, 0.1f, 0.82f * contentAlpha);

        for (int i = 0; i < bands.Count; i++)
        {
            float direction = i % 2 == 0 ? -1f : 1f;
            bands[i].anchoredPosition = bandPositions[i] + new Vector2(direction * Mathf.Lerp(700f, 0f, eased), 0f);
        }

        for (int i = 0; i < rays.Count; i++)
        {
            Image ray = rays[i];
            Color rayColor = rarityColor;
            rayColor.a = pulse * effectStrength * (i % 2 == 0 ? 0.22f : 0.12f);
            ray.color = rayColor;
            ray.rectTransform.localScale = new Vector3(1f, Mathf.Lerp(0.25f, 1f, eased), 1f);
        }

        for (int i = 0; i < stars.Count; i++)
        {
            Image star = stars[i];
            float stagger = Mathf.Repeat(progress * 2.2f + i * 0.19f, 1f);
            Color starColor = i % 2 == 0 ? rarityColor : Color.white;
            starColor.a = Mathf.Sin(stagger * Mathf.PI) * effectStrength;
            star.color = starColor;
            star.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.45f, 1.25f, stagger);
        }
    }

    public void ShowRevealMode()
    {
        introContent.SetActive(false);
        rootBlocker.color = Color.clear;
        rootBlocker.raycastTarget = false;
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void BuildRuntimeView()
    {
        rootBlocker = GetComponent<Image>();
        rootBlocker.color = Color.clear;
        rootBlocker.raycastTarget = false;

        introContent = new GameObject("Intro_Content", typeof(RectTransform), typeof(Image));
        RectTransform contentRect = introContent.GetComponent<RectTransform>();
        contentRect.SetParent(transform, false);
        Stretch(contentRect);
        backdropImage = introContent.GetComponent<Image>();
        backdropImage.raycastTarget = true;

        CreateBands(contentRect);
        CreateRays(contentRect);
        CreateStars(contentRect);
        CreateNewUnitLabel(contentRect);
        CreateUnitPortrait(contentRect);
        CreateNamePanel(contentRect);
        CreateSkipButton();
    }

    private void CreateBands(Transform parent)
    {
        float[] heights = { 80f, 130f, 60f };
        float[] yPositions = { 360f, -60f, -470f };

        for (int i = 0; i < heights.Length; i++)
        {
            GameObject bandObject = new($"Diagonal_Band_{i}", typeof(RectTransform), typeof(Image));
            RectTransform rect = bandObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(1800f, heights[i]);
            rect.anchoredPosition = new Vector2(0f, yPositions[i]);
            rect.localRotation = Quaternion.Euler(0f, 0f, 18f);
            bands.Add(rect);
            bandPositions.Add(rect.anchoredPosition);
        }
    }

    private void CreateRays(Transform parent)
    {
        for (int i = 0; i < MaxRayCount; i++)
        {
            GameObject rayObject = new($"Ray_{i}", typeof(RectTransform), typeof(Image));
            RectTransform rect = rayObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(i % 2 == 0 ? 44f : 24f, 620f);
            rect.anchoredPosition = new Vector2(0f, -40f);
            rect.localRotation = Quaternion.Euler(0f, 0f, i * (360f / MaxRayCount));
            Image image = rayObject.GetComponent<Image>();
            image.raycastTarget = false;
            rays.Add(image);
        }
    }

    private void CreateStars(Transform parent)
    {
        Vector2[] positions =
        {
            new(-360f, 510f), new(340f, 440f), new(-420f, 120f), new(390f, 30f), new(-330f, -360f),
            new(330f, -430f), new(-120f, 590f), new(150f, -560f), new(-455f, -540f), new(445f, 570f),
        };

        for (int i = 0; i < MaxStarCount; i++)
        {
            GameObject starObject = new($"Star_{i}", typeof(RectTransform), typeof(Image));
            RectTransform rect = starObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = Vector2.one * (i % 3 == 0 ? 28f : 18f);
            rect.anchoredPosition = positions[i];
            rect.localRotation = Quaternion.Euler(0f, 0f, 45f);
            Image image = starObject.GetComponent<Image>();
            image.raycastTarget = false;
            stars.Add(image);
        }
    }

    private void CreateNewUnitLabel(Transform parent)
    {
        newUnitText = CreateText("NewUnit_Text", parent, new Vector2(0.12f, 0.78f), new Vector2(0.88f, 0.92f), 54f);
        newUnitText.text = "새로운 유닛";
        newUnitText.fontStyle = FontStyles.Bold;
    }

    private void CreateUnitPortrait(Transform parent)
    {
        GameObject unitIcon = new("Unit_Portrait", typeof(RectTransform), typeof(Image));
        RectTransform rect = unitIcon.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, 30f);
        rect.sizeDelta = new Vector2(480f, 480f);
        unitIconImage = unitIcon.GetComponent<Image>();
        unitIconImage.preserveAspect = true;
        unitIconImage.raycastTarget = false;
    }

    private void CreateNamePanel(Transform parent)
    {
        GameObject panel = new("Name_Panel", typeof(RectTransform), typeof(Image));
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.SetParent(parent, false);
        panelRect.anchorMin = new Vector2(0.22f, 0.1f);
        panelRect.anchorMax = new Vector2(0.78f, 0.26f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        namePanelImage = panel.GetComponent<Image>();
        namePanelImage.raycastTarget = false;

        unitNameText = CreateText("UnitName_Text", panel.transform, new Vector2(0.05f, 0.42f), new Vector2(0.95f, 0.95f), 38f);
        unitNameText.fontStyle = FontStyles.Bold;
        rarityText = CreateText("Rarity_Text", panel.transform, new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.43f), 23f);
    }

    private void CreateSkipButton()
    {
        GameObject buttonObject = new("Skip_Button", typeof(RectTransform), typeof(Image), typeof(Button));
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.SetParent(transform, false);
        rect.anchorMin = new Vector2(0.8f, 0.02f);
        rect.anchorMax = new Vector2(0.96f, 0.08f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.72f);
        skipButton = buttonObject.GetComponent<Button>();
        skipButton.targetGraphic = image;
        skipButton.onClick.AddListener(HandleSkip);

        TMP_Text label = CreateText("Label", buttonObject.transform, Vector2.zero, Vector2.one, 24f);
        label.text = "SKIP";
        label.fontStyle = FontStyles.Bold;
    }

    private void ConfigureRarity(Rarity rarity)
    {
        int activeRays;
        int activeStars;

        switch (rarity)
        {
            case Rarity.Legend:
                effectStrength = 1f;
                activeRays = 16;
                activeStars = 10;
                break;
            case Rarity.Rare:
                effectStrength = 0.78f;
                activeRays = 12;
                activeStars = 7;
                break;
            default:
                effectStrength = 0.55f;
                activeRays = 8;
                activeStars = 4;
                break;
        }

        for (int i = 0; i < rays.Count; i++)
            rays[i].gameObject.SetActive(i < activeRays);

        for (int i = 0; i < stars.Count; i++)
            stars[i].gameObject.SetActive(i < activeStars);

        for (int i = 0; i < bands.Count; i++)
        {
            Image bandImage = bands[i].GetComponent<Image>();
            float brightness = i == 1 ? 1.15f : 0.72f;
            bandImage.color = new Color(
                Mathf.Clamp01(rarityColor.r * brightness),
                Mathf.Clamp01(rarityColor.g * brightness),
                Mathf.Clamp01(rarityColor.b * brightness),
                i == 1 ? 0.95f : 0.62f);
        }
    }

    private static Color GetBackdropColor(Color color, float alpha)
    {
        return new Color(
            Mathf.Lerp(0.015f, color.r, 0.13f),
            Mathf.Lerp(0.02f, color.g, 0.13f),
            Mathf.Lerp(0.06f, color.b, 0.13f),
            alpha);
    }

    private static TMP_Text CreateText(
        string objectName,
        Transform parent,
        Vector2 anchorMin,
        Vector2 anchorMax,
        float fontSize)
    {
        GameObject textObject = new(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        TMP_Text text = textObject.GetComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = fontSize;
        text.raycastTarget = false;
        return text;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
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
