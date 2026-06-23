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

    private LobbyUnitViewModel currentVm;
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

    private void Awake()
    {
        trainButton.onClick.AddListener(OnClickTrain);
        addOneLevelButton.onClick.AddListener(OnClickAddOneLevel);
        maxLevelButton.onClick.AddListener(OnClickMaxLevel);
    }

    private void OnEnable()
    {
        if (currentVm == null)
            return;

        ResetSelection();
    }

    private void OnDisable()
    {
        ResetSelection();
    }

    public void Bind(LobbyUnitViewModel vm, UnitDataSO unitData, UnitDetailView panel)
    {
        currentVm = vm;
        currentUnitData = unitData;
        currentUnit = UserDataManager.Instance.UserData.Roster.GetOwnedUnit(vm.UnitId);

        resource = UserDataManager.Instance.UserData.Resource;

        materials = UserDataManager.Instance.InventoryService.GetMaterials().OrderBy(x =>
        {
            MaterialDataSO data = ItemDatabase.Get(x.ItemId) as MaterialDataSO;

            return data?.Value ?? int.MaxValue;
        }).ToList();

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
        foreach (Transform child in contentRoot)
            Destroy(child.gameObject);

        slots.Clear();

        foreach (var item in materials)
        {
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
       foreach (var item in materials)
        {
            if (item.ItemId == itemId)
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
        if (currentVm == null)
            return;

        previewTotalExp = 0;
        previewTotalGold = 0;

        foreach (var pair in selectedMaterials)
        {
            MaterialDataSO material = ItemDatabase.Get(pair.Key) as MaterialDataSO;

            if (material == null)
                continue;

            previewTotalExp += material.Value * pair.Value;
            previewTotalGold += material.Value * pair.Value;
        }

        previewLevel = currentUnit.Level;
        previewExp = currentUnit.Exp + previewTotalExp;

        while (previewLevel < currentUnitData.maxLevel)
        {
            int needExp = UnitExpTable.GetRequiredExp(previewLevel);

            if (previewExp < needExp)
                break;

            previewExp -= needExp;
            previewLevel++;
        }

        if (previewLevel >= currentUnitData.maxLevel)
        {
            previewLevel = currentUnitData.maxLevel;
            previewExp = 0;
        }

        gainedExpText.gameObject.SetActive(previewTotalExp > 0);
        gainedExpText.text = $"+{previewTotalExp}";

        int levelDiff = previewLevel - currentUnit.Level;

        levelUpDiffText.gameObject.SetActive(levelDiff > 0);
        levelUpDiffText.text = $"+{levelDiff}";

        bool canAfford = resource.Gold >= previewTotalGold;
        string color = canAfford ? "white" : "red";

        goldCostText.text = $"<sprite=1> <color={color}>{previewTotalGold:N0}</color> / {resource.Gold:N0}";

        int needCurrentExp = previewLevel >= currentUnitData.maxLevel ? 1 : UnitExpTable.GetRequiredExp(previewLevel);

        expSlider.maxValue = needCurrentExp;
        expSlider.value = previewExp;

        currentExpText.text = $"{previewExp}/{needCurrentExp}";
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

        foreach (var item in materials)
        {
            MaterialDataSO material = ItemDatabase.Get(item.ItemId) as MaterialDataSO;

            if (material == null)
                continue;

            int selectedCount = GetSelectedCount(item.ItemId);
            int remainCount = item.Count - selectedCount;

            if (remainCount <= 0)
                continue;

            int needCount = Mathf.CeilToInt((float)targetExp / material.Value);
            int addCount = Mathf.Min(remainCount, needCount);

            if (addCount <= 0)
                continue;

            selectedMaterials[item.ItemId] = selectedCount + addCount;

            targetExp -= addCount * material.Value;

            addedAny = true;

            if (targetExp <= 0)
                break;
        }

        return addedAny;
    }

    private async void OnClickTrain()
    {
        if (selectedMaterials.Count == 0)
            return;

        bool success = UserDataManager.Instance.ResourceService.SpendGold(previewTotalGold);

        if (!success)
            return;

        foreach (var pair in selectedMaterials)
            UserDataManager.Instance.InventoryService.RemoveStackItem(ItemCategory.Material, pair.Key, pair.Value);

        currentUnit.AddExp(previewTotalExp);

        await UserDataManager.Instance.SaveAsync();

        detailPanel.Refresh();
        ResetSelection();
    }
}