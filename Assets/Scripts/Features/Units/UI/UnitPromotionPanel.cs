using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class UnitPromotionPanel : MonoBehaviour
{
    [Header("Promotion")]
    [SerializeField] private Image promotionImage;
    [SerializeField] private UnitVisualConfigSO visualConfig;
    [SerializeField] private TMP_Text promotionText;

    [Header("Upgrades")]
    [SerializeField] private GameObject[] upgradeLockObjects;

    [Header("Materials")]
    [SerializeField] private CommonSlotUI materialSlot;

    [Header("Button")]
    [SerializeField] private Button promotionButton;

    private UnitDataSO currentUnitData;
    private UserUnitData currentUnit;
    private UserDataManager userDataManager;
    private bool isPromoting;
    private int operationVersion;

    private void Awake()
    {
        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }

        promotionButton.onClick.AddListener(OnClickPromotion);
    }

    private void OnEnable()
    {
        if (currentUnitData != null && userDataManager != null)
            Refresh();
    }

    private void OnDisable()
    {
        operationVersion++;
    }

    public bool Bind(UnitDataSO unitData, UserDataManager manager)
    {
        if (unitData == null || manager?.UserData == null || !manager.IsLoaded ||
            manager.RosterService == null || manager.InventoryService == null)
        {
            Debug.LogError("[UnitPromotionPanel] Loaded user data and unit definition are required.", this);
            promotionButton.interactable = false;
            return false;
        }

        currentUnitData = unitData;
        userDataManager = manager;

        Refresh();
        return currentUnit != null;
    }

    private void Refresh()
    {
        if (currentUnitData == null || userDataManager == null)
            return;

        currentUnit = userDataManager.RosterService.GetUnit(currentUnitData.unitId);

        if (currentUnit == null)
        {
            Debug.LogWarning($"[UnitPromotionPanel] Owned unit data not found: {currentUnitData.unitId}");
            return;
        }

        RefreshPromotionInfo();
        RefreshEffects();
        RefreshMaterials();
    }

    private void RefreshPromotionInfo()
    {
        int promotion = currentUnit.Promotion;

        if (promotionText != null)
            promotionText.text = $"{promotion}진급";

        Sprite promotionSprite = visualConfig?.GetPromotionSprite(promotion);
        if (promotionImage != null && promotionSprite != null)
            promotionImage.sprite = promotionSprite;

    }

    private void RefreshEffects()
    {
        if (upgradeLockObjects == null)
            return;

        for (int i = 0; i < upgradeLockObjects.Length; i++)
        {
            GameObject lockObject = upgradeLockObjects[i];
            if (lockObject == null)
                continue;

            int requiredPromotion = i + 1;
            lockObject.SetActive(currentUnit.Promotion < requiredPromotion);
        }
    }

    private void RefreshMaterials()
    {
        materialSlot.gameObject.SetActive(false);

        if (currentUnitData.promotionCost == null ||
            currentUnit.Promotion >= currentUnitData.promotionCost.Length)
        {
            promotionButton?.gameObject.SetActive(false);
            return;
        }

        promotionButton.gameObject.SetActive(true);

        PromotionCost cost = currentUnitData.promotionCost[currentUnit.Promotion];

        if (cost == null || string.IsNullOrWhiteSpace(cost.MaterialId) || cost.Count <= 0)
        {
            Debug.LogWarning($"[UnitPromotionPanel] Invalid promotion cost at level {currentUnit.Promotion}.");
            promotionButton.interactable = false;
            return;
        }

        MaterialDataSO material = GameConfig.Items.Get(cost.MaterialId) as MaterialDataSO;

        if (material == null)
        {
            Debug.LogWarning($"[UnitPromotionPanel] Promotion material not found: {cost.MaterialId}");
            promotionButton.interactable = false;
            return;
        }

        int owned = userDataManager.InventoryService.GetItemCount(material.ItemId);

        materialSlot.gameObject.SetActive(true);
        materialSlot.SetupRequirement(
            material.Icon,
            material.Rarity,
            owned,
            cost.Count,
            null);

        promotionButton.interactable = !isPromoting && owned >= cost.Count;
    }

    private async void OnClickPromotion()
    {
        if (isPromoting || currentUnitData == null)
            return;

        isPromoting = true;
        int currentOperation = ++operationVersion;

        if (promotionButton != null)
            promotionButton.interactable = false;

        try
        {
            PromoteUnitResult result = await userDataManager.PromoteUnitAsync(
                new PromoteUnitCommand(currentUnitData.unitId));

            if (!isActiveAndEnabled || currentOperation != operationVersion)
                return;

            if (!result.Succeeded)
            {
                Debug.LogWarning($"[UnitPromotionPanel] Promotion failed: {result.Failure}");
                UIFeedbackToast.Show(UnitOperationFeedbackMessages.Get(result.Failure));
                return;
            }

        }
        catch (Exception exception)
        {
            Debug.LogException(exception, this);
            if (isActiveAndEnabled && currentOperation == operationVersion)
                UIFeedbackToast.Show(UnitOperationFeedbackMessages.Get(PromoteUnitFailure.SaveFailed));
        }
        finally
        {
            isPromoting = false;

            if (isActiveAndEnabled && currentOperation == operationVersion)
                Refresh();
        }
    }

    private bool ValidateReferences()
    {
        bool hasUpgradeLocks = upgradeLockObjects != null && upgradeLockObjects.Length > 0;
        bool isValid = promotionImage != null && visualConfig != null && promotionText != null &&
                       hasUpgradeLocks && materialSlot != null && promotionButton != null;

        if (!isValid)
            Debug.LogError("[UnitPromotionPanel] One or more required UI references are missing.", this);

        return isValid;
    }

    private void OnDestroy()
    {
        if (promotionButton != null)
            promotionButton.onClick.RemoveListener(OnClickPromotion);
    }
}
