using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryPanelView : MonoBehaviour
{
    private enum InventoryTab
    {
        Consumables,
        Materials,
        Equipments,
    }

    [Header("Inventory List")]
    [SerializeField] private Transform contentRoot;
    [SerializeField] private CommonSlotUI slotPrefab;

    [Header("Popup")]
    [SerializeField] private ItemDetailPanelView detailPopup;

    [Header("Buttons")]
    [SerializeField] private Button consumableTabButton;
    [SerializeField] private Button materialTabButton;
    [SerializeField] private Button equipmentTabButton;

    private InventoryService inventoryService;
    private readonly Dictionary<string, CommonSlotUI> slotsByKey = new();
    private readonly List<string> staleSlotKeys = new();
    private InventoryTab currentTab;

    private void Awake()
    {
        consumableTabButton.onClick.AddListener(ShowConsumables);
        materialTabButton.onClick.AddListener(ShowMaterials);
        equipmentTabButton.onClick.AddListener(ShowEquipments);
    }

    private void OnEnable()
    {
        inventoryService = UserDataManager.Instance.InventoryService;
        UserDataManager.Instance.OnInventoryUpdated += RefreshCurrentTab;
        ShowConsumables();
    }

    private void OnDisable()
    {
        if (UserDataManager.Instance != null)
            UserDataManager.Instance.OnInventoryUpdated -= RefreshCurrentTab;
    }

    public void ShowConsumables()
    {
        currentTab = InventoryTab.Consumables;
        RefreshCurrentTab();
    }

    public void ShowMaterials()
    {
        currentTab = InventoryTab.Materials;
        RefreshCurrentTab();
    }

    public void ShowEquipments()
    {
        currentTab = InventoryTab.Equipments;
        RefreshCurrentTab();
    }

    private void RefreshCurrentTab()
    {
        if (inventoryService == null || contentRoot == null || slotPrefab == null)
            return;

        detailPopup?.Hide();

        HashSet<string> visibleKeys = new();
        int siblingIndex = 0;

        switch (currentTab)
        {
            case InventoryTab.Consumables:
                foreach (InventoryStackItem item in inventoryService.GetConsumables())
                    ReconcileStackSlot(item, visibleKeys, ref siblingIndex);
                break;

            case InventoryTab.Materials:
                foreach (InventoryStackItem item in inventoryService.GetMaterials())
                    ReconcileStackSlot(item, visibleKeys, ref siblingIndex);
                break;

            case InventoryTab.Equipments:
                foreach (EquipmentItemData equipment in inventoryService.GetEquipments())
                    ReconcileEquipmentSlot(equipment, visibleKeys, ref siblingIndex);
                break;
        }

        RemoveStaleSlots(visibleKeys);
    }

    private void ReconcileStackSlot(
        InventoryStackItem item,
        HashSet<string> visibleKeys,
        ref int siblingIndex)
    {
        if (item == null || string.IsNullOrWhiteSpace(item.ItemId) || item.Count <= 0)
            return;

        string key = $"stack:{item.ItemId}";
        ReconcileSlot(key, item.ItemId, item.Count, visibleKeys, ref siblingIndex);
    }

    private void ReconcileEquipmentSlot(
        EquipmentItemData equipment,
        HashSet<string> visibleKeys,
        ref int siblingIndex)
    {
        if (equipment == null || string.IsNullOrWhiteSpace(equipment.ItemId))
            return;

        string identity = string.IsNullOrWhiteSpace(equipment.UniqueId)
            ? $"{equipment.ItemId}:{siblingIndex}"
            : equipment.UniqueId;
        string key = $"equipment:{identity}";

        ReconcileSlot(key, equipment.ItemId, 1, visibleKeys, ref siblingIndex);
    }

    private void ReconcileSlot(
        string key,
        string itemId,
        int count,
        HashSet<string> visibleKeys,
        ref int siblingIndex)
    {
        ItemDataSO itemData = ItemDatabase.Get(itemId);
        if (itemData == null || !visibleKeys.Add(key))
            return;

        if (!slotsByKey.TryGetValue(key, out CommonSlotUI slot) || slot == null)
        {
            slot = Instantiate(slotPrefab, contentRoot);
            slotsByKey[key] = slot;
        }

        slot.gameObject.SetActive(true);
        slot.transform.SetSiblingIndex(siblingIndex++);
        slot.Setup(
            itemData.Icon,
            GameConfig.Icons.GetRarityFrame(itemData.Rarity),
            count,
            itemData.Stackable,
            () => detailPopup?.Show(itemData, count));
    }

    private void RemoveStaleSlots(HashSet<string> visibleKeys)
    {
        staleSlotKeys.Clear();

        foreach (string key in slotsByKey.Keys)
        {
            if (!visibleKeys.Contains(key))
                staleSlotKeys.Add(key);
        }

        foreach (string key in staleSlotKeys)
        {
            CommonSlotUI slot = slotsByKey[key];
            if (slot != null)
            {
                slot.gameObject.SetActive(false);
                Destroy(slot.gameObject);
            }

            slotsByKey.Remove(key);
        }

        staleSlotKeys.Clear();
    }

    private void OnDestroy()
    {
        if (consumableTabButton != null)
            consumableTabButton.onClick.RemoveListener(ShowConsumables);

        if (materialTabButton != null)
            materialTabButton.onClick.RemoveListener(ShowMaterials);

        if (equipmentTabButton != null)
            equipmentTabButton.onClick.RemoveListener(ShowEquipments);

        slotsByKey.Clear();
    }
}
