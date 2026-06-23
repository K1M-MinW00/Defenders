using UnityEngine;
using UnityEngine.UI;

public class InventoryPanelView : MonoBehaviour
{
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

    private void Awake()
    {
        consumableTabButton.onClick.AddListener(ShowConsumables);
        materialTabButton.onClick.AddListener(ShowMaterials);
        equipmentTabButton.onClick.AddListener(ShowEquipments);
    }

    private void OnEnable()
    {
        inventoryService = UserDataManager.Instance.InventoryService;
        ShowConsumables();
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
        ItemDataSO itemData = ItemDatabase.Get(itemId);

        if (itemData == null)
            return;

        CommonSlotUI slot = Instantiate(slotPrefab, contentRoot);

        slot.Setup(itemData.Icon, count, itemData.Stackable, itemData.Rarity, () => { detailPopup.Show(itemData, count); });
    }

    private void ClearSlots()
    {
        for (int i = contentRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(contentRoot.GetChild(i).gameObject);
        }
    }
}