using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageTopControlUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button speedButton;

    [Header("Optional Text")]
    [SerializeField] private TextMeshProUGUI speedText;

    private StageTimeController timeController;

    public void Initialize(StageTimeController timeController)
    {
        this.timeController = timeController;

        pauseButton.onClick.AddListener(HandlePauseClicked);
        speedButton.onClick.AddListener(HandleSpeedClicked);

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
        if (speedText != null)
            speedText.SetText("{0:0.##}x", speed);
    }
}