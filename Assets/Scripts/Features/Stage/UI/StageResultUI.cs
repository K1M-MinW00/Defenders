using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class StageResultUI : MonoBehaviour
{
    [SerializeField] private GameObject stageClearPanel;
    [SerializeField] private GameObject stageFailPanel;

    [Header("Clear Result")]
    [SerializeField] private TMP_Text clearStageText;
    [SerializeField] private TMP_Text clearWaveText;
    [SerializeField] private GameObject clearRewardArea;
    [SerializeField] private ShopRewardSlotView[] clearRewardSlots;
    [SerializeField] private Button clearReturnButton;
    [SerializeField] private TMP_Text clearReturnButtonText;

    [Header("Fail Result")]
    [SerializeField] private TMP_Text failStageText;
    [SerializeField] private TMP_Text failWaveText;
    [SerializeField] private GameObject failRewardArea;
    [SerializeField] private Image failFuelIcon;
    [SerializeField] private TMP_Text failFuelAmountText;
    [SerializeField] private Button failReturnButton;
    [SerializeField] private TMP_Text failReturnButtonText;

    [Header("Scene")]
    [SerializeField] private string lobbySceneName = "LobbyScene";

    private StageSessionController session;
    private SceneFlowService sceneFlowService;
    private StageResultPresentation currentPresentation;
    private StageDataSO currentStage;
    private string currentReturnButtonKey = "result.return";
    private bool isReturningToLobby;

    private void Awake()
    {
        clearReturnButton.onClick.AddListener(OnClickReturnToLobby);
        failReturnButton.onClick.AddListener(OnClickReturnToLobby);
        LocalizationService.LocaleChanged += HandleLocaleChanged;
    }

    private void OnDestroy()
    {
        clearReturnButton.onClick.RemoveListener(OnClickReturnToLobby);
        failReturnButton.onClick.RemoveListener(OnClickReturnToLobby);
        LocalizationService.LocaleChanged -= HandleLocaleChanged;
    }

    public void Initialize(StageSessionController stageSession, SceneFlowService sceneFlow)
    {
        session = stageSession;
        sceneFlowService = sceneFlow;
        isReturningToLobby = false;
        HideAll();
    }

    public void ShowSettling(bool isClear, StageDataSO stage, int clearedWaveCount)
    {
        currentStage = stage;
        currentPresentation = StageResultPresenter.Create(isClear, stage, clearedWaveCount, System.Array.Empty<RewardData>());
        HideAll();
    }

    public void ShowResult(bool isClear, StageDataSO stage, int clearedWaveCount, IReadOnlyList<RewardData> rewards)
    {
        currentStage = stage;
        currentPresentation = StageResultPresenter.Create(isClear, stage, clearedWaveCount, rewards);
        if (isClear)
            ShowClear(currentPresentation);
        else
            ShowFail(currentPresentation);
    }

    public void ShowSaveFailure()
    {
        if (currentPresentation.IsClear)
            ShowClear(currentPresentation);
        else
            ShowFail(currentPresentation);

        SetReturnButtons(true, "result.retry");
        UIFeedbackToast.Show(Localize("result.save_failed"));
    }

    public void HideAll()
    {
        stageClearPanel.SetActive(false);
        stageFailPanel.SetActive(false);
    }

    private void ShowClear(StageResultPresentation presentation)
    {
        HideAll();
        clearStageText.text = presentation.StageLabel;
        clearWaveText.text = Localize("result.waves_survived", presentation.ClearedWaveCount, presentation.TotalWaveCount);
        BindClearRewards(presentation.Rewards);
        SetReturnButtons(true, "result.return");
        stageClearPanel.SetActive(true);
    }

    private void ShowFail(StageResultPresentation presentation)
    {
        HideAll();
        failStageText.text = presentation.StageLabel;
        failWaveText.text = Localize("result.waves_survived", presentation.ClearedWaveCount, presentation.TotalWaveCount);

        RewardData fuelReward = FindReward(presentation.Rewards, RewardType.Fuel);
        failRewardArea.SetActive(fuelReward != null && fuelReward.Amount > 0);
        failFuelIcon.sprite = GameConfig.Icons?.GetResourceIcon(RewardType.Fuel);
        failFuelAmountText.text = fuelReward != null ? fuelReward.Amount.ToString("N0") : "0";
        SetReturnButtons(true, "result.return");
        stageFailPanel.SetActive(true);
    }

    private void BindClearRewards(IReadOnlyList<RewardData> rewards)
    {
        int visibleCount = Mathf.Min(rewards?.Count ?? 0, clearRewardSlots.Length);
        clearRewardArea.SetActive(visibleCount > 0);
        for (int i = 0; i < clearRewardSlots.Length; i++)
        {
            bool visible = i < visibleCount;
            clearRewardSlots[i].gameObject.SetActive(visible);
            if (visible)
                clearRewardSlots[i].Bind(rewards[i]);
        }
    }

    private void SetReturnButtons(bool interactable, string localizationKey)
    {
        currentReturnButtonKey = localizationKey;
        string label = Localize(localizationKey);
        clearReturnButton.interactable = interactable;
        failReturnButton.interactable = interactable;
        clearReturnButtonText.text = label;
        failReturnButtonText.text = label;
    }

    private static RewardData FindReward(IReadOnlyList<RewardData> rewards, RewardType type)
    {
        if (rewards == null)
            return null;
        foreach (RewardData reward in rewards)
            if (reward?.Type == type)
                return reward;
        return null;
    }

    private async void OnClickReturnToLobby()
    {
        if (isReturningToLobby)
            return;

        isReturningToLobby = true;
        SetReturnButtons(false, "result.processing");
        if (session == null || !await session.TryPrepareExitAsync())
        {
            isReturningToLobby = false;
            ShowSaveFailure();
            return;
        }

        if (sceneFlowService == null)
        {
            isReturningToLobby = false;
            SetReturnButtons(true, "result.return");
            UIFeedbackToast.Show(Localize("result.scene_service_unavailable"));
            return;
        }

        SceneTransitionResult result = await sceneFlowService.LoadAsync(lobbySceneName);
        if (result != SceneTransitionResult.Succeeded && this != null)
        {
            isReturningToLobby = false;
            SetReturnButtons(true, "result.return");
            UIFeedbackToast.Show(Localize("result.lobby_load_failed"));
        }
    }

    private static string Localize(string key) =>
        LocalizationService.Get(LocalizationTables.Stage, key);

    private static string Localize(string key, params object[] arguments) =>
        LocalizationService.Get(LocalizationTables.Stage, key, arguments);

    private void HandleLocaleChanged(string _)
    {
        if (!stageClearPanel.activeSelf && !stageFailPanel.activeSelf)
            return;

        currentPresentation = StageResultPresenter.Create(
            currentPresentation.IsClear,
            currentStage,
            currentPresentation.ClearedWaveCount,
            currentPresentation.Rewards);

        if (stageClearPanel.activeSelf)
        {
            clearStageText.text = currentPresentation.StageLabel;
            clearWaveText.text = Localize("result.waves_survived", currentPresentation.ClearedWaveCount, currentPresentation.TotalWaveCount);
        }
        else
        {
            failStageText.text = currentPresentation.StageLabel;
            failWaveText.text = Localize("result.waves_survived", currentPresentation.ClearedWaveCount, currentPresentation.TotalWaveCount);
        }

        SetReturnButtons(!isReturningToLobby, currentReturnButtonKey);
    }
}
