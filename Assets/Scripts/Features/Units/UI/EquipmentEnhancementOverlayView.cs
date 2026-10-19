using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Owns equipment enhancement presentation and material-slot input.
/// Experience calculation and persistence are supplied by the coordinator.
/// </summary>
[DisallowMultipleComponent]
public sealed class EquipmentEnhancementOverlayView : MonoBehaviour, IModalDismissHandler
{
    [SerializeField] private EquipmentSlotView equipmentSlot;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text nextLevelText;
    [SerializeField] private TMP_Text effectText;
    [SerializeField] private Slider experienceSlider;
    [SerializeField] private Image experienceBackground;
    [SerializeField] private Image experienceFill;
    [SerializeField] private TMP_Text experienceText;
    [SerializeField] private Transform materialContent;
    [SerializeField] private TrainingMaterialSlot materialTemplate;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button autoSelectButton;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button closeButton;

    private readonly List<TrainingMaterialSlot> materialSlots = new();
    private Action resetRequested;
    private Action autoSelectRequested;
    private Action confirmRequested;
    private Action closeRequested;
    private ModalOverlayLifecycle lifecycle;

    public GameObject Root => gameObject;
    public bool IsVisible => Lifecycle.IsVisible;
    private ModalOverlayLifecycle Lifecycle => lifecycle ??= new ModalOverlayLifecycle(gameObject);

    private void Awake()
    {
        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }

        ConfigureSliderVisuals();
    }

    public void Initialize(Action onReset, Action onAutoSelect, Action onConfirm, Action onClose)
    {
        RemoveListeners();
        resetRequested = onReset;
        autoSelectRequested = onAutoSelect;
        confirmRequested = onConfirm;
        closeRequested = onClose;
        resetButton?.onClick.AddListener(HandleResetClicked);
        autoSelectButton?.onClick.AddListener(HandleAutoSelectClicked);
        confirmButton?.onClick.AddListener(HandleConfirmClicked);
        closeButton?.onClick.AddListener(HandleCloseClicked);
    }

    public void Show() => Lifecycle.Show();
    public void Close(Action onClosed = null) => Lifecycle.Close(onClosed);
    public void HideImmediately() => Lifecycle.HideImmediately();

    public void Dismiss() => closeRequested?.Invoke();

    public void Render(
        EquipmentItemData equipment,
        EquipmentDataSO definition,
        string currentEffect,
        string nextEffect,
        int requiredExperience,
        int previewExperience,
        bool ready,
        IReadOnlyList<InventoryStackItem> materials,
        EquipmentEnhancementSelection selection,
        IItemCatalog catalog,
        Action<string, int> addMaterial,
        Action<string> removeMaterial,
        bool busy)
    {
        equipmentSlot?.Bind(equipment, definition, false, null);
        if (levelText != null) levelText.text = $"LV.{equipment.Level}";
        if (nextLevelText != null)
        {
            nextLevelText.gameObject.SetActive(ready);
            nextLevelText.text = ready
                ? LocalizationService.Get(
                    LocalizationTables.Items,
                    "equipment.enhance.next_level",
                    equipment.Level + 1)
                : string.Empty;
        }
        if (effectText != null)
            effectText.text = ready ? $"{currentEffect}  >  {nextEffect}" : currentEffect;

        if (experienceSlider != null)
            experienceSlider.value = requiredExperience > 0
                ? Mathf.Clamp01((float)previewExperience / requiredExperience)
                : 0f;
        if (experienceFill != null)
            experienceFill.color = ready
                ? new Color32(61, 224, 105, 255)
                : new Color32(99, 156, 194, 255);
        if (experienceText != null)
            experienceText.text = $"{previewExperience:N0} / {requiredExperience:N0}";

        int materialCount = materials?.Count ?? 0;
        EnsureMaterialCapacity(materialCount);
        for (int index = 0; index < materialSlots.Count; index++)
        {
            bool visible = index < materialCount;
            TrainingMaterialSlot slot = materialSlots[index];
            slot.gameObject.SetActive(visible);
            if (!visible)
                continue;

            InventoryStackItem stack = materials[index];
            MaterialDataSO material = catalog?.Get(stack.ItemId) as MaterialDataSO;
            slot.Setup(
                material,
                stack.Count,
                () => addMaterial?.Invoke(stack.ItemId, stack.Count),
                () => removeMaterial?.Invoke(stack.ItemId));
            slot.Refresh(selection?.GetQuantity(stack.ItemId) ?? 0, !ready && !busy);
        }

        if (confirmButton != null) confirmButton.interactable = ready && !busy;
        if (autoSelectButton != null) autoSelectButton.interactable = !busy && requiredExperience > 0;
        if (resetButton != null) resetButton.interactable = !busy && selection != null && selection.Count > 0;
    }

    private void ConfigureSliderVisuals()
    {
        if (experienceSlider == null)
            return;

        experienceSlider.wholeNumbers = false;
        experienceSlider.minValue = 0f;
        experienceSlider.maxValue = 1f;
        experienceSlider.interactable = false;
        experienceSlider.transition = Selectable.Transition.None;
        experienceSlider.targetGraphic = null;

        if (experienceBackground != null)
        {
            experienceBackground.color = new Color32(24, 43, 61, 255);
            experienceBackground.raycastTarget = false;
        }
        if (experienceFill != null)
            experienceFill.raycastTarget = false;
    }

    private void EnsureMaterialCapacity(int count)
    {
        if (materialTemplate == null || materialContent == null)
            return;
        while (materialSlots.Count < count)
            materialSlots.Add(Instantiate(materialTemplate, materialContent));
    }

    private bool ValidateReferences()
    {
        bool isValid = equipmentSlot != null && levelText != null && nextLevelText != null &&
                       effectText != null && experienceSlider != null && experienceBackground != null &&
                       experienceFill != null && experienceText != null &&
                       materialContent != null && materialTemplate != null &&
                       resetButton != null && autoSelectButton != null &&
                       confirmButton != null && closeButton != null;

        if (!isValid)
            Debug.LogError("[EquipmentEnhancementOverlayView] One or more required UI references are missing.", this);

        return isValid;
    }

    private void HandleResetClicked() => resetRequested?.Invoke();
    private void HandleAutoSelectClicked() => autoSelectRequested?.Invoke();
    private void HandleConfirmClicked() => confirmRequested?.Invoke();
    private void HandleCloseClicked() => Dismiss();

    private void RemoveListeners()
    {
        resetButton?.onClick.RemoveListener(HandleResetClicked);
        autoSelectButton?.onClick.RemoveListener(HandleAutoSelectClicked);
        confirmButton?.onClick.RemoveListener(HandleConfirmClicked);
        closeButton?.onClick.RemoveListener(HandleCloseClicked);
    }

    private void OnDestroy() => RemoveListeners();
}
