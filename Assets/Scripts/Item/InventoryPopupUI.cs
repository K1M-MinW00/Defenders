using UnityEngine;
using UnityEngine.UI;

public class InventoryPopupUI : MonoBehaviour
{
    [SerializeField] private Transform contentRoot;

    [SerializeField] private CommonSlotUI slotPrefab;
    // [SerializeField] private InventorySlotUI slotPrefab;

    [SerializeField] private ItemDetailPopupUI detailPopup;

    [SerializeField] private Button ConsumableTabButton;
    [SerializeField] private Button MaterialTabButton;
    [SerializeField] private Button EquipmentTabButton;

    private InventoryService inventoryService;

    private void Awake()
    {
        inventoryService = UserDataManager.Instance.InventoryService;
    }

    private void OnEnable()
    {
        ShowConsumables();

        ConsumableTabButton.onClick.AddListener(ShowConsumables);
        MaterialTabButton.onClick.AddListener(ShowMaterials);
        EquipmentTabButton.onClick.AddListener(ShowEquipments);
    }

    private void OnDisable()
    {
        ConsumableTabButton.onClick.RemoveListener(ShowConsumables);
        MaterialTabButton.onClick.RemoveListener(ShowMaterials);
        EquipmentTabButton.onClick.RemoveListener(ShowEquipments);
    }

    public void ShowConsumables()
    {
        ClearSlots();

        foreach (var data in inventoryService.GetConsumables())
        {
            CreateSlot(data.ItemId, data.Count);
        }
    }

    public void ShowMaterials()
    {
        ClearSlots();

        foreach (var data in inventoryService.GetMaterials())
        {
            CreateSlot(data.ItemId, data.Count);
        }
    }

    public void ShowEquipments()
    {
        ClearSlots();

        foreach (var data in inventoryService.GetEquipments())
        {
            CreateSlot(data.ItemId, 1);
        }
    }

    private void CreateSlot(string itemId, int count)
    {
        ItemDataSO itemData = ItemDatabase.GetItem(itemId);

        if (itemData == null)
            return;

        CommonSlotUI slot = Instantiate(slotPrefab, contentRoot);

        slot.Setup(itemData.Icon, count, itemData.Stackable, itemData.Rarity, () => {detailPopup.Show(itemData, count); });

        // InventorySlotUI slot = Instantiate(slotPrefab, contentRoot);
        //slot.Setup(itemData, count, OnClickSlot);
    }

    //private void OnClickSlot(ItemDataSO data, int count)
    //{
    //    detailPopup.Show(data, count);
    //}

    private void ClearSlots()
    {
        for (int i = contentRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(contentRoot.GetChild(i).gameObject);
        }
    }
}