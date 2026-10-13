using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class StartupLoadingView : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider progressSlider;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button startButton;
    [SerializeField] private TMP_Text actionButtonLabel;

    [Header("Animation")]
    [SerializeField] private float smoothSpeed = 3f;

    private float targetProgress;
    public event System.Action ActionRequested;

    private void Awake()
    {
        targetProgress = 0f;

        if (progressSlider != null)
            progressSlider.value = 0f;

        if (statusText != null)
            statusText.text = "Initializing...";

        if (!HasRequiredReferences())
        {
            enabled = false;
            return;
        }

        startButton.onClick.AddListener(HandleActionRequested);
        startButton.gameObject.SetActive(false);
        startButton.interactable = false;
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

        actionButtonLabel.text = label;

        startButton.gameObject.SetActive(visible);
        startButton.interactable = visible;
    }

    public bool IsProgressCompleted()
    {
        if (progressSlider == null)
            return true;

        return Mathf.Approximately(progressSlider.value, targetProgress);
    }

    private bool HasRequiredReferences()
    {
        if (progressSlider != null && statusText != null && startButton != null && actionButtonLabel != null)
            return true;

        Debug.LogError($"[{nameof(StartupLoadingView)}] Required UI references are missing.", this);
        return false;
    }

    private void HandleActionRequested()
    {
        ActionRequested?.Invoke();
    }

    private void OnDestroy()
    {
        if (startButton != null)
            startButton.onClick.RemoveListener(HandleActionRequested);
    }
}
