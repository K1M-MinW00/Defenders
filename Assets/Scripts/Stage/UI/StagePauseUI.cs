using UnityEngine;
using UnityEngine.UI;

public class StagePauseUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject panelRoot;

    [Header("Background")]
    [SerializeField] private Button blockerButton;

    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button exitButton;

    [Header("Toggles")]
    [SerializeField] private Toggle soundToggle;
    [SerializeField] private Toggle vibrationToggle;
    [SerializeField] private Toggle pushToggle;

    private StageTimeController timeController;
    private StageSessionController session;

    private bool suppressToggleEvent;

    public void Initialize(
        StageTimeController timeController,
        StageSessionController session)
    {
        this.timeController = timeController;
        this.session = session;

        panelRoot.SetActive(false);

        blockerButton.onClick.AddListener(HandleResumeClicked);
        resumeButton.onClick.AddListener(HandleResumeClicked);
        exitButton.onClick.AddListener(HandleExitClicked);

        soundToggle.onValueChanged.AddListener(HandleSoundChanged);
        vibrationToggle.onValueChanged.AddListener(HandleVibrationChanged);
        pushToggle.onValueChanged.AddListener(HandlePushChanged);

        timeController.OnPauseChanged += HandlePauseChanged;

        RefreshToggleStates();
    }

    public void Dispose()
    {
        if (blockerButton != null)
            blockerButton.onClick.RemoveListener(HandleResumeClicked);

        if (resumeButton != null)
            resumeButton.onClick.RemoveListener(HandleResumeClicked);

        if (exitButton != null)
            exitButton.onClick.RemoveListener(HandleExitClicked);

        if (soundToggle != null)
            soundToggle.onValueChanged.RemoveListener(HandleSoundChanged);

        if (vibrationToggle != null)
            vibrationToggle.onValueChanged.RemoveListener(HandleVibrationChanged);

        if (pushToggle != null)
            pushToggle.onValueChanged.RemoveListener(HandlePushChanged);

        if (timeController != null)
            timeController.OnPauseChanged -= HandlePauseChanged;
    }

    private void HandlePauseChanged(bool isPaused)
    {
        panelRoot.SetActive(isPaused);

        if (isPaused)
            RefreshToggleStates();
    }

    private void RefreshToggleStates()
    {
        suppressToggleEvent = true;

        soundToggle.isOn = GameSettingsManager.Instance.SoundEnabled;
        vibrationToggle.isOn = GameSettingsManager.Instance.VibrationEnabled;
        pushToggle.isOn = GameSettingsManager.Instance.PushEnabled;

        suppressToggleEvent = false;
    }

    private void HandleResumeClicked()
    {
        timeController?.Resume();
    }

    private void HandleExitClicked()
    {
        timeController?.Resume();
        session.RequestStageFail();
    }

    private void HandleSoundChanged(bool isOn)
    {
        if (suppressToggleEvent)
            return;

        GameSettingsManager.Instance.SetSound(isOn);
    }

    private void HandleVibrationChanged(bool isOn)
    {
        if (suppressToggleEvent)
            return;

        GameSettingsManager.Instance.SetVibration(isOn);
    }

    private void HandlePushChanged(bool isOn)
    {
        if (suppressToggleEvent)
            return;

        GameSettingsManager.Instance.SetPush(isOn);
    }
}