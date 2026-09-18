using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitTrainingPanel : MonoBehaviour
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

    private UnitDetailView detailPanel;

    private IReadOnlyList<InventoryStackItem> materials;
    private readonly Dictionary<string, int> selectedMaterials = new();
    private readonly List<TrainingMaterialSlot> slots = new();

    private int previewLevel;
    private int previewExp;

    private int previewTotalExp;
    private int previewTotalGold;
    private bool isTraining;

    private void Awake()
    {
        trainButton.onClick.AddListener(OnClickTrain);
        addOneLevelButton.onClick.AddListener(OnClickAddOneLevel);
        maxLevelButton.onClick.AddListener(OnClickMaxLevel);
    }

    private void OnEnable()
    {
        if (currentUnitData != null)
            ResetSelection();
    }

    private void OnDisable()
    {
        selectedMaterials.Clear();
    }

    public void Bind(UnitDataSO unitData, UnitDetailView panel)
    {
        currentUnitData = unitData;
        currentUnit = UserDataManager.Instance.RosterService.GetUnit(unitData.unitId);
        resource = UserDataManager.Instance.UserData.Resource;

        materials = UserDataManager.Instance.InventoryService.GetMaterials(MaterialType.Training);
        detailPanel = panel;

        ResetSelection();
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

        foreach (Transform child in contentRoot)
            Destroy(child.gameObject);

        slots.Clear();

        if (materials == null)
            return;

        foreach (InventoryStackItem item in materials)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.ItemId) || item.Count <= 0)
                continue;

            MaterialDataSO materialData = ItemDatabase.Get(item.ItemId) as MaterialDataSO;

            if (materialData == null)
                continue;

            TrainingMaterialSlot slot = Instantiate(slotPrefab, contentRoot);
            slot.Setup(materialData, item.Count, this);

            slots.Add(slot);
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
        foreach (var slot in slots)
            slot.Refresh();
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

            MaterialDataSO material = ItemDatabase.Get(item.ItemId) as MaterialDataSO;

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
        RefreshUI();

        try
        {
            TrainUnitResult result = await UserDataManager.Instance.UnitTrainingUseCase.ExecuteAsync(
                new TrainUnitCommand(currentUnitData.unitId, selectedMaterials));

            if (!result.Succeeded)
            {
                Debug.LogWarning($"Unit training failed: {result.Failure}");
                RefreshUI();
                return;
            }

            currentUnit = UserDataManager.Instance.RosterService.GetUnit(currentUnitData.unitId);
            resource = UserDataManager.Instance.UserData.Resource;
            materials = UserDataManager.Instance.InventoryService.GetMaterials(MaterialType.Training);

            UserDataManager.Instance.RaiseResourceUpdated();
            UserDataManager.Instance.RaiseRosterUpdated();
            detailPanel?.Refresh();
            ResetSelection();
        }
        finally
        {
            isTraining = false;
            RefreshUI();
        }
    }
}
