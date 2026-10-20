using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(1000)]
[DisallowMultipleComponent]
public sealed class LobbyTutorialController : MonoBehaviour
{
    private enum GuideStep
    {
        None,
        UnitTab,
        OwnedUnit,
        Training,
        Promotion,
        LimitBreak,
        Equipment,
        CloseDetail,
        LaboratoryTab,
        Complete,
    }

    private const int UnitMainTabIndex = 1;
    private const int LaboratoryMainTabIndex = 3;
    private const int TrainingTabIndex = 0;
    private const int PromotionTabIndex = 1;
    private const int LimitBreakTabIndex = 2;
    private const int EquipmentTabIndex = 3;

    [SerializeField] private TutorialOverlayView overlay;
    [SerializeField] private LobbyTabView mainTabs;
    [SerializeField] private LobbyUnitPanelView unitPanel;
    [SerializeField] private LobbyLevelUpView levelUpView;

    private UserDataManager userDataManager;
    private UnitDetailView openedDetail;
    private GuideStep step;
    private bool rootEventsAttached;

    private void Start()
    {
        userDataManager = UserDataManager.Instance;
        if (userDataManager?.UserData?.Tutorial == null)
        {
            overlay?.Hide();
            enabled = false;
            return;
        }

        levelUpView ??= GetComponent<LobbyLevelUpView>();
        if (levelUpView == null)
        {
            Debug.LogError($"[{nameof(LobbyTutorialController)}] LobbyLevelUpView must be placed and wired in LobbyScene.", this);
            DisableGuide();
            return;
        }
        if (LevelUpPresentationState.TryConsume(out LevelUpPresentationData presentation))
        {
            overlay?.Hide();
            levelUpView.Closed += HandleLevelUpClosed;
            Sprite profileIcon = ProfileIconResolver.ResolveIcon(
                presentation.IconId,
                userDataManager.UserData.Roster);
            Button laboratoryButton = mainTabs.GetTabButton(LaboratoryMainTabIndex);
            Sprite laboratoryIcon = ResolveButtonIcon(laboratoryButton);
            levelUpView.Show(presentation, profileIcon, laboratoryIcon);
            return;
        }

        BeginPendingGuide();
    }

    private void HandleLevelUpClosed()
    {
        levelUpView.Closed -= HandleLevelUpClosed;
        BeginPendingGuide();
    }

    private void BeginPendingGuide()
    {
        UserTutorialData tutorial = userDataManager?.UserData?.Tutorial;
        if (tutorial == null || !tutorial.Completed)
        {
            DisableGuide();
            return;
        }

        AttachRootEvents();
        if (!tutorial.LobbyUnitGuideCompleted)
        {
            step = GuideStep.UnitTab;
            mainTabs.RestrictInteractionTo(UnitMainTabIndex);
            ShowUnitTabGuide();
            return;
        }

        int accountLevel = userDataManager.UserData.Profile?.Level ?? 0;
        if (accountLevel >= LobbyFeatureUnlockPolicy.LaboratoryRequiredLevel &&
            !tutorial.LaboratoryUnlockGuideCompleted)
        {
            step = GuideStep.LaboratoryTab;
            mainTabs.RestrictInteractionTo(LaboratoryMainTabIndex);
            overlay?.ShowRequiredAction(
                GetText("lobby.laboratory_unlocked"),
                mainTabs.GetTabTarget(LaboratoryMainTabIndex));
            return;
        }

        DisableGuide();
    }

    private void ShowUnitTabGuide()
    {
        overlay?.ShowRequiredAction(
            GetText("lobby.unit_tab"),
            mainTabs.GetTabTarget(UnitMainTabIndex));
    }

    private void HandleMainTabSelected(int index)
    {
        if (step == GuideStep.LaboratoryTab && index == LaboratoryMainTabIndex)
        {
            mainTabs.RestrictInteractionTo(null);
            overlay?.ShowExplanation(
                GetText("lobby.laboratory_explanation"));
            StartCoroutine(CompleteLaboratoryGuideAfterReading());
            return;
        }

        if (step != GuideStep.UnitTab || index != UnitMainTabIndex)
            return;

        step = GuideStep.OwnedUnit;
        Canvas.ForceUpdateCanvases();
        overlay?.ShowRequiredAction(
            GetText("lobby.owned_unit"),
            unitPanel.GetFirstOwnedUnitTarget());
    }

    private void HandleUnitDetailOpened(UnitDetailView detailView)
    {
        if (step != GuideStep.OwnedUnit || detailView?.ContentTabView == null)
            return;

        openedDetail = detailView;
        openedDetail.SetDismissAllowed(false);
        openedDetail.ContentTabView.TabSelected += HandleDetailTabSelected;
        openedDetail.Dismissed += HandleDetailDismissed;
        openedDetail.ContentTabView.ShowTab(TrainingTabIndex);
        openedDetail.ContentTabView.RestrictInteractionTo(TrainingTabIndex);

        step = GuideStep.Training;
        overlay?.ShowRequiredAction(
            GetText("lobby.training"),
            openedDetail.ContentTabView.GetTabTarget(TrainingTabIndex));
    }

