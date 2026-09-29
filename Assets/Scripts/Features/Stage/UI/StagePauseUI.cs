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
    private GameSettingsManager settingsManager;

    private bool suppressToggleEvent;

    public void Initialize(
        StageTimeController timeController,
        StageSessionController session)
    {
        Dispose();

        if (timeController == null || session == null || panelRoot == null)
        {
            Debug.LogError($"[{nameof(StagePauseUI)}] Required references are missing.", this);
            return;
        }

        this.timeController = timeController;
        this.session = session;

        panelRoot.SetActive(false);

        blockerButton?.onClick.AddListener(HandleResumeClicked);
        resumeButton?.onClick.AddListener(HandleResumeClicked);
        exitButton?.onClick.AddListener(HandleExitClicked);

        soundToggle?.onValueChanged.AddListener(HandleSoundChanged);
        vibrationToggle?.onValueChanged.AddListener(HandleVibrationChanged);
        pushToggle?.onValueChanged.AddListener(HandlePushChanged);

        timeController.OnPauseChanged += HandlePauseChanged;

        settingsManager = GameSettingsManager.Instance;
        if (settingsManager != null)
            settingsManager.OnSoundChanged += HandleExternalSoundChanged;

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

        if (settingsManager != null)
            settingsManager.OnSoundChanged -= HandleExternalSoundChanged;

        timeController = null;
        session = null;
        settingsManager = null;
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

        GameSettingsManager settings = settingsManager;
        if (settings != null)
        {
            soundToggle?.SetIsOnWithoutNotify(settings.SoundEnabled);
            vibrationToggle?.SetIsOnWithoutNotify(settings.VibrationEnabled);
            pushToggle?.SetIsOnWithoutNotify(settings.PushEnabled);
        }

        suppressToggleEvent = false;
    }

    private void HandleResumeClicked()
    {
        timeController?.Resume();
    }

    private void HandleExitClicked()
    {
        session.RequestStageFail();
    }

    private void HandleSoundChanged(bool isOn)
    {
        if (suppressToggleEvent)
            return;

        settingsManager?.SetSound(isOn);
    }

    private void HandleExternalSoundChanged(bool isOn)
    {
        soundToggle?.SetIsOnWithoutNotify(isOn);
    }

    private void HandleVibrationChanged(bool isOn)
    {
        if (suppressToggleEvent)
            return;

        settingsManager?.SetVibration(isOn);
    }

    private void HandlePushChanged(bool isOn)
    {
        if (suppressToggleEvent)
            return;

        settingsManager?.SetPush(isOn);
    }
}
