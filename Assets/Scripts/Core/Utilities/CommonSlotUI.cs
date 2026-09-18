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

    public void Setup(Sprite icon, int count, bool showCount, Rarity rarity, Action onClick)
    {
        SetVisuals(icon, rarity);
        SetCount(showCount, count.ToString(), normalCountColor);
        SetClickHandler(onClick);
    }

    public void SetupRequirement(
        Sprite icon,
        int ownedCount,
        int requiredCount,
        Rarity rarity,
        Action onClick)
    {
        SetVisuals(icon, rarity);

        bool sufficient = ownedCount >= requiredCount;
        Color countColor = sufficient ? normalCountColor : insufficientCountColor;
        SetCount(true, $"{ownedCount:N0} / {requiredCount:N0}", countColor);
        SetClickHandler(onClick);
    }

    private void SetVisuals(Sprite icon, Rarity rarity)
    {
        if (iconImage != null)
            iconImage.sprite = icon;

        if (frameImage != null)
            frameImage.sprite = GameIconDatabase.GetRarityFrame(rarity);
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

