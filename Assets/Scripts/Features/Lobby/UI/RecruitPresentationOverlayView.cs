using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class RecruitPresentationOverlayView : MonoBehaviour
{
    [SerializeField] private List<Image> rays = new();
    [SerializeField] private List<Image> stars = new();
    [SerializeField] private List<RectTransform> bands = new();
    [SerializeField] private List<Vector2> bandPositions = new();

    [SerializeField] private Image rootBlocker;
    [SerializeField] private GameObject introContent;
    [SerializeField] private Image backdropImage;
    [SerializeField] private Image unitIconImage;
    [SerializeField] private Image namePanelImage;
    [SerializeField] private TMP_Text newUnitText;
    [SerializeField] private TMP_Text unitNameText;
    [SerializeField] private TMP_Text rarityText;
    [SerializeField] private Button skipButton;
    private Color rarityColor;
    private float effectStrength;

    public event Action SkipRequested;

    private void Awake()
    {
        skipButton.onClick.AddListener(HandleSkip);
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
            case Rarity.Epic:
                effectStrength = 0.9f;
                activeRays = 14;
                activeStars = 8;
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
