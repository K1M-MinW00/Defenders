using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyBattlePanelView : MonoBehaviour
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
    [SerializeField] private Button startButton;

    private LobbyBattlePresenter presenter;
    private bool isSubscribed;

    private void Awake()
    {
        startButton.onClick.AddListener(HandleStartButtonClicked);
    }

    private void OnEnable()
    {
        UserDataManager manager = UserDataManager.Instance;
        if (manager?.UserData == null)
            return;

        presenter ??= new LobbyBattlePresenter(manager.UserData, GameConfig.UserLevelProgression);
        SubscribeEvents();
        Refresh();
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
    }

    private void SubscribeEvents()
    {
        if (isSubscribed || UserDataManager.Instance == null)
            return;

        UserDataManager.Instance.OnProfileUpdated += Refresh;
        UserDataManager.Instance.OnProgressUpdated += Refresh;
        UserDataManager.Instance.OnResourceUpdated += Refresh;
        UserDataManager.Instance.OnRosterUpdated += Refresh;
        isSubscribed = true;
    }

    private void UnsubscribeEvents()
    {
        if (!isSubscribed || UserDataManager.Instance == null)
            return;

        UserDataManager.Instance.OnProfileUpdated -= Refresh;
        UserDataManager.Instance.OnProgressUpdated -= Refresh;
        UserDataManager.Instance.OnResourceUpdated -= Refresh;
        UserDataManager.Instance.OnRosterUpdated -= Refresh;
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
        startButton.interactable = state.CanStartBattle;
    }

    private void HandleStartButtonClicked()
    {
        StageEnterData enterData = null;
        LobbyBattleStartFailure failure = LobbyBattleStartFailure.UserDataUnavailable;

        if (presenter == null || !presenter.TryBuildStageEnterData(out enterData, out failure))
        {
            UIFeedbackToast.Show(GetStartFailureMessage(failure));
            return;
        }

        StageEnterHolder.Set(enterData);
        SceneManager.LoadScene(gameSceneName);
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

    private void OnDestroy()
    {
        if (startButton != null)
            startButton.onClick.RemoveListener(HandleStartButtonClicked);
    }
}
