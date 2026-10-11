using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CommonSlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image frameImage;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private Button button;

    [Header("Count State")]
    [SerializeField] private Color normalCountColor = Color.white;
    [SerializeField] private Color insufficientCountColor = new(1f, 0.25f, 0.25f, 1f);

    public void Configure(Image icon, Image frame, TMP_Text count, Button clickButton = null)
    {
        iconImage = icon;
        frameImage = frame;
        countText = count;
        button = clickButton;

        if (frameImage != null)
            frameImage.type = Image.Type.Sliced;
    }

    public void SetupItem(ItemDataSO item, int count, Action onClick)
    {
        if (item == null)
        {
            Clear();
            return;
        }

        Setup(item.Icon, item.Rarity, count, item.Stackable, onClick);
    }

    public void SetupPresentation(RewardPresentation presentation, int amount, Action onClick)
    {
        SetVisuals(presentation.Icon, presentation.Rarity);
        string amountText = presentation.Countable
            ? amount > 1 ? amount.ToString("N0") : string.Empty
            : amount > 1 ? $"×{amount:N0}" : string.Empty;
        SetCount(!string.IsNullOrEmpty(amountText), amountText, normalCountColor);
        SetClickHandler(onClick);
    }

    public void Setup(
        Sprite icon,
        Rarity rarity,
        int count,
        bool countable,
        Action onClick,
        bool omitSingleCount = true)
    {
        SetVisuals(icon, rarity);
        string countValue = countable && (!omitSingleCount || count > 1)
            ? count.ToString("N0")
            : string.Empty;
        SetCount(!string.IsNullOrEmpty(countValue), countValue, normalCountColor);
        SetClickHandler(onClick);
    }

    public void SetupRequirement(
        Sprite icon,
        Rarity rarity,
        int ownedCount,
        int requiredCount,
        Action onClick)
    {
        SetVisuals(icon, rarity);

        bool sufficient = ownedCount >= requiredCount;
        Color countColor = sufficient ? normalCountColor : insufficientCountColor;
        SetCount(true, $"{ownedCount:N0} / {requiredCount:N0}", countColor);
        SetClickHandler(onClick);
    }

    public void Clear()
    {
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }

        SetCount(false, string.Empty, normalCountColor);
        SetClickHandler(null);
    }

    private void SetVisuals(Sprite icon, Rarity rarity)
    {
        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = icon != null;
        }

        if (frameImage != null)
            frameImage.color = GetRarityColor(rarity);
    }

    public Color GetRarityColor(Rarity rarity)
    {
        return RarityPalette.Get(rarity);
    }

    private void SetCount(bool visible, string text, Color color)
    {
        if (countText == null)
            return;

        countText.gameObject.SetActive(visible);

        if (!visible)
            return;

        countText.text = text;
        countText.color = color;
    }

    private void SetClickHandler(Action onClick)
    {
        if (button == null)
            return;

        button.onClick.RemoveAllListeners();

        if (onClick != null)
            button.onClick.AddListener(() => onClick());
    }
}

