using UnityEngine;

[DisallowMultipleComponent]
public sealed class LobbySceneCompositionRoot : MonoBehaviour
{
    [Header("Main Tabs")]
    [SerializeField] private LobbyRecruitPanelView recruitPanel;
    [SerializeField] private LobbyUnitPanelView unitPanel;
    [SerializeField] private LobbyBattlePanelView battlePanel;
    [SerializeField] private LabPanelView labPanel;
    [SerializeField] private LobbyShopPanelView shopPanel;

    [Header("Shared Lobby UI")]
    [SerializeField] private FuelPanelView fuelPanel;
    [SerializeField] private IdleRewardPanelView idleRewardPanel;
    [SerializeField] private SettingsPanelView settingsPanel;
    [SerializeField] private InventoryPanelView inventoryPanel;
    [SerializeField] private MailboxPanelView mailboxPanel;
    [SerializeField] private QuestController questController;
    [SerializeField] private WaveMilestoneRewardController waveMilestoneRewards;
    [SerializeField] private ShopMainNotificationBadgeView shopNotificationBadge;

    private void Start()
    {
        Compose();
    }

    public bool Compose()
    {
        if (!HasAllReferences())
        {
            Debug.LogError("[LobbySceneCompositionRoot] One or more lobby UI references are not assigned.", this);
            return false;
        }

        UserDataManager userDataManager = UserDataManager.Instance;
        if (userDataManager == null || !userDataManager.IsLoaded)
        {
            Debug.LogError("[LobbySceneCompositionRoot] Loaded user data is required before entering the lobby.", this);
            return false;
        }

        recruitPanel.Initialize(userDataManager);
        unitPanel.Initialize(userDataManager);
        battlePanel.Initialize(userDataManager, SceneFlowService.Shared);
        labPanel.Initialize(userDataManager);
        shopPanel.Initialize(userDataManager);

        fuelPanel.Initialize(userDataManager, AdManager.Instance);
        idleRewardPanel.Initialize(userDataManager);
        settingsPanel.Initialize(userDataManager, GameSettingsManager.Instance);
        inventoryPanel.Initialize(userDataManager);
        mailboxPanel.Initialize(userDataManager);
        questController.Initialize(userDataManager);
        waveMilestoneRewards.Initialize(userDataManager);
        shopNotificationBadge.Initialize(userDataManager);
        return true;
    }

    private void OnValidate()
    {
        if (!HasAllReferences())
            Debug.LogWarning("[LobbySceneCompositionRoot] One or more lobby UI references are not assigned.", this);
    }

    private bool HasAllReferences() =>
        recruitPanel != null && unitPanel != null && battlePanel != null && labPanel != null && shopPanel != null &&
        fuelPanel != null && idleRewardPanel != null && settingsPanel != null && inventoryPanel != null &&
        mailboxPanel != null && questController != null && waveMilestoneRewards != null && shopNotificationBadge != null;
}
