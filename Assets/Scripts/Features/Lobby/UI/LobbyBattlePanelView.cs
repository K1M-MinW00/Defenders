using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class LobbyBattlePanelView : MonoBehaviour
{
    [Header("Profile")]
    [SerializeField] private Image profileIconImage;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text powerText;
    [SerializeField] private Slider expSlider;

    [Header("Progress")]
    [SerializeField] private TMP_Text sectorStageText;
    [SerializeField] private TMP_Text bestWaveText;

    [Header("Resources")]
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text gemText;
    [SerializeField] private TMP_Text fuelText;

    [Header("Battle")]
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField, Min(1)] private int stageFuelCost = 10;
    [SerializeField] private Button startButton;

    private LobbyBattlePresenter presenter;
    private UserDataManager userDataManager;
    private SceneFlowService sceneFlowService;
    private bool isSubscribed;
    private bool isStartingBattle;
    private int operationVersion;

    private void Awake()
    {
        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }

        startButton.onClick.AddListener(HandleStartButtonClicked);
    }

    private void OnEnable()
    {
        if (!TryInitialize())
            return;

        SubscribeEvents();
        LocalizationService.LocaleChanged += HandleLocaleChanged;
        Refresh();
    }

    public void Initialize(UserDataManager manager, SceneFlowService sceneFlow = null)
    {
        if (manager != null && userDataManager != manager)
        {
            UnsubscribeEvents();
            userDataManager = manager;
            presenter = null;
        }

        if (sceneFlow != null)
            sceneFlowService = sceneFlow;

        if (isActiveAndEnabled && TryInitialize())
        {
            SubscribeEvents();
            Refresh();
        }
    }

    private void OnDisable()
    {
        operationVersion++;
        LocalizationService.LocaleChanged -= HandleLocaleChanged;
        UnsubscribeEvents();
    }

    private void SubscribeEvents()
    {
        if (isSubscribed || userDataManager == null)
            return;

        userDataManager.OnProfileUpdated += Refresh;
        userDataManager.OnProgressUpdated += Refresh;
        userDataManager.OnResourceUpdated += Refresh;
        userDataManager.OnRosterUpdated += Refresh;
        isSubscribed = true;
    }

    private void UnsubscribeEvents()
    {
        if (!isSubscribed || userDataManager == null)
            return;

        userDataManager.OnProfileUpdated -= Refresh;
        userDataManager.OnProgressUpdated -= Refresh;
        userDataManager.OnResourceUpdated -= Refresh;
        userDataManager.OnRosterUpdated -= Refresh;
        isSubscribed = false;
    }

    public void Refresh()
    {
        LobbyBattleViewState state = presenter?.Build();
        if (state == null)
        {
            startButton.interactable = false;
            return;
        }

        profileIconImage.sprite = state.ProfileIcon;
        profileIconImage.enabled = state.ProfileIcon != null;
        levelText.text = state.Level.ToString();
        powerText.text = state.Power.ToString("N0");
        expSlider.value = state.NormalizedExp;
        sectorStageText.text = LocalizationService.Get(
            LocalizationTables.Lobby,
            "battle.stage",
            state.Sector,
            state.Stage);
        bestWaveText.text = state.BestWaveCleared.ToString();
        goldText.text = state.Gold.ToString("N0");
        gemText.text = state.Gem.ToString("N0");
        fuelText.text = LocalizationService.Get(
            LocalizationTables.Lobby,
            "battle.fuel",
            state.Fuel,
            state.MaxFuel);
        startButton.interactable = !isStartingBattle && state.CanStartBattle && state.Fuel >= stageFuelCost;
    }

    private async void HandleStartButtonClicked()
    {
        if (isStartingBattle || !TryInitialize())
            return;

        StageEnterData enterData = null;
        LobbyBattleStartFailure failure = LobbyBattleStartFailure.UserDataUnavailable;

        if (presenter == null || !presenter.TryBuildStageEnterData(out enterData, out failure))
        {
            UIFeedbackToast.Show(GetStartFailureMessage(failure));
            return;
        }

        isStartingBattle = true;
        int currentOperation = ++operationVersion;
        startButton.interactable = false;
        bool fuelConsumed = false;

        try
        {
            StageEntryFuelResult fuelResult =
                await userDataManager.ConsumeStageEntryFuelAsync(stageFuelCost);
            if (!fuelResult.Succeeded)
            {
                isStartingBattle = false;
                if (CanApplyOperationResult(currentOperation))
                {
                    Refresh();
                    UIFeedbackToast.Show(GetFuelFailureMessage(fuelResult.Failure, stageFuelCost));
                }
                return;
            }

            fuelConsumed = true;
            StageEnterHolder.Set(enterData.WithEntryFuelCost(stageFuelCost));

            SceneTransitionResult result = await sceneFlowService.LoadAsync(gameSceneName);
            if (result == SceneTransitionResult.Succeeded)
                return;

            StageEnterHolder.Clear();
            bool refunded = await userDataManager.RefundStageEntryFuelAsync(stageFuelCost);
            fuelConsumed = false;
            if (this == null)
                return;

            isStartingBattle = false;

            if (!CanApplyOperationResult(currentOperation))
                return;

            Refresh();
            string message = GetSceneTransitionFailureMessage(result);
            if (!refunded)
                message += Localize("battle.scene.refund_failed_suffix");
            UIFeedbackToast.Show(message);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception, this);
            StageEnterHolder.Clear();

            bool refunded = !fuelConsumed ||
                            await userDataManager.RefundStageEntryFuelAsync(stageFuelCost);
            if (this == null)
                return;

            isStartingBattle = false;

            if (CanApplyOperationResult(currentOperation))
            {
                Refresh();
                UIFeedbackToast.Show(refunded
                    ? Localize("battle.scene.exception_refunded")
                    : Localize("battle.scene.exception_refund_failed"));
            }
        }
    }

    private bool CanApplyOperationResult(int version) =>
        this != null && isActiveAndEnabled && version == operationVersion;

    private void HandleLocaleChanged(string _) => Refresh();

    private bool TryInitialize()
    {
        if (userDataManager?.UserData == null || !userDataManager.IsLoaded || sceneFlowService == null)
        {
            startButton.interactable = false;
            return false;
        }

        presenter ??= new LobbyBattlePresenter(userDataManager.UserData, GameConfig.UserLevelProgression);
        return true;
    }

    private bool ValidateReferences()
    {
        bool isValid = profileIconImage != null && levelText != null && powerText != null &&
                       expSlider != null && sectorStageText != null && bestWaveText != null &&
                       goldText != null && gemText != null && fuelText != null && startButton != null &&
                       !string.IsNullOrWhiteSpace(gameSceneName) && stageFuelCost > 0;

        if (!isValid)
            Debug.LogError("[LobbyBattlePanelView] One or more required references or battle settings are invalid.", this);

        return isValid;
    }

    private static string GetStartFailureMessage(LobbyBattleStartFailure failure)
    {
        return failure switch
        {
            LobbyBattleStartFailure.EmptyFormation => Localize("battle.start.empty_formation"),
            LobbyBattleStartFailure.InvalidFormation => Localize("battle.start.invalid_formation"),
            LobbyBattleStartFailure.InvalidProgress => Localize("battle.start.invalid_progress"),
            _ => Localize("battle.start.unavailable"),
        };
    }

    private static string GetSceneTransitionFailureMessage(SceneTransitionResult result)
    {
        return result switch
        {
            SceneTransitionResult.InvalidScene => Localize("battle.scene.invalid"),
            SceneTransitionResult.AlreadyLoading => Localize("battle.scene.already_loading"),
            _ => Localize("battle.scene.load_failed"),
        };
    }

    private static string GetFuelFailureMessage(StageEntryFuelFailure failure, int fuelCost)
    {
        return failure switch
        {
            StageEntryFuelFailure.InsufficientFuel => LocalizationService.Get(
                LocalizationTables.Lobby,
                "battle.fuel.insufficient",
                fuelCost),
            StageEntryFuelFailure.SaveFailed => Localize("battle.fuel.save_failed"),
            _ => Localize("battle.fuel.unavailable"),
        };
    }

    private static string Localize(string key) =>
        LocalizationService.Get(LocalizationTables.Lobby, key);

    private void OnDestroy()
    {
        LocalizationService.LocaleChanged -= HandleLocaleChanged;
        UnsubscribeEvents();

        if (startButton != null)
            startButton.onClick.RemoveListener(HandleStartButtonClicked);
    }
}