    private void HandleUnitTrained()
    {
        if (step != GuideStep.Training || openedDetail == null)
            return;

        step = GuideStep.Promotion;
        openedDetail.ContentTabView.RestrictInteractionTo(PromotionTabIndex);
        overlay?.ShowRequiredAction(
            GetText("lobby.promotion"),
            openedDetail.ContentTabView.GetTabTarget(PromotionTabIndex));
    }

    private void HandleDetailTabSelected(int index)
    {
        if (openedDetail == null)
            return;

        if (step == GuideStep.Promotion && index == PromotionTabIndex)
        {
            step = GuideStep.LimitBreak;
            openedDetail.ContentTabView.RestrictInteractionTo(LimitBreakTabIndex);
            overlay?.ShowRequiredAction(
                GetText("lobby.limit_break"),
                openedDetail.ContentTabView.GetTabTarget(LimitBreakTabIndex));
            return;
        }

        if (step == GuideStep.LimitBreak && index == LimitBreakTabIndex)
        {
            step = GuideStep.Equipment;
            openedDetail.ContentTabView.RestrictInteractionTo(EquipmentTabIndex);
            overlay?.ShowRequiredAction(
                GetText("lobby.equipment"),
                openedDetail.ContentTabView.GetTabTarget(EquipmentTabIndex));
            return;
        }

        if (step == GuideStep.Equipment && index == EquipmentTabIndex)
        {
            step = GuideStep.CloseDetail;
            openedDetail.ContentTabView.RestrictInteractionTo(null);
            openedDetail.SetDismissAllowed(true);
            overlay?.ShowExplanation(
                GetText("lobby.close_detail"));
        }
    }

    private void HandleDetailDismissed()
    {
        if (step != GuideStep.CloseDetail)
            return;

        step = GuideStep.Complete;
        mainTabs.RestrictInteractionTo(null);
        overlay?.Hide();
        DetachDetailEvents();
        SaveCompletionAndDisable();
    }

    private async void SaveCompletionAndDisable()
    {
        if (userDataManager != null)
            await userDataManager.CompleteLobbyUnitGuideAsync();
        DetachRootEvents();
        enabled = false;
    }

    private IEnumerator CompleteLaboratoryGuideAfterReading()
    {
        yield return new WaitForSecondsRealtime(3f);
        overlay?.Hide();
        step = GuideStep.Complete;
        if (userDataManager != null)
            _ = userDataManager.CompleteLaboratoryUnlockGuideAsync();
        DetachRootEvents();
        enabled = false;
    }

    private void AttachRootEvents()
    {
        if (rootEventsAttached)
            return;

        mainTabs.TabSelected += HandleMainTabSelected;
        unitPanel.UnitDetailOpened += HandleUnitDetailOpened;
        userDataManager.OnUnitTrained += HandleUnitTrained;
        rootEventsAttached = true;
    }

    private void DisableGuide()
    {
        mainTabs?.RestrictInteractionTo(null);
        overlay?.Hide();
        DetachRootEvents();
        enabled = false;
    }

    private void DetachRootEvents()
    {
        if (!rootEventsAttached)
            return;

        if (mainTabs != null)
            mainTabs.TabSelected -= HandleMainTabSelected;
        if (unitPanel != null)
            unitPanel.UnitDetailOpened -= HandleUnitDetailOpened;
        if (userDataManager != null)
            userDataManager.OnUnitTrained -= HandleUnitTrained;
        rootEventsAttached = false;
    }

    private void DetachDetailEvents()
    {
        if (openedDetail == null)
            return;

        if (openedDetail.ContentTabView != null)
        {
            openedDetail.ContentTabView.RestrictInteractionTo(null);
            openedDetail.ContentTabView.TabSelected -= HandleDetailTabSelected;
        }
        openedDetail.SetDismissAllowed(true);
        openedDetail.Dismissed -= HandleDetailDismissed;
        openedDetail = null;
    }

    private static string GetText(string key) =>
        LocalizationService.Get(LocalizationTables.Tutorial, key);

    private static Sprite ResolveButtonIcon(Button button)
    {
        if (button == null)
            return null;

        Graphic targetGraphic = button.targetGraphic;
        foreach (Image image in button.GetComponentsInChildren<Image>(true))
        {
            if (image != null && image != targetGraphic && image.sprite != null)
                return image.sprite;
        }

        return (targetGraphic as Image)?.sprite;
    }

    private void OnDestroy()
    {
        if (levelUpView != null)
            levelUpView.Closed -= HandleLevelUpClosed;
        DetachRootEvents();
        DetachDetailEvents();
    }
}
