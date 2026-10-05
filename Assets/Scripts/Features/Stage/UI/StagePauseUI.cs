using UnityEngine;
using System.Collections.Generic;
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
    [SerializeField] private Toggle pushToggle;

    [Header("Combat Formation")]
    [Tooltip("로비에서 선택한 전투 부대 순서대로 표시할 초상화 이미지입니다.")]
    [SerializeField] private Image[] unitPortraitImages = new Image[5];

    private StageTimeController timeController;
    private StageSessionController session;
    private GameSettingsManager settingsManager;

    private bool suppressToggleEvent;

    public void Initialize(
        StageTimeController timeController,
        StageSessionController session,
        IReadOnlyList<StageUnitInitData> combatFormation)
    {
        Dispose();

        if (timeController == null || session == null || panelRoot == null)
        {
            Debug.LogError($"[{nameof(StagePauseUI)}] Required references are missing.", this);
            return;
        }

        this.timeController = timeController;
        this.session = session;

        BindCombatFormation(combatFormation);

        panelRoot.SetActive(false);

        blockerButton?.onClick.AddListener(HandleResumeClicked);
        resumeButton?.onClick.AddListener(HandleResumeClicked);
        exitButton?.onClick.AddListener(HandleExitClicked);

        soundToggle?.onValueChanged.AddListener(HandleSoundChanged);
        pushToggle?.onValueChanged.AddListener(HandlePushChanged);

        timeController.OnPauseChanged += HandlePauseChanged;

        settingsManager = GameSettingsManager.Instance;
        if (settingsManager != null)
            settingsManager.OnSoundChanged += HandleExternalSoundChanged;

        RefreshToggleStates();
    }

    private void BindCombatFormation(IReadOnlyList<StageUnitInitData> combatFormation)
    {
        if (unitPortraitImages == null)
            return;

        for (int i = 0; i < unitPortraitImages.Length; i++)
        {
            Image portrait = unitPortraitImages[i];
            if (portrait == null)
                continue;

            Sprite sprite = i < (combatFormation?.Count ?? 0)
                ? combatFormation[i]?.UnitData?.icon
                : null;

            portrait.sprite = sprite;
            portrait.color = sprite != null ? Color.white : Color.clear;
            portrait.preserveAspect = true;
            portrait.raycastTarget = false;
        }
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

    private void HandlePushChanged(bool isOn)
    {
        if (suppressToggleEvent)
            return;

        settingsManager?.SetPush(isOn);
    }
}
