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
    private string currentStatusKey;
    private string currentActionKey;
    public event System.Action ActionRequested;

    private void OnEnable() => LocalizationService.LocaleChanged += HandleLocaleChanged;

    private void Awake()
    {
        if (!HasRequiredReferences())
        {
            enabled = false;
            return;
        }

        targetProgress = 0f;
        progressSlider.value = 0f;
        SetStatusKey("startup.status.initializing");
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
        currentStatusKey = null;
        statusText.text = message ?? string.Empty;
    }

    public void SetStatusKey(string key)
    {
        currentStatusKey = key;
        statusText.text = Localize(key);
    }

    public void SetStartButtonVisible(bool visible)
    {
        SetActionButtonKey("startup.action.start", visible);
    }

    public void SetActionButton(string label, bool visible)
    {
        currentActionKey = null;
        actionButtonLabel.text = label;

        startButton.gameObject.SetActive(visible);
        startButton.interactable = visible;
    }

    public void SetActionButtonKey(string key, bool visible)
    {
        currentActionKey = key;
        actionButtonLabel.text = Localize(key);
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

    private void HandleLocaleChanged(string _)
    {
        if (!string.IsNullOrWhiteSpace(currentStatusKey))
            statusText.text = Localize(currentStatusKey);
        if (!string.IsNullOrWhiteSpace(currentActionKey))
            actionButtonLabel.text = Localize(currentActionKey);
    }

    private void OnDisable() => LocalizationService.LocaleChanged -= HandleLocaleChanged;

    private static string Localize(string key) =>
        LocalizationService.Get(LocalizationTables.Common, key);

    private void OnDestroy()
    {
        LocalizationService.LocaleChanged -= HandleLocaleChanged;
        if (startButton != null)
            startButton.onClick.RemoveListener(HandleActionRequested);
    }
}
