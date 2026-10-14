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
        if (!HasRequiredReferences())
        {
            enabled = false;
            return;
        }

        targetProgress = 0f;
        progressSlider.value = 0f;
        statusText.text = "Initializing...";
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
            smoothSpeed * Time.unscaledDeltaTime
        );
    }

    public void SetProgress(float value)
    {
        targetProgress = Mathf.Clamp01(value);
    }

    public void SetStatus(string message)
    {
        statusText.text = message ?? string.Empty;
    }

    public void SetStartButtonVisible(bool visible)
    {
        SetActionButton("Start", visible);
    }

    public void SetActionButton(string label, bool visible)
    {
        actionButtonLabel.text = label;

        startButton.gameObject.SetActive(visible);
        startButton.interactable = visible;
    }

    public bool IsProgressCompleted()
    {
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
