using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Owns rendering and input for the equipment replacement overlay.
/// Equipment mutations remain in UnitEquipmentPanel/EquipmentUseCase.
/// </summary>
[DisallowMultipleComponent]
public sealed class EquipmentChangeOverlayView : MonoBehaviour, IModalDismissHandler
{
    [SerializeField] private EquipmentSlotView previewSlot;
    [SerializeField] private TMP_Text previewText;
    [SerializeField] private Transform contentRoot;
    [SerializeField] private EquipmentSlotView slotTemplate;
    [SerializeField] private Button equipButton;
    [SerializeField] private Button closeButton;

    private readonly List<EquipmentSlotView> slots = new();
    private Action equipRequested;
    private Action closeRequested;
    private ModalOverlayLifecycle lifecycle;

    public GameObject Root => gameObject;
    public bool IsVisible => Lifecycle.IsVisible;
    private ModalOverlayLifecycle Lifecycle => lifecycle ??= new ModalOverlayLifecycle(gameObject);

    private void Awake()
    {
        if (previewSlot != null && previewText != null && contentRoot != null &&
            slotTemplate != null && equipButton != null && closeButton != null)
        {
            return;
        }

        Debug.LogError("[EquipmentChangeOverlayView] One or more required UI references are missing.", this);
        enabled = false;
    }

    public void Initialize(Action onEquipRequested, Action onCloseRequested)
    {
        RemoveListeners();
        equipRequested = onEquipRequested;
        closeRequested = onCloseRequested;
        equipButton?.onClick.AddListener(HandleEquipClicked);
        closeButton?.onClick.AddListener(HandleCloseClicked);
    }

    public void Show() => Lifecycle.Show();
    public void Close(Action onClosed = null) => Lifecycle.Close(onClosed);
    public void HideImmediately() => Lifecycle.HideImmediately();

    public void Dismiss() => closeRequested?.Invoke();

    public void Render(
        EquipmentItemData selected,
        IReadOnlyList<EquipmentItemData> candidates,
        string selectedEffect,
        Sprite emptyIcon,
        Func<EquipmentItemData, EquipmentDataSO> resolveDefinition,
        Action<EquipmentItemData> onSelected,
        bool canEquip)
    {
        EquipmentDataSO selectedDefinition = resolveDefinition?.Invoke(selected);
        if (selectedDefinition == null)
        {
            previewSlot?.BindEmpty(emptyIcon, string.Empty, null);
            if (previewText != null)
                previewText.text = "현재 착용한 장비가 없습니다.";
        }
        else
        {
            previewSlot?.Bind(selected, selectedDefinition, false, null);
            if (previewText != null)
                previewText.text = selectedEffect ?? string.Empty;
        }

        int count = candidates?.Count ?? 0;
        EnsureSlotCapacity(count);
        for (int index = 0; index < slots.Count; index++)
        {
            bool visible = index < count;
            EquipmentSlotView slot = slots[index];
            slot.gameObject.SetActive(visible);
            if (!visible)
                continue;

            EquipmentItemData item = candidates[index];
            slot.Bind(
                item,
                resolveDefinition?.Invoke(item),
                selected?.UniqueId == item.UniqueId,
                () => onSelected?.Invoke(item));
        }

        if (equipButton != null)
            equipButton.interactable = canEquip;
    }

    private void EnsureSlotCapacity(int count)
    {
        if (slotTemplate == null || contentRoot == null)
            return;

        while (slots.Count < count)
            slots.Add(Instantiate(slotTemplate, contentRoot));
    }

    private void HandleEquipClicked() => equipRequested?.Invoke();
    private void HandleCloseClicked() => Dismiss();

    private void RemoveListeners()
    {
        equipButton?.onClick.RemoveListener(HandleEquipClicked);
        closeButton?.onClick.RemoveListener(HandleCloseClicked);
    }

    private void OnDestroy() => RemoveListeners();
}
