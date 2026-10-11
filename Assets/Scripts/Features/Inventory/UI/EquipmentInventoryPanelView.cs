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
    private ModalOverlayLifecycle dismantleLifecycle;
    private ModalOverlayLifecycle filterLifecycle;
    private ModalOverlayLifecycle rewardLifecycle;

    private ModalOverlayLifecycle DismantleLifecycle =>
        dismantleLifecycle ??= new ModalOverlayLifecycle(dismantlePanel);
    private ModalOverlayLifecycle FilterLifecycle =>
        filterLifecycle ??= new ModalOverlayLifecycle(filterPanel);
    private ModalOverlayLifecycle RewardLifecycle =>
        rewardLifecycle ??= new ModalOverlayLifecycle(rewardOverlay);

    public void ConfigureMain(TMP_Text capacity, Button dismantle, Button filter, Transform content, EquipmentSlotView prefab)
    { capacityText = capacity; dismantleModeButton = dismantle; filterButton = filter; contentRoot = content; slotPrefab = prefab; }
    public void ConfigureDetail(EquipmentDetailOverlayView overlay) => detailOverlay = overlay;
    public void ConfigureDismantle(GameObject panel, Transform content, Button filter, Transform rewards, TMP_Text exp, Button confirm, Button close)
    { dismantlePanel=panel; dismantleContentRoot=content; dismantleFilterButton=filter; rewardRoot=rewards; totalExpText=exp; confirmDismantleButton=confirm; closeDismantleButton=close; }
    public void ConfigureFilter(GameObject panel, Button all, Button main, Button sub, Button helmet, Button armor, Button reset, Button close)
    { filterPanel=panel; allFilterButton=all; mainWeaponFilterButton=main; subWeaponFilterButton=sub; helmetFilterButton=helmet; armorFilterButton=armor; resetFilterButton=reset; closeFilterButton=close; }
    public void ConfigureReward(GameObject overlay, Transform root)
    { rewardOverlay=overlay; acquiredRewardRoot=root; }

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
        if (UserDataManager.Instance != null) UserDataManager.Instance.OnInventoryUpdated += Refresh;
        Refresh();
    }
    private void OnDisable()
    {
        if (UserDataManager.Instance != null) UserDataManager.Instance.OnInventoryUpdated -= Refresh;
    }

    public void Show() { gameObject.SetActive(true); Refresh(); }
    public void Hide()
    {
        detailItem = null;
        detailOverlay?.HideImmediately();
        selectedIds.Clear();
        DismantleLifecycle.HideImmediately();
        FilterLifecycle.HideImmediately();
        RewardLifecycle.HideImmediately();
        gameObject.SetActive(false);
    }
    public void Dismiss()
    {
        if (RewardLifecycle.IsVisible) CloseRewardOverlay();
        else if (FilterLifecycle.IsVisible) CloseFilter();
        else if (DismantleLifecycle.IsVisible) CloseDismantle();
    }

    private IEnumerable<EquipmentItemData> GetVisibleEquipment()
    {
        UserDataRoot data = UserDataManager.Instance?.UserData;
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
        if (!isActiveAndEnabled || UserDataManager.Instance?.UserData == null) return;
        EquipmentInventoryIndex index = new(UserDataManager.Instance.UserData.Inventory, UserDataManager.Instance.UserData.Roster);
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
        UserDataRoot data=UserDataManager.Instance.UserData;
        return new EquipmentDismantleService(GameConfig.Items,GameConfig.EquipmentBalance)
            .Dismantle(UserDataCloner.Copy(data.Inventory),UserDataCloner.Copy(data.Roster),selectedIds.ToArray());
    }
    private void RefreshRewards()
    {
        EquipmentDismantleResult preview=GetPreview(); totalExpText.text=$"합산 경험치 : {(preview.Succeeded?preview.RefundedExp:0):N0}";
        confirmDismantleButton.interactable=preview.Succeeded&&!busy;
        BindRewardSlots(rewardRoot,rewardSlots,preview.Succeeded?preview.Rewards:null);
    }

    private async Task DismantleSelectedAsync()
    {
        if(busy)return; EquipmentDismantleResult preview=GetPreview(); if(!preview.Succeeded)return;
        busy=true; RefreshRewards(); EquipmentOperationFailure result=await UserDataManager.Instance.DismantleEquipmentAsync(selectedIds.ToArray()); busy=false;
        if(result!=EquipmentOperationFailure.None){RefreshRewards();return;}
        CloseDismantle(() => ShowRewards(preview.Rewards)); Refresh();
    }
    private async Task DismantleDetailAsync()
    {
        if(busy||detailItem==null)return; selectedIds.Clear();selectedIds.Add(detailItem.UniqueId); EquipmentDismantleResult preview=GetPreview(); if(!preview.Succeeded)return;
        busy=true; EquipmentOperationFailure result=await UserDataManager.Instance.DismantleEquipmentAsync(selectedIds.ToArray()); busy=false;
        if(result!=EquipmentOperationFailure.None)return; CloseDetail(() => ShowRewards(preview.Rewards)); selectedIds.Clear(); Refresh();
    }

    private void ShowRewards(IReadOnlyDictionary<string,int> rewards) { BindRewardSlots(acquiredRewardRoot,acquiredSlots,rewards); RewardLifecycle.Show(); }
    private void CloseRewardOverlay() => RewardLifecycle.Close();
    private void BindRewardSlots(Transform root,List<CommonSlotUI> pool,IReadOnlyDictionary<string,int> rewards)
    {
        var values=rewards?.Where(x=>x.Value>0).ToList()??new List<KeyValuePair<string,int>>();
        while(pool.Count<values.Count){CommonSlotUI slot=CreateRewardSlot(root);pool.Add(slot);}
        for(int i=0;i<pool.Count;i++){bool show=i<values.Count;pool[i].gameObject.SetActive(show);if(show)pool[i].SetupItem(GameConfig.Items.Get(values[i].Key),values[i].Value,null);}
    }

    private CommonSlotUI CreateRewardSlot(Transform parent)
    {
        GameObject go=new("RewardSlot",typeof(RectTransform),typeof(Image),typeof(Button),typeof(CommonSlotUI)); go.transform.SetParent(parent,false);
        ((RectTransform)go.transform).sizeDelta=new Vector2(150,150); Image frame=go.GetComponent<Image>(); frame.color=Color.white;
        Image icon=CreateImage("Icon",go.transform,Color.white); Stretch(icon.rectTransform,12); TMP_Text count=CreateText("Count",go.transform,"",32,TextAlignmentOptions.BottomRight); Stretch(count.rectTransform,8);
        frame.sprite=slotPrefab != null ? slotPrefab.FrameSprite : null;
        frame.type=Image.Type.Sliced;
        CommonSlotUI slot=go.GetComponent<CommonSlotUI>();slot.Configure(icon,frame,count,go.GetComponent<Button>());return slot;
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
    private static Image CreateImage(string name,Transform parent,Color color){GameObject go=new(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));go.transform.SetParent(parent,false);Image image=go.GetComponent<Image>();image.color=color;return image;}
    private static TMP_Text CreateText(string name,Transform parent,string value,float size,TextAlignmentOptions alignment){GameObject go=new(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(TextMeshProUGUI));go.transform.SetParent(parent,false);TMP_Text text=go.GetComponent<TMP_Text>();text.text=value;text.fontSize=size;text.alignment=alignment;text.color=Color.white;text.fontStyle=FontStyles.Bold;return text;}
    private static void Stretch(RectTransform rect,float inset){rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=new Vector2(inset,inset);rect.offsetMax=new Vector2(-inset,-inset);}
}
