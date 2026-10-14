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
        userDataManager ??= UserDataManager.Instance;
        if (userDataManager?.UserData == null || !userDataManager.IsLoaded)
            return;

        presenter ??= new LobbyBattlePresenter(userDataManager.UserData, GameConfig.UserLevelProgression);
        SubscribeEvents();
        Refresh();
    }

    private void OnDisable()
    {
        operationVersion++;
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
        levelText.text = state.Level.ToString();
        powerText.text = state.Power.ToString("N0");
        expSlider.value = state.NormalizedExp;
        sectorStageText.text = $"{state.Sector}-{state.Stage}";
        bestWaveText.text = state.BestWaveCleared.ToString();
        goldText.text = state.Gold.ToString("N0");
        gemText.text = state.Gem.ToString("N0");
        fuelText.text = $"{state.Fuel} / {state.MaxFuel}";
        startButton.interactable = !isStartingBattle && state.CanStartBattle && state.Fuel >= stageFuelCost;
    }

    private async void HandleStartButtonClicked()
    {
        if (isStartingBattle)
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

            SceneTransitionResult result = await SceneFlowService.Shared.LoadAsync(gameSceneName);
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
                message += " 연료 반환에 실패했습니다.";
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
                    ? "전투 화면을 불러오지 못해 사용한 연료를 반환했습니다."
                    : "전투 화면을 불러오지 못했으며 연료 반환에도 실패했습니다.");
            }
        }
    }

    private bool CanApplyOperationResult(int version) =>
        this != null && isActiveAndEnabled && version == operationVersion;

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
            LobbyBattleStartFailure.EmptyFormation => "전투에 참여할 유닛을 편성해주세요.",
            LobbyBattleStartFailure.InvalidFormation => "편성 정보가 올바르지 않습니다.",
            LobbyBattleStartFailure.InvalidProgress => "스테이지 진행 정보를 확인할 수 없습니다.",
            _ => "전투를 시작할 수 없습니다.",
        };
    }

    private static string GetSceneTransitionFailureMessage(SceneTransitionResult result)
    {
        return result switch
        {
            SceneTransitionResult.InvalidScene => "전투 씬이 빌드 설정에 없습니다.",
            SceneTransitionResult.AlreadyLoading => "다른 화면으로 이동 중입니다.",
            _ => "전투 화면을 불러오지 못했습니다.",
        };
    }

    private static string GetFuelFailureMessage(StageEntryFuelFailure failure, int fuelCost)
    {
        return failure switch
        {
            StageEntryFuelFailure.InsufficientFuel => $"스테이지 입장에는 연료 {fuelCost}이 필요합니다.",
            StageEntryFuelFailure.SaveFailed => "연료 사용 정보를 저장하지 못했습니다.",
            _ => "연료를 사용할 수 없습니다.",
        };
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();

        if (startButton != null)
            startButton.onClick.RemoveListener(HandleStartButtonClicked);
    }
}
