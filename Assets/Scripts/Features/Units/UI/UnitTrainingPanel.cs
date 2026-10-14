using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class UnitTrainingPanel : MonoBehaviour
{
    [Header("Scroll")]
    [SerializeField] private Transform contentRoot;
    [SerializeField] private TrainingMaterialSlot slotPrefab;

    [Header("Preview")]
    [SerializeField] private TMP_Text gainedExpText;
    [SerializeField] private TMP_Text goldCostText;

    [Header("EXP")]
    [SerializeField] private Slider expSlider;
    [SerializeField] private TMP_Text currentExpText;
    [SerializeField] private TMP_Text levelUpDiffText;

    [Header("Quick Select")]
    [SerializeField] private Button addOneLevelButton;
    [SerializeField] private Button maxLevelButton;
    [SerializeField] private Button trainButton;

    private UnitDataSO currentUnitData;
    private UserUnitData currentUnit;
    private UserResourceData resource;
    private UserDataManager userDataManager;

    private IReadOnlyList<InventoryStackItem> materials;
    private readonly Dictionary<string, int> selectedMaterials = new();
    private readonly List<TrainingMaterialSlot> slots = new();

    private int previewLevel;
    private int previewExp;

    private int previewTotalExp;
    private int previewTotalGold;
    private bool isTraining;
    private int operationVersion;

    private void Awake()
    {
        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }

        trainButton.onClick.AddListener(OnClickTrain);
        addOneLevelButton.onClick.AddListener(OnClickAddOneLevel);
        maxLevelButton.onClick.AddListener(OnClickMaxLevel);
    }

    private void OnEnable()
    {
        if (currentUnitData != null && TryRefreshBoundData())
            ResetSelection();
    }

    private void OnDisable()
    {
        operationVersion++;
        selectedMaterials.Clear();
    }

    public bool Bind(UnitDataSO unitData, UserDataManager manager)
    {
        if (unitData == null || manager?.UserData == null || !manager.IsLoaded)
        {
            Debug.LogError("[UnitTrainingPanel] Loaded user data and unit definition are required.", this);
            SetControlsInteractable(false);
            return false;
        }

        currentUnitData = unitData;
        userDataManager = manager;
        if (!TryRefreshBoundData())
        {
            SetControlsInteractable(false);
            return false;
        }

        ResetSelection();
        return true;
    }

    private bool TryRefreshBoundData()
    {
        if (userDataManager?.UserData == null || currentUnitData == null ||
            userDataManager.RosterService == null || userDataManager.InventoryService == null)
        {
            return false;
        }

        currentUnit = userDataManager.RosterService.GetUnit(currentUnitData.unitId);
        resource = userDataManager.UserData.Resource;
        materials = userDataManager.InventoryService.GetMaterials(MaterialType.Training);
        return currentUnit != null && resource != null && materials != null;
    }

    private void ResetSelection()
    {
        selectedMaterials.Clear();
        BuildMaterialList();
        RefreshUI();
    }

    private void RefreshUI()
    {
        RefreshSlots();
        RefreshPreview();
    }

    private void BuildMaterialList()
    {
        if (contentRoot == null || slotPrefab == null)
            return;

        HideAllSlots();

        if (materials == null)
            return;

        int slotIndex = 0;
        foreach (InventoryStackItem item in materials)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.ItemId) || item.Count <= 0)
                continue;

            MaterialDataSO materialData = GameConfig.Items.Get(item.ItemId) as MaterialDataSO;

            if (materialData == null)
                continue;

            TrainingMaterialSlot slot = GetOrCreateSlot(slotIndex++);
            slot.Setup(
                materialData,
                item.Count,
                () => OnAddMaterial(materialData),
                () => OnRemoveMaterial(materialData));

            slot.gameObject.SetActive(true);
        }
    }

    private TrainingMaterialSlot GetOrCreateSlot(int index)
    {
        if (index < slots.Count)
            return slots[index];

        TrainingMaterialSlot slot = Instantiate(slotPrefab, contentRoot);
        slot.name = $"TrainingMaterialSlot_{slots.Count + 1}";
        slots.Add(slot);
        return slot;
    }

    private void HideAllSlots()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] != null)
                slots[i].gameObject.SetActive(false);
        }
    }

    public void OnAddMaterial(MaterialDataSO material)
    {
        if (material == null || currentUnit == null || currentUnitData == null)
            return;

        if (currentUnit.Level >= currentUnitData.maxLevel)
            return;

        selectedMaterials.TryGetValue(material.ItemId, out int count);

        int ownedCount = GetOwnedCount(material.ItemId);

        if (count >= ownedCount)
            return;

        selectedMaterials[material.ItemId] = count + 1;

        RefreshUI();
    }

    public void OnRemoveMaterial(MaterialDataSO material)
    {
        if (material == null)
            return;

        if (!selectedMaterials.ContainsKey(material.ItemId))
            return;

        selectedMaterials[material.ItemId]--;

        if (selectedMaterials[material.ItemId] <= 0)
            selectedMaterials.Remove(material.ItemId);
        
        RefreshUI();
    }

    public int GetSelectedCount(string itemId)
    {
        return selectedMaterials.TryGetValue(itemId, out int count) ? count : 0;
    }

    private int GetOwnedCount(string itemId)
    {
        if (materials == null)
            return 0;

        foreach (InventoryStackItem item in materials)
        {
            if (item != null && item.ItemId == itemId)
                return item.Count;
        }

        return 0;
    }

    private void RefreshSlots()
    {
        foreach (TrainingMaterialSlot slot in slots)
        {
            if (slot == null || string.IsNullOrWhiteSpace(slot.ItemId))
                continue;

            slot.Refresh(GetSelectedCount(slot.ItemId));
        }
    }

    private void RefreshPreview()
    {
        if (currentUnit == null || currentUnitData == null || resource == null)
            return;

        UnitTrainingPreview preview = UnitTrainingPreviewCalculator.Calculate(
            currentUnit,
            currentUnitData.maxLevel,
            selectedMaterials,
            resource.Gold);

        previewLevel = preview.Level;
        previewExp = preview.Exp;
        previewTotalExp = preview.TotalExp;
        previewTotalGold = preview.GoldCost;

        gainedExpText.gameObject.SetActive(previewTotalExp > 0);
        gainedExpText.text = $"+{previewTotalExp}";

        int levelDiff = previewLevel - currentUnit.Level;

        levelUpDiffText.gameObject.SetActive(levelDiff > 0);
        levelUpDiffText.text = $"+{levelDiff}";

        string color = preview.CanAfford ? "white" : "red";

        goldCostText.text = $"<sprite=1> <color={color}>{previewTotalGold:N0}</color> / {resource.Gold:N0}";

        int needCurrentExp = previewLevel >= currentUnitData.maxLevel ? 1 : UnitExpTable.GetRequiredExp(previewLevel);

        expSlider.maxValue = needCurrentExp;
        expSlider.value = previewExp;

        currentExpText.text = $"{previewExp}/{needCurrentExp}";

        bool canTrain = !isTraining && selectedMaterials.Count > 0 && preview.CanAfford &&
            currentUnit.Level < currentUnitData.maxLevel;
        trainButton.interactable = canTrain;
        addOneLevelButton.interactable = previewLevel < currentUnitData.maxLevel;
        maxLevelButton.interactable = previewLevel < currentUnitData.maxLevel;
    }

    private void OnClickAddOneLevel()
    {
        if (previewLevel >= currentUnitData.maxLevel)
            return;

        int needExp = UnitExpTable.GetRequiredExp(previewLevel) - previewExp;

        if (previewTotalGold + needExp > resource.Gold)
            return;

        AutoFillMaterials(needExp);
    }

    private void OnClickMaxLevel()
    {
         while (true)
         {
            RefreshPreview();

            if (previewLevel >= currentUnitData.maxLevel)
                break;

            int needExp = UnitExpTable.GetRequiredExp(previewLevel) - previewExp;

            if (previewTotalGold + needExp > resource.Gold)
                break;

            bool enoughMaterial = AutoFillMaterialsInternal(needExp);

            if (!enoughMaterial)
                break;
         }

        RefreshSlots();
        RefreshPreview();
    }

    private void AutoFillMaterials(int targetExp)
    {
        AutoFillMaterialsInternal(targetExp);

        RefreshSlots();
        RefreshPreview();
    }
    private bool AutoFillMaterialsInternal(int targetExp)
    {
        bool addedAny = false;
        int availableGold = Mathf.Max(0, resource.Gold - previewTotalGold);

        foreach (InventoryStackItem item in materials)
        {
            if (item == null)
                continue;

            MaterialDataSO material = GameConfig.Items.Get(item.ItemId) as MaterialDataSO;

            if (material == null || material.Value <= 0)
                continue;

            int selectedCount = GetSelectedCount(item.ItemId);
            int remainCount = item.Count - selectedCount;

            if (remainCount <= 0)
                continue;

            int needCount = Mathf.CeilToInt((float)targetExp / material.Value);
            int affordableCount = availableGold / material.Value;
            int addCount = Mathf.Min(remainCount, needCount, affordableCount);

            if (addCount <= 0)
                continue;

            selectedMaterials[item.ItemId] = selectedCount + addCount;

            targetExp -= addCount * material.Value;
            availableGold -= addCount * material.Value;

            addedAny = true;

            if (targetExp <= 0)
                break;
        }

        return addedAny;
    }

    private async void OnClickTrain()
    {
        if (isTraining || selectedMaterials.Count == 0 || currentUnitData == null)
            return;

        isTraining = true;
        int currentOperation = ++operationVersion;
        RefreshUI();

        try
        {
            TrainUnitResult result = await userDataManager.TrainUnitAsync(
                new TrainUnitCommand(currentUnitData.unitId, selectedMaterials));

            if (!isActiveAndEnabled || currentOperation != operationVersion)
                return;

            if (!result.Succeeded)
            {
                Debug.LogWarning($"Unit training failed: {result.Failure}");
                UIFeedbackToast.Show(UnitOperationFeedbackMessages.Get(result.Failure));
                RefreshUI();
                return;
            }

            if (TryRefreshBoundData())
                ResetSelection();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception, this);
            if (isActiveAndEnabled && currentOperation == operationVersion)
                UIFeedbackToast.Show(UnitOperationFeedbackMessages.Get(TrainUnitFailure.SaveFailed));
        }
        finally
        {
            isTraining = false;
            if (isActiveAndEnabled && currentOperation == operationVersion)
                RefreshUI();
        }
    }

    private void SetControlsInteractable(bool interactable)
    {
        if (trainButton != null)
            trainButton.interactable = interactable;
        if (addOneLevelButton != null)
            addOneLevelButton.interactable = interactable;
        if (maxLevelButton != null)
            maxLevelButton.interactable = interactable;
    }

    private bool ValidateReferences()
    {
        bool isValid = contentRoot != null && slotPrefab != null &&
                       gainedExpText != null && goldCostText != null &&
                       expSlider != null && currentExpText != null && levelUpDiffText != null &&
                       addOneLevelButton != null && maxLevelButton != null && trainButton != null;

        if (!isValid)
            Debug.LogError("[UnitTrainingPanel] One or more required UI references are missing.", this);

        return isValid;
    }

    private void OnDestroy()
    {
        if (trainButton != null)
            trainButton.onClick.RemoveListener(OnClickTrain);
        if (addOneLevelButton != null)
            addOneLevelButton.onClick.RemoveListener(OnClickAddOneLevel);
        if (maxLevelButton != null)
            maxLevelButton.onClick.RemoveListener(OnClickMaxLevel);
    }
}
