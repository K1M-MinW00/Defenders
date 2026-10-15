using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class EquipmentInventoryPanelView : MonoBehaviour, IModalDismissHandler
{
    [Header("Main")]
    [SerializeField] private TMP_Text capacityText;
    [SerializeField] private Button dismantleModeButton;
    [SerializeField] private Button filterButton;
    [SerializeField] private Transform contentRoot;
    [SerializeField] private EquipmentSlotView slotPrefab;

    [Header("Detail")]
    [SerializeField] private EquipmentDetailOverlayView detailOverlay;

    [Header("Dismantle")]
    [SerializeField] private GameObject dismantlePanel;
    [SerializeField] private Transform dismantleContentRoot;
    [SerializeField] private Button dismantleFilterButton;
    [SerializeField] private Transform rewardRoot;
    [SerializeField] private CommonSlotUI rewardSlotPrefab;
    [SerializeField] private TMP_Text totalExpText;
    [SerializeField] private Button confirmDismantleButton;
    [SerializeField] private Button closeDismantleButton;

    [Header("Filter")]
    [SerializeField] private GameObject filterPanel;
    [SerializeField] private Button allFilterButton;
    [SerializeField] private Button mainWeaponFilterButton;
    [SerializeField] private Button subWeaponFilterButton;
    [SerializeField] private Button helmetFilterButton;
    [SerializeField] private Button armorFilterButton;
    [SerializeField] private Button resetFilterButton;
    [SerializeField] private Button closeFilterButton;

    [Header("Reward Overlay")]
    [SerializeField] private GameObject rewardOverlay;
    [SerializeField] private Transform acquiredRewardRoot;

    private readonly List<EquipmentSlotView> mainSlots = new();
    private readonly List<EquipmentSlotView> dismantleSlots = new();
    private readonly List<CommonSlotUI> rewardSlots = new();
    private readonly List<CommonSlotUI> acquiredSlots = new();
    private readonly HashSet<string> selectedIds = new(StringComparer.Ordinal);
    private readonly HashSet<EquipmentType> filters = new();
    private EquipmentItemData detailItem;
    private bool busy;
    private bool isSubscribed;
    private int dismantleOperationVersion;
    private UserDataManager userDataManager;
    private ModalOverlayLifecycle dismantleLifecycle;
    private ModalOverlayLifecycle filterLifecycle;
    private ModalOverlayLifecycle rewardLifecycle;

    private ModalOverlayLifecycle DismantleLifecycle =>
        dismantleLifecycle ??= new ModalOverlayLifecycle(dismantlePanel);
    private ModalOverlayLifecycle FilterLifecycle =>
        filterLifecycle ??= new ModalOverlayLifecycle(filterPanel);
    private ModalOverlayLifecycle RewardLifecycle =>
        rewardLifecycle ??= new ModalOverlayLifecycle(rewardOverlay);

    private void Awake()
    {
        dismantleModeButton.onClick.AddListener(OpenDismantle);
        filterButton.onClick.AddListener(OpenFilter);
        dismantleFilterButton.onClick.AddListener(OpenFilter);
        confirmDismantleButton.onClick.AddListener(() => _ = DismantleSelectedAsync());
        closeDismantleButton.onClick.AddListener(() => CloseDismantle());
        allFilterButton.onClick.AddListener(ResetFilters);
        mainWeaponFilterButton.onClick.AddListener(() => ToggleFilter(EquipmentType.MainWeapon));
        subWeaponFilterButton.onClick.AddListener(() => ToggleFilter(EquipmentType.SubWeapon));
        helmetFilterButton.onClick.AddListener(() => ToggleFilter(EquipmentType.Helmet));
        armorFilterButton.onClick.AddListener(() => ToggleFilter(EquipmentType.Armor));
        resetFilterButton.onClick.AddListener(ResetFilters);
        closeFilterButton.onClick.AddListener(CloseFilter);
        detailOverlay?.HideImmediately();
        DismantleLifecycle.HideImmediately();
        FilterLifecycle.HideImmediately();
        RewardLifecycle.HideImmediately();
    }

    private void OnEnable()
    {
        TryInitialize();
        Subscribe();
        Refresh();
    }

    private void OnDisable()
    {
        dismantleOperationVersion++;
        busy = false;
        Unsubscribe();
        HideTransientUi();
    }

    public void Initialize(UserDataManager manager)
    {
        if (manager == null)
            return;

        if (userDataManager != manager)
        {
            Unsubscribe();
            userDataManager = manager;
        }

        if (!isActiveAndEnabled || !TryInitialize())
            return;

        Subscribe();
        Refresh();
    }

    public void Show() { gameObject.SetActive(true); Refresh(); }
    public void Hide()
    {
        dismantleOperationVersion++;
        busy = false;
        HideTransientUi();
        gameObject.SetActive(false);
    }
    public void Dismiss()
    {
        if (busy)
            return;

        if (RewardLifecycle.IsVisible) CloseRewardOverlay();
        else if (FilterLifecycle.IsVisible) CloseFilter();
        else if (DismantleLifecycle.IsVisible) CloseDismantle();
    }

    private IEnumerable<EquipmentItemData> GetVisibleEquipment()
    {
        UserDataRoot data = userDataManager?.UserData;
        if (data?.Inventory == null || data.Roster == null) return Enumerable.Empty<EquipmentItemData>();
        EquipmentInventoryIndex index = new(data.Inventory, data.Roster);
        return index.GetUnequipped()
            .Where(item => GameConfig.Items.Get(item.ItemId) is EquipmentDataSO definition &&
                (filters.Count == 0 || filters.Contains(definition.EquipmentType)))
            .OrderByDescending(item => (int)((EquipmentDataSO)GameConfig.Items.Get(item.ItemId)).Rarity)
            .ThenByDescending(item => item.Level)
            .ThenBy(item => item.ItemId, StringComparer.Ordinal)
            .ToList();
    }

    private void Refresh()
    {
        if (!isActiveAndEnabled || userDataManager?.UserData == null) return;
        EquipmentInventoryIndex index = new(userDataManager.UserData.Inventory, userDataManager.UserData.Roster);
        capacityText.text = $"{index.UnequippedCount} / {EquipmentInventoryIndex.UnequippedCapacity}";
        RebuildSlots(contentRoot, mainSlots, false);
        if (DismantleLifecycle.IsVisible) { RemoveMissingSelections(); RebuildSlots(dismantleContentRoot, dismantleSlots, true); RefreshRewards(); }
        RefreshFilterVisuals();
    }

    private void RebuildSlots(Transform root, List<EquipmentSlotView> pool, bool selectable)
    {
        List<EquipmentItemData> items = GetVisibleEquipment().ToList();
        while (pool.Count < items.Count) pool.Add(Instantiate(slotPrefab, root));
        for (int i=0;i<pool.Count;i++)
        {
            bool visible=i<items.Count; pool[i].gameObject.SetActive(visible); if (!visible) continue;
            EquipmentItemData item=items[i]; EquipmentDataSO definition=GameConfig.Items.Get(item.ItemId) as EquipmentDataSO;
            pool[i].Bind(item, definition, selectedIds.Contains(item.UniqueId), selectable ? () => ToggleSelection(item.UniqueId) : () => OpenDetail(item));
        }
    }

    private void OpenDetail(EquipmentItemData item)
    {
        EquipmentDataSO definition=GameConfig.Items.Get(item.ItemId) as EquipmentDataSO; if (definition==null || detailOverlay==null) return;
        detailItem=item;
        detailOverlay.ShowInventory(
            item,
            definition,
            () => _ = DismantleDetailAsync(),
            !busy,
            () => detailItem = null);
    }
    private void CloseDetail(Action onClosed = null)
    {
        detailItem = null;
        if (detailOverlay != null)
            detailOverlay.Close(onClosed);
        else
            onClosed?.Invoke();
    }

    private void OpenDismantle() { selectedIds.Clear(); DismantleLifecycle.Show(); Refresh(); }
    private void CloseDismantle(Action onClosed = null)
    {
        selectedIds.Clear();
        DismantleLifecycle.Close(onClosed);
    }
    private void ToggleSelection(string id) { if(!selectedIds.Add(id))selectedIds.Remove(id); RebuildSlots(dismantleContentRoot,dismantleSlots,true); RefreshRewards(); }
    private void RemoveMissingSelections() { HashSet<string> available=GetVisibleEquipment().Select(x=>x.UniqueId).ToHashSet(StringComparer.Ordinal); selectedIds.RemoveWhere(id=>!available.Contains(id)); }

    private EquipmentDismantleResult GetPreview()
    {
        if(selectedIds.Count==0)return EquipmentDismantleResult.Fail(EquipmentDismantleFailure.InvalidData);
        UserDataRoot data=userDataManager.UserData;
        return new EquipmentDismantleService(GameConfig.Items,GameConfig.EquipmentBalance)
            .Dismantle(UserDataCloner.Copy(data.Inventory),UserDataCloner.Copy(data.Roster),selectedIds.ToArray());
    }
    private void RefreshRewards()
    {
        EquipmentDismantleResult preview=GetPreview(); totalExpText.text=$"합산 경험치 : {(preview.Succeeded?preview.RefundedExp:0):N0}";
        confirmDismantleButton.interactable=preview.Succeeded&&!busy;
        closeDismantleButton.interactable=!busy;
        BindRewardSlots(rewardRoot,rewardSlots,preview.Succeeded?preview.Rewards:null);
    }

    private async Task DismantleSelectedAsync()
    {
        if (busy || !TryInitialize()) return;
        EquipmentDismantleResult preview = GetPreview();
        if (!preview.Succeeded) return;

        busy = true;
        int operationVersion = ++dismantleOperationVersion;
        RefreshRewards();

        EquipmentOperationFailure result;
        try
        {
            result = await userDataManager.DismantleEquipmentAsync(selectedIds.ToArray());
        }
        catch (Exception exception)
        {
            Debug.LogError($"[EquipmentInventoryPanelView] Dismantle failed: {exception}", this);
            result = EquipmentOperationFailure.SaveFailed;
        }

        if (!IsCurrentOperation(operationVersion)) return;
        busy = false;
        if (result != EquipmentOperationFailure.None)
        {
            RefreshRewards();
            UIFeedbackToast.Show("장비 분해에 실패했습니다.");
            return;
        }

        CloseDismantle(() => ShowRewards(preview.Rewards));
        Refresh();
    }
    private async Task DismantleDetailAsync()
    {
        if (busy || detailItem == null || !TryInitialize()) return;
        selectedIds.Clear();
        selectedIds.Add(detailItem.UniqueId);
        EquipmentDismantleResult preview = GetPreview();
        if (!preview.Succeeded) return;

        busy = true;
        int operationVersion = ++dismantleOperationVersion;
        EquipmentOperationFailure result;
        try
        {
            result = await userDataManager.DismantleEquipmentAsync(selectedIds.ToArray());
        }
        catch (Exception exception)
        {
            Debug.LogError($"[EquipmentInventoryPanelView] Detail dismantle failed: {exception}", this);
            result = EquipmentOperationFailure.SaveFailed;
        }

        if (!IsCurrentOperation(operationVersion)) return;
        busy = false;
        if (result != EquipmentOperationFailure.None)
        {
            UIFeedbackToast.Show("장비 분해에 실패했습니다.");
            return;
        }

        CloseDetail(() => ShowRewards(preview.Rewards));
        selectedIds.Clear();
        Refresh();
    }

    private void ShowRewards(IReadOnlyDictionary<string,int> rewards) { BindRewardSlots(acquiredRewardRoot,acquiredSlots,rewards); RewardLifecycle.Show(); }
    private void CloseRewardOverlay() => RewardLifecycle.Close();
    private void BindRewardSlots(Transform root,List<CommonSlotUI> pool,IReadOnlyDictionary<string,int> rewards)
    {
        var values=rewards?.Where(x=>x.Value>0).ToList()??new List<KeyValuePair<string,int>>();
        if (rewardSlotPrefab == null)
            return;

        while(pool.Count<values.Count){CommonSlotUI slot=Instantiate(rewardSlotPrefab,root);pool.Add(slot);}
        for(int i=0;i<pool.Count;i++){bool show=i<values.Count;pool[i].gameObject.SetActive(show);if(show)pool[i].SetupItem(GameConfig.Items.Get(values[i].Key),values[i].Value,null);}
    }

    private void OpenFilter(){FilterLifecycle.Show();RefreshFilterVisuals();}
    private void CloseFilter()=>FilterLifecycle.Close();
    private void ResetFilters(){filters.Clear();Refresh();}
    private void ToggleFilter(EquipmentType type){if(!filters.Add(type))filters.Remove(type);Refresh();}
    private void RefreshFilterVisuals()
    {
        SetFilter(allFilterButton,filters.Count==0);SetFilter(mainWeaponFilterButton,filters.Contains(EquipmentType.MainWeapon));SetFilter(subWeaponFilterButton,filters.Contains(EquipmentType.SubWeapon));SetFilter(helmetFilterButton,filters.Contains(EquipmentType.Helmet));SetFilter(armorFilterButton,filters.Contains(EquipmentType.Armor));
    }
    private static void SetFilter(Button button,bool selected){if(button?.targetGraphic!=null)button.targetGraphic.color=selected?new Color(.2f,.65f,1f):new Color(.75f,.82f,.86f);}

    private bool TryInitialize()
    {
        userDataManager ??= UserDataManager.Instance;
        return userDataManager?.UserData != null;
    }

    private void Subscribe()
    {
        if (isSubscribed || userDataManager == null || !isActiveAndEnabled)
            return;

        userDataManager.OnInventoryUpdated += Refresh;
        isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (!isSubscribed || userDataManager == null)
            return;

        userDataManager.OnInventoryUpdated -= Refresh;
        isSubscribed = false;
    }

    private bool IsCurrentOperation(int version) =>
        this != null && isActiveAndEnabled && version == dismantleOperationVersion;

    private void HideTransientUi()
    {
        detailItem = null;
        detailOverlay?.HideImmediately();
        selectedIds.Clear();
        DismantleLifecycle.HideImmediately();
        FilterLifecycle.HideImmediately();
        RewardLifecycle.HideImmediately();
    }

    private void OnDestroy()
    {
        Unsubscribe();
        dismantleModeButton?.onClick.RemoveListener(OpenDismantle);
        filterButton?.onClick.RemoveListener(OpenFilter);
        dismantleFilterButton?.onClick.RemoveListener(OpenFilter);
        allFilterButton?.onClick.RemoveListener(ResetFilters);
        resetFilterButton?.onClick.RemoveListener(ResetFilters);
        closeFilterButton?.onClick.RemoveListener(CloseFilter);
    }
}
