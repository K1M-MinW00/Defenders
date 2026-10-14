using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class EquipmentSlotView : MonoBehaviour
{
    [SerializeField] private Image frame;
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private Image selectionOverlay;
    [SerializeField] private Image checkImage;
    [SerializeField] private Button button;
    public void Bind(EquipmentItemData instance, EquipmentDataSO definition, bool selected, Action onClick)
    {
        frame.color = RarityPalette.Get(definition.Rarity);
        icon.sprite = definition.Icon;
        icon.enabled = definition.Icon != null;
        levelText.text = $"LV.{Mathf.Clamp(instance.Level, 0, EquipmentBalanceConfigSO.MaxLevel)}";
        selectionOverlay.gameObject.SetActive(selected);
        if (checkImage != null) checkImage.gameObject.SetActive(selected);
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick?.Invoke());
    }

    public void BindEmpty(Sprite emptyIcon, string label, Action onClick)
    {
        frame.color = new Color(.72f,.75f,.78f,1f);
        icon.sprite = emptyIcon;
        icon.enabled = emptyIcon != null;
        levelText.text = label ?? string.Empty;
        selectionOverlay.gameObject.SetActive(false);
        if (checkImage != null) checkImage.gameObject.SetActive(false);
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick?.Invoke());
    }
}
