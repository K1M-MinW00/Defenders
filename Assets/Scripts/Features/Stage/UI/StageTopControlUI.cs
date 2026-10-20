using UnityEngine;
using UnityEngine.UI;

public class StageTopControlUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button speedButton;

    [Header("Speed Visual")]
    [SerializeField] private Image speedIcon;
    [SerializeField] private Sprite normalSpeedSprite;
    [SerializeField] private Sprite fastSpeedSprite;

    private StageTimeController timeController;
    private bool allowPause;

    public void Initialize(StageTimeController timeController, bool allowPause = true)
    {
        Dispose();

        if (timeController == null)
        {
            Debug.LogError($"[{nameof(StageTopControlUI)}] StageTimeController is missing.", this);
            return;
        }

        this.timeController = timeController;
        this.allowPause = allowPause;

        if (pauseButton != null)
            pauseButton.interactable = allowPause;
        if (speedButton != null)
            speedButton.interactable = allowPause;

        pauseButton?.onClick.AddListener(HandlePauseClicked);
        speedButton?.onClick.AddListener(HandleSpeedClicked);

        timeController.OnSpeedChanged += HandleSpeedChanged;
        HandleSpeedChanged(timeController.SelectedCombatSpeed);
    }
    
    public void Dispose()
    {
        if (pauseButton != null)
            pauseButton.onClick.RemoveListener(HandlePauseClicked);

        if (speedButton != null)
            speedButton.onClick.RemoveListener(HandleSpeedClicked);

        if (timeController != null)
            timeController.OnSpeedChanged -= HandleSpeedChanged;

        timeController = null;
    }

    private void HandlePauseClicked()
    {
        if (!allowPause)
            return;

        timeController?.Pause();
    }

    private void HandleSpeedClicked()
    {
        if (!allowPause)
            return;

        timeController?.ToggleSpeed();
    }

    private void HandleSpeedChanged(float speed)
    {
        if (speedIcon == null || timeController == null)
            return;

        bool isFast = Mathf.Approximately(speed, timeController.FastSpeed);
        speedIcon.sprite = isFast ? fastSpeedSprite : normalSpeedSprite;
    }
}
