using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitTrainingPanel : MonoBehaviour
{
    [Header("Scroll")]
    [SerializeField] private Transform contentRoot;
    [SerializeField] private TrainingMaterialSlot slotPrefab;

    [Header("Preview")]
    [SerializeField] private TMP_Text previewLevelText;
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
    private UnitDetailPanel detailPanel;

    private int previewLevel;
    private int previewExp;

    private readonly Dictionary<string, int> selectedMaterials = new();
    private readonly List<TrainingMaterialSlot> slots = new();

    private int totalExp;
    private int totalGold;

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

    public void Bind(LobbyUnitViewModel vm, UnitDataSO unitData, UnitDetailPanel panel)
    {
        currentVm = vm;
        currentUnitData = unitData;
        detailPanel = panel;

        ResetSelection();
    }

    private void ResetSelection()
    {
        selectedMaterials.Clear();
        BuildMaterialList();
        RefreshPreview();
    }

    private void BuildMaterialList()
    {
        foreach (Transform child in contentRoot)
            Destroy(child.gameObject);

        slots.Clear();

        var materialList = UserDataManager.Instance.InventoryService.GetMaterials();

        foreach (var item in materialList)
        {
            MaterialDataSO materialData = ItemDatabase.GetItem(item.ItemId) as MaterialDataSO;
            if (materialData == null)
                continue;

            TrainingMaterialSlot slot = Instantiate(slotPrefab, contentRoot);
            slot.Setup(materialData, item.Count, this);

            slots.Add(slot);
        }

        RefreshSlots();
    }

    public void OnAddMaterial(MaterialDataSO material)
    {
        UserUnitData unit = UserDataManager.Instance.UserData.Roster.GetOwnedUnit(currentVm.UnitId);

        if (unit.Level >= currentUnitData.maxLevel)
            return;

        selectedMaterials.TryGetValue(material.ItemId, out int count);

        int ownedCount = GetOwnedCount(material.ItemId);

        if (count >= ownedCount)
            return;

        selectedMaterials[material.ItemId] = count + 1;

        RefreshSlots();
        RefreshPreview();
    }

    public void OnRemoveMaterial(MaterialDataSO material)
    {
        if (!selectedMaterials.ContainsKey(material.ItemId))
            return;

        selectedMaterials[material.ItemId]--;

        if (selectedMaterials[material.ItemId] <= 0)
            selectedMaterials.Remove(material.ItemId);

        RefreshSlots();
        RefreshPreview();
    }

    public int GetSelectedCount(string itemId)
    {
        return selectedMaterials.TryGetValue(itemId, out int count) ? count : 0;
    }

    private int GetOwnedCount(string itemId)
    {
        var materials = UserDataManager.Instance.InventoryService.GetMaterials();

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

        totalExp = 0;
        totalGold = 0;

        foreach (var pair in selectedMaterials)
        {
            MaterialDataSO material = ItemDatabase.GetItem(pair.Key) as MaterialDataSO;

            if (material == null)
                continue;

            totalExp += material.Value * pair.Value;
            totalGold += material.Value * pair.Value;
        }

        UserUnitData unit = UserDataManager.Instance.UserData.Roster.GetOwnedUnit(currentVm.UnitId);

        previewLevel = unit.Level;
        previewExp = unit.Exp + totalExp;

        while (previewLevel < currentUnitData.maxLevel)
        {
            int needExp = UnitExpTable.GetRequiredExp(previewLevel);

            if (previewExp < needExp)
                break;

            previewExp -= needExp;
            previewLevel++;
        }

        previewLevelText.text = $"Lv {previewLevel}";

        gainedExpText.gameObject.SetActive(totalExp > 0);
        gainedExpText.text = $"+{totalExp}";

        int levelDiff = previewLevel - unit.Level;

        levelUpDiffText.gameObject.SetActive(levelDiff > 0);
        levelUpDiffText.text = $"+{levelDiff}";

        goldCostText.text = $"<sprite=1> {totalGold.ToString("N0")}";

        int needCurrentExp = previewLevel >= currentUnitData.maxLevel ? 1 :  UnitExpTable.GetRequiredExp(previewLevel);

        expSlider.maxValue = needCurrentExp;
        expSlider.value = previewExp;

        currentExpText.text = $"{previewExp}/{needCurrentExp}";
    }

    private void OnClickAddOneLevel()
    {
        RefreshPreview();

        if (previewLevel >= currentUnitData.maxLevel)
            return;

        int needExp = UnitExpTable.GetRequiredExp(previewLevel) - previewExp;

        
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

            bool added = AutoFillMaterialsInternal(needExp);

            if (!added)
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

        var materials = UserDataManager.Instance.InventoryService.GetMaterials();

        foreach (var item in materials)
        {
            MaterialDataSO material = ItemDatabase.GetItem(item.ItemId) as MaterialDataSO;

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

        UserResourceData resource = UserDataManager.Instance.UserData.Resource;

        if (resource.Gold < totalGold)
            return;

        // TODO : Resource 처리 서비스로 빼기
        resource.Gold -= totalGold;


        foreach (var pair in selectedMaterials)
            UserDataManager.Instance.InventoryService.RemoveStackItem(ItemCategory.Material, pair.Key, pair.Value);

        UserUnitData unit = UserDataManager.Instance.UserData.Roster.GetOwnedUnit(currentVm.UnitId);

        unit.AddExp(totalExp);
        
        await UserDataManager.Instance.SaveAsync();

        detailPanel.Refresh();
        ResetSelection();
    }
}