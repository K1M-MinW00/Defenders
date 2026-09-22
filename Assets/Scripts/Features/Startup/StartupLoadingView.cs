using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StartupLoadingView : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider progressSlider;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button startButton;

    [Header("Animation")]
    [SerializeField] private float smoothSpeed = 3f;

    private float targetProgress;
    private TMP_Text startButtonLabel;

    private void Awake()
    {
        targetProgress = 0f;

        if (progressSlider != null)
            progressSlider.value = 0f;

        if (statusText != null)
            statusText.text = "Initializing...";

        if (startButton != null)
        {
            startButtonLabel = startButton.GetComponentInChildren<TMP_Text>(true);
            startButton.gameObject.SetActive(false);
            startButton.interactable = false;
        }
    }

    private void Update()
    {
        if (progressSlider == null)
            return;

        if (Mathf.Approximately(progressSlider.value, targetProgress))
            return;

        progressSlider.value = Mathf.MoveTowards(
            progressSlider.value,
            targetProgress,
            smoothSpeed * Time.deltaTime
        );
    }

    public void SetProgress(float value)
    {
        targetProgress = Mathf.Clamp01(value);
    }

    public void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }

    public void SetStartButtonVisible(bool visible)
    {
        SetActionButton("Start", visible);
    }

    public void SetActionButton(string label, bool visible)
    {
        if (startButton == null)
            return;

        if (startButtonLabel != null)
            startButtonLabel.text = label;

        startButton.gameObject.SetActive(visible);
        startButton.interactable = visible;
    }

    public bool IsProgressCompleted()
    {
        if (progressSlider == null)
            return true;

        return Mathf.Approximately(progressSlider.value, targetProgress);
    }
}
