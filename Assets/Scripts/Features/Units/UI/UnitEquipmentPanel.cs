using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class UnitEquipmentPanel : MonoBehaviour, IModalDismissHandler
{
    [Serializable] public sealed class PartSlot { public EquipmentType type; public EquipmentSlotView slot; }
    [Header("Loadout")][SerializeField] private PartSlot[] partSlots;
    [SerializeField] private Transform basicStatsRoot;
    [SerializeField] private EquipmentStatRowView basicStatTemplate;
    [SerializeField] private Transform bonusStatsRoot;
    [SerializeField] private EquipmentStatRowView bonusStatTemplate;
    [SerializeField] private Sprite mainWeaponEmptyIcon;
    [SerializeField] private Sprite subWeaponEmptyIcon;
    [SerializeField] private Sprite helmetEmptyIcon;
    [SerializeField] private Sprite armorEmptyIcon;

    [Header("Change")][SerializeField] private EquipmentChangeOverlayView changeView;

    [Header("Detail")][SerializeField] private EquipmentDetailOverlayView detailOverlay;

    [Header("Enhancement")][SerializeField] private EquipmentEnhancementOverlayView enhancementView;

    private EquipmentEnhancementPresenter enhancementPresenter;
    private EquipmentChangePresenter changePresenter;
    private UnitEquipmentCoordinator coordinator;
    private UnitEquipmentDataSource dataSource;
    private EquipmentStatListRenderer basicStatRenderer;
    private EquipmentStatListRenderer bonusStatRenderer;
    private UserDataManager userDataManager;
    private string unitId;
    private EquipmentType activeType;
    private EquipmentItemData selectedEquipment;
    private bool changeReturnsToDetail;
    private bool enhancementReturnsToDetail;
    private int operationVersion;
    private UnitEquipmentCoordinator Coordinator => coordinator;
    private UnitEquipmentDataSource DataSource => dataSource;
    private EquipmentEnhancementPresenter EnhancementPresenter => enhancementPresenter;
    private EquipmentChangePresenter ChangePresenter => changePresenter;
    private EquipmentStatListRenderer BasicStatRenderer =>
        basicStatRenderer ??= new EquipmentStatListRenderer(basicStatsRoot, basicStatTemplate);
    private EquipmentStatListRenderer BonusStatRenderer =>
        bonusStatRenderer ??= new EquipmentStatListRenderer(bonusStatsRoot, bonusStatTemplate);

    private void Awake()
    {
        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }

        _=BasicStatRenderer;
        _=BonusStatRenderer;
        foreach(PartSlot part in partSlots) { EquipmentType type=part.type; part.slot.BindEmpty(GetEmptySlotIcon(type),string.Empty,()=>OpenPart(type)); }
        changeView?.Initialize(()=>_ = EquipSelectedAsync(),CloseChange);
        enhancementView?.Initialize(ResetEnhancementSelection,AutoSelect,()=>_ = EnhanceAsync(),CloseEnhancement);
        changeView?.HideImmediately();
        detailOverlay?.HideImmediately();
        enhancementView?.HideImmediately();
    }

    private void OnEnable()
    {
        LocalizationService.LocaleChanged += HandleLocaleChanged;
        if(!string.IsNullOrWhiteSpace(unitId) && userDataManager != null)Refresh();
    }

    private void OnDisable()
    {
        LocalizationService.LocaleChanged -= HandleLocaleChanged;
        operationVersion++;
    }

    public bool Bind(string targetUnitId, UserDataManager manager)
    {
        if (string.IsNullOrWhiteSpace(targetUnitId) || manager?.UserData == null || !manager.IsLoaded)
        {
            Debug.LogError("[UnitEquipmentPanel] Loaded user data and unit ID are required.", this);
            return false;
        }

        userDataManager = manager;
        coordinator ??= new UnitEquipmentCoordinator(userDataManager);
        dataSource ??= new UnitEquipmentDataSource(() => userDataManager.UserData, GameConfig.Items);
        enhancementPresenter ??= new EquipmentEnhancementPresenter(
            dataSource,
            GameConfig.Items,
            GameConfig.EquipmentBalance);
        changePresenter ??= new EquipmentChangePresenter(dataSource, GameConfig.EquipmentBalance);
        unitId = targetUnitId;
        Refresh();
        return DataSource.GetUnit(unitId) != null;
    }
    public void Dismiss()
    {
        if(enhancementView!=null&&enhancementView.IsVisible)CloseEnhancement();
        else if(changeView!=null&&changeView.IsVisible)CloseChange();
    }
    private void Refresh()
    {
        UserDataRoot data=userDataManager?.UserData; UserUnitData unit=DataSource?.GetUnit(unitId); if(data?.Inventory==null||unit==null)return;
        foreach(PartSlot part in partSlots)
        {
            EquipmentItemData item=GetEquipped(part.type); EquipmentDataSO definition=DataSource.GetDefinition(item);
            if(definition==null)part.slot.BindEmpty(GetEmptySlotIcon(part.type),string.Empty,()=>OpenPart(part.type));
            else part.slot.Bind(item,definition,false,()=>OpenPart(part.type));
        }
        BasicStatRenderer.Render(
            EquipmentBaseStatSummaryFormatter.Build(GetEquippedItems(),GameConfig.Items,GameConfig.EquipmentBalance));
        BonusStatRenderer.Render(EquipmentBonusSummaryFormatter.Build(GetEquippedItems()));
        if(changeView!=null&&changeView.IsVisible)RefreshChange(); if(detailOverlay!=null&&detailOverlay.IsVisible)RefreshDetail(); if(enhancementView!=null&&enhancementView.IsVisible)RefreshEnhancement();
    }

    private void OpenPart(EquipmentType type){activeType=type;EquipmentItemData item=GetEquipped(type);if(item==null)OpenChange(type,false);else{selectedEquipment=item;OpenDetail();}}
    private void OpenChange(EquipmentType type,bool returnToDetail)
    {
        activeType=type;
        changeReturnsToDetail=returnToDetail;
        ChangePresenter.Bind(unitId,type);
        changeView.Show();
        RefreshChange();
    }

    private void OpenChangeFromDetail()
    {
        if(detailOverlay==null)return;
        detailOverlay.Close(()=>OpenChange(activeType,true));
    }

    private void CloseChange()
    {
        bool restoreDetail=changeReturnsToDetail;
        changeReturnsToDetail=false;
        changeView.Close(() =>
        {
            ChangePresenter.Clear();
            selectedEquipment=restoreDetail?GetEquipped(activeType):null;
            Refresh();
            if(restoreDetail&&selectedEquipment!=null)RefreshDetail();
        });
    }
    private void RefreshChange()
    {
        EquipmentChangePresenter.ViewState state=ChangePresenter.BuildState(Coordinator.IsBusy);
        changeView?.Render(
            state.Selected,
            state.Candidates,
            state.SelectedEffect,
            GetEmptySlotIcon(activeType),
            DataSource.GetDefinition,
            item=>{if(ChangePresenter.Select(item))RefreshChange();},
            state.CanEquip);
    }
    private async Task EquipSelectedAsync()
    {
        EquipmentItemData selected=ChangePresenter.Selected;
        if(Coordinator.IsBusy||selected==null)return;
        int currentOperation = ++operationVersion;

        try
        {
            Task<EquipmentOperationFailure> operation=Coordinator.EquipAsync(unitId,selected.UniqueId);
            RefreshChange();
            EquipmentOperationFailure result=await operation;
            if (!CanApplyOperationResult(currentOperation))
                return;

            if(result==EquipmentOperationFailure.None)CloseChange();
            else{UIFeedbackToast.Show(Localize("equipment.equip.failed"));Refresh();}
        }
        catch (Exception exception)
        {
            Debug.LogException(exception, this);
            if (CanApplyOperationResult(currentOperation))
                UIFeedbackToast.Show(Localize("equipment.equip.failed"));
        }
    }

    private void OpenDetail(){RefreshDetail();}
    private void CloseDetail(){if(detailOverlay!=null)detailOverlay.Close(()=>selectedEquipment=null);else selectedEquipment=null;}
    private void RefreshDetail()
    {
        EquipmentDataSO definition=DataSource.GetDefinition(selectedEquipment);if(definition==null||detailOverlay==null){CloseDetail();return;}
        bool max=selectedEquipment.Level>=EquipmentBalanceConfigSO.MaxLevel;
        detailOverlay.ShowEquipped(
            selectedEquipment,
            definition,
            () => _ = UnequipAsync(),
            OpenEnhancement,
            OpenChangeFromDetail,
            max,
            !Coordinator.IsBusy,
            () => selectedEquipment=null);
    }
    private async Task UnequipAsync()
    {
        if(Coordinator.IsBusy)return;
        int currentOperation = ++operationVersion;

        try
        {
            Task<EquipmentOperationFailure> operation=Coordinator.UnequipAsync(unitId,activeType);
            RefreshDetail();
            EquipmentOperationFailure result=await operation;
            if (!CanApplyOperationResult(currentOperation))
                return;

            if(result==EquipmentOperationFailure.None)CloseDetail();
            else UIFeedbackToast.Show(Localize("equipment.unequip.failed"));
            Refresh();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception, this);
            if (CanApplyOperationResult(currentOperation))
                UIFeedbackToast.Show(Localize("equipment.unequip.failed"));
        }
    }

    private void OpenEnhancement()
    {
        if(detailOverlay==null)return;
        detailOverlay.Close(()=>
        {
            enhancementReturnsToDetail=true;
            EnhancementPresenter.Bind(selectedEquipment);
            enhancementView.Show();
            RefreshEnhancement();
        });
    }

    private void CloseEnhancement()
    {
        bool restoreDetail=enhancementReturnsToDetail;
        enhancementReturnsToDetail=false;
        enhancementView.Close(() =>
        {
            EnhancementPresenter.Clear();
            Refresh();
            if(restoreDetail&&selectedEquipment!=null)RefreshDetail();
            else selectedEquipment=null;
        });
    }
    private void RefreshEnhancement()
    {
        if(!EnhancementPresenter.TryBuildState(out EquipmentEnhancementPresenter.ViewState state)){CloseEnhancement();return;}
        enhancementView?.Render(
            state.Equipment,state.Definition,state.CurrentEffect,state.NextEffect,
            state.RequiredExperience,state.PreviewExperience,state.Ready,state.Materials,
            EnhancementPresenter.Selection,GameConfig.Items,
            AddMaterial,RemoveMaterial,Coordinator.IsBusy);
    }
    private void AddMaterial(string id,int owned){if(EnhancementPresenter.TryAddMaterial(id,owned))RefreshEnhancement();}
    private void RemoveMaterial(string id){if(EnhancementPresenter.TryRemoveMaterial(id))RefreshEnhancement();}
    private void AutoSelect(){if(EnhancementPresenter.TryAutoSelect())RefreshEnhancement();}
    private async Task EnhanceAsync()
    {
        if(Coordinator.IsBusy||!EnhancementPresenter.HasSelection)return;
        int currentOperation = ++operationVersion;

        try
        {
            Task<EquipmentOperationFailure> operation=Coordinator.EnhanceAsync(selectedEquipment.UniqueId,EnhancementPresenter.CreateSelectionSnapshot());
            RefreshEnhancement();
            EquipmentOperationFailure result=await operation;
            if (!CanApplyOperationResult(currentOperation))
                return;

            if(result==EquipmentOperationFailure.None)
            {
                selectedEquipment=DataSource.FindEquipment(selectedEquipment.UniqueId);
                EnhancementPresenter.Bind(selectedEquipment);
                if(selectedEquipment!=null&&selectedEquipment.Level>=EquipmentBalanceConfigSO.MaxLevel)
                {
                    enhancementReturnsToDetail=false;
                    enhancementView.Close(() =>
                    {
                        EnhancementPresenter.Clear();
                        Refresh();
                        RefreshDetail();
                    });
                    return;
                }
            }
            else UIFeedbackToast.Show(Localize("equipment.enhance.failed"));
            Refresh();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception, this);
            if (CanApplyOperationResult(currentOperation))
                UIFeedbackToast.Show(Localize("equipment.enhance.failed"));
        }
    }

    private bool CanApplyOperationResult(int version) =>
        isActiveAndEnabled && version == operationVersion;

    private EquipmentItemData GetEquipped(EquipmentType type)=>DataSource.GetEquipped(unitId,type);
    private IEnumerable<EquipmentItemData> GetEquippedItems()=>DataSource.GetEquippedItems(unitId,partSlots.Select(slot=>slot.type));
    private Sprite GetEmptySlotIcon(EquipmentType type)=>type switch{EquipmentType.MainWeapon=>mainWeaponEmptyIcon,EquipmentType.SubWeapon=>subWeaponEmptyIcon,EquipmentType.Helmet=>helmetEmptyIcon,EquipmentType.Armor=>armorEmptyIcon,_=>null};
    private void ResetEnhancementSelection(){EnhancementPresenter.ResetSelection();RefreshEnhancement();}
    private void HandleLocaleChanged(string _){if(isActiveAndEnabled)Refresh();}
    private static string Localize(string key)=>LocalizationService.Get(LocalizationTables.Items,key);
    private void OnDestroy()=>LocalizationService.LocaleChanged-=HandleLocaleChanged;

    private bool ValidateReferences()
    {
        bool hasPartSlots = partSlots != null && partSlots.Length > 0;
        if (hasPartSlots)
        {
            for (int i = 0; i < partSlots.Length; i++)
                hasPartSlots &= partSlots[i] != null && partSlots[i].slot != null;
        }

        bool isValid = hasPartSlots && basicStatsRoot != null && basicStatTemplate != null &&
                       bonusStatsRoot != null && bonusStatTemplate != null &&
                       mainWeaponEmptyIcon != null && subWeaponEmptyIcon != null &&
                       helmetEmptyIcon != null && armorEmptyIcon != null &&
                       changeView != null && detailOverlay != null && enhancementView != null;

        if (!isValid)
            Debug.LogError("[UnitEquipmentPanel] One or more required UI references are missing.", this);

        return isValid;
    }
}
