using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[DisallowMultipleComponent]
public sealed class InventoryPanelView : MonoBehaviour, IModalDismissHandler
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
    [SerializeField] private GameObject consumableInventoryPanel;
    [SerializeField] private Transform consumableContentRoot;
    [SerializeField] private GameObject materialInventoryPanel;
    [SerializeField] private Transform materialContentRoot;

    [Header("Popup")]
    [SerializeField] private ItemDetailPanelView detailPopup;
    [SerializeField] private EquipmentInventoryPanelView equipmentPanel;

    [Header("Buttons")]
    [SerializeField] private Button consumableTabButton;
    [SerializeField] private Button materialTabButton;
    [SerializeField] private Button equipmentTabButton;

    private InventoryService inventoryService;
    private UserDataManager userDataManager;
    private bool isSubscribed;
    private readonly Dictionary<string, CommonSlotUI> slotsByKey = new();
    private InventoryTab currentTab;
    private Transform currentContentRoot;
    private ModalOverlayLifecycle lifecycle;

    private ModalOverlayLifecycle Lifecycle => lifecycle ??= new ModalOverlayLifecycle(gameObject);

    public void Open()
    {
        Lifecycle.Show();
        RefreshCurrentTab();
    }

    public void Close() => Lifecycle.Close(CleanupTransientUi);
    public void Dismiss() => Close();

    private void Awake()
    {
        consumableTabButton.onClick.AddListener(ShowConsumables);
        materialTabButton.onClick.AddListener(ShowMaterials);
        equipmentTabButton.onClick.AddListener(ShowEquipments);
    }

    private void OnEnable()
    {
        if (!TryInitialize())
            return;

        Subscribe();
        ShowConsumables();
    }

    private void OnDisable() => Unsubscribe();

    public void Initialize(UserDataManager manager)
    {
        if (manager == null)
            return;

        if (userDataManager != manager)
        {
            Unsubscribe();
            userDataManager = manager;
            inventoryService = null;
        }

        equipmentPanel?.Initialize(manager);
        if (!isActiveAndEnabled || !TryInitialize())
            return;

        Subscribe();
        RefreshCurrentTab();
    }

    public void ShowConsumables()
    {
        SelectTab(InventoryTab.Consumables);
    }

    public void ShowMaterials()
    {
        SelectTab(InventoryTab.Materials);
    }

    public void ShowEquipments()
    {
        SelectTab(InventoryTab.Equipments);
    }

    private void SelectTab(InventoryTab tab)
    {
        detailPopup?.HideImmediately();
        currentTab = tab;

        if (tab == InventoryTab.Equipments)
        {
            SetInventoryBody(null);
            ClearStandardSlots();
            equipmentPanel?.Show();
            return;
        }

        equipmentPanel?.Hide();
        bool consumables = tab == InventoryTab.Consumables;
        SetInventoryBody(consumables ? consumableInventoryPanel : materialInventoryPanel);
        currentContentRoot = consumables
            ? (consumableContentRoot != null ? consumableContentRoot : contentRoot)
            : (materialContentRoot != null ? materialContentRoot : contentRoot);
        RefreshCurrentTab();
    }

    private void RefreshCurrentTab()
    {
        if (currentTab == InventoryTab.Equipments)
        {
            equipmentPanel?.Show();
            return;
        }

        if (inventoryService == null || currentContentRoot == null || slotPrefab == null)
            return;

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
                break;
        }

        HideUnusedSlots(visibleKeys);
    }

    private void SetInventoryBody(GameObject active)
    {
        if (consumableInventoryPanel != null) consumableInventoryPanel.SetActive(consumableInventoryPanel == active);
        if (materialInventoryPanel != null) materialInventoryPanel.SetActive(materialInventoryPanel == active);
    }

    private void ClearStandardSlots()
    {
        HideUnusedSlots(new HashSet<string>());
    }

    private void CleanupTransientUi()
    {
        detailPopup?.HideImmediately();
        equipmentPanel?.Hide();
        ClearStandardSlots();
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

    private void ReconcileSlot(
        string key,
        string itemId,
        int count,
        HashSet<string> visibleKeys,
        ref int siblingIndex)
    {
        ItemDataSO itemData = GameConfig.Items.Get(itemId);
        if (itemData == null || !visibleKeys.Add(key))
            return;

        if (!slotsByKey.TryGetValue(key, out CommonSlotUI slot) || slot == null)
        {
            slot = Instantiate(slotPrefab, currentContentRoot);
            slotsByKey[key] = slot;
        }

        slot.transform.SetParent(currentContentRoot, false);
        slot.gameObject.SetActive(true);
        slot.transform.SetSiblingIndex(siblingIndex++);
        slot.SetupItem(itemData, count, () => detailPopup?.Show(itemData, count));
    }

    private void HideUnusedSlots(HashSet<string> visibleKeys)
    {
        foreach (KeyValuePair<string, CommonSlotUI> pair in slotsByKey)
        {
            if (!visibleKeys.Contains(pair.Key) && pair.Value != null)
            {
                pair.Value.Clear();
                pair.Value.gameObject.SetActive(false);
            }
        }
    }

    private void OnDestroy()
    {
        Unsubscribe();

        if (consumableTabButton != null)
            consumableTabButton.onClick.RemoveListener(ShowConsumables);

        if (materialTabButton != null)
            materialTabButton.onClick.RemoveListener(ShowMaterials);

        if (equipmentTabButton != null)
            equipmentTabButton.onClick.RemoveListener(ShowEquipments);

        slotsByKey.Clear();
    }

    private bool TryInitialize()
    {
        userDataManager ??= UserDataManager.Instance;
        if (userDataManager?.UserData == null || userDataManager.InventoryService == null)
            return false;

        inventoryService = userDataManager.InventoryService;
        equipmentPanel?.Initialize(userDataManager);
        return true;
    }

    private void Subscribe()
    {
        if (isSubscribed || userDataManager == null)
            return;

        userDataManager.OnInventoryUpdated += RefreshCurrentTab;
        isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (!isSubscribed || userDataManager == null)
            return;

        userDataManager.OnInventoryUpdated -= RefreshCurrentTab;
        isSubscribed = false;
    }
}
