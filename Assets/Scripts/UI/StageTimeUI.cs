using UnityEngine;

public class StageTimeUI : MonoBehaviour
{
    [SerializeField] private StageTimeController timeController;

    public void OnClickSpeedButton()
    {
        timeController.ToggleSpeed();
    }

    public void OnTogglePauseButton()
    {
        timeController.TogglePause();
    }

    public void OnClickPauseButton()
    {
        timeController.Pause();
    }

    public void OnClickResumeButton()
    {
        timeController.Resume();
    }
}