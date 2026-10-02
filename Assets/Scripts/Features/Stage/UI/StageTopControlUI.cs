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

    public void Initialize(StageTimeController timeController)
    {
        Dispose();

        if (timeController == null)
        {
            Debug.LogError($"[{nameof(StageTopControlUI)}] StageTimeController is missing.", this);
            return;
        }

        this.timeController = timeController;

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
        timeController?.Pause();
    }

    private void HandleSpeedClicked()
    {
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
