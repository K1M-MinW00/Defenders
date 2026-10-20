using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class LoadingScreenView : MonoBehaviour
{
    [SerializeField] private RectTransform spinner;
    [SerializeField] private TMP_Text tipText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField, Min(1f)] private float spinnerDegreesPerSecond = 150f;

    private void OnEnable()
    {
        LoadingScreenState.Changed += Refresh;
        LocalizationService.LocaleChanged += HandleLocaleChanged;
        RefreshTip();
        Refresh();
    }

    private void Start()
    {
        LoadingScreenState.MarkReady();
    }

    private void Update()
    {
        if (spinner != null)
            spinner.Rotate(0f, 0f, -spinnerDegreesPerSecond * Time.unscaledDeltaTime);
    }

    private void OnDisable()
    {
        LoadingScreenState.Changed -= Refresh;
        LocalizationService.LocaleChanged -= HandleLocaleChanged;
    }

    private void Refresh()
    {
        if (statusText != null)
        {
            statusText.text = string.IsNullOrEmpty(LoadingScreenState.ErrorMessage)
                ? LocalizationService.Get(LocalizationTables.Tutorial, "loading.status")
                : LoadingScreenState.ErrorMessage;
        }
    }

    private void RefreshTip()
    {
        if (tipText != null)
            tipText.text = LoadingTipCatalog.GetRandomTip();
    }

    private void HandleLocaleChanged(string _)
    {
        RefreshTip();
        Refresh();
    }
}
