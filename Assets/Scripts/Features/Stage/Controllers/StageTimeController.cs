using System;
using UnityEngine;

public sealed class StageTimeController : MonoBehaviour
{
    private const string SavedSpeedKey = "Stage_CombatSpeed";
    private const float DefaultGlobalTimeScale = 1f;

    [Header("Speed")]
    [SerializeField] private float normalSpeed = 1f;
    [SerializeField] private float fastSpeed = 1.5f;

    private float selectedCombatSpeed = 1f;
    private bool isCombatPhase;
    private bool isPaused;
    private bool isInitialized;

    public float SelectedCombatSpeed => selectedCombatSpeed;
    public bool IsCombatPhase => isCombatPhase;
    public bool IsPaused => isPaused;

    public event Action<float> OnSpeedChanged;
    public event Action<bool> OnPauseChanged;

    public void Initialize()
    {
        ValidateSpeedSettings();
        selectedCombatSpeed = PlayerPrefs.GetFloat(SavedSpeedKey, normalSpeed);

        if (!Mathf.Approximately(selectedCombatSpeed, fastSpeed))
            selectedCombatSpeed = normalSpeed;

        isCombatPhase = false;
        isPaused = false;
        isInitialized = true;
        ApplyTimeScale();

        OnSpeedChanged?.Invoke(selectedCombatSpeed);
        OnPauseChanged?.Invoke(false);
    }

    private void OnDestroy()
    {
        Time.timeScale = DefaultGlobalTimeScale;
    }

    public void ToggleSpeed()
    {
        selectedCombatSpeed = Mathf.Approximately(selectedCombatSpeed, normalSpeed) ? fastSpeed : normalSpeed;

        PlayerPrefs.SetFloat(SavedSpeedKey, selectedCombatSpeed);
        PlayerPrefs.Save();

        ApplyTimeScale();

        OnSpeedChanged?.Invoke(selectedCombatSpeed);
    }

    public void EnterCombatPhase()
    {
        isCombatPhase = true;
        ApplyTimeScale();
    }

    public void ExitCombatPhase()
    {
        isCombatPhase = false;
        ApplyTimeScale();
    }

    public void Pause()
    {
        if (isPaused)
            return;

        isPaused = true;
        ApplyTimeScale();
        OnPauseChanged?.Invoke(true);
    }

    public void Resume()
    {
        if (!isPaused)
            return;

        isPaused = false;
        ApplyTimeScale();
        OnPauseChanged?.Invoke(false);
    }

    public void TogglePause()
    {
        if (isPaused)
            Resume();
        else
            Pause();
    }

    public void ResetToNormalTime()
    {
        bool wasPaused = isPaused;
        isCombatPhase = false;
        isPaused = false;
        ApplyTimeScale();

        if (wasPaused)
            OnPauseChanged?.Invoke(false);
    }

    private void ApplyTimeScale()
    {
        if (!isInitialized)
            return;

        Time.timeScale = isPaused
            ? 0f
            : isCombatPhase ? selectedCombatSpeed : normalSpeed;
    }

    private void ValidateSpeedSettings()
    {
        normalSpeed = Mathf.Max(0.01f, normalSpeed);
        fastSpeed = Mathf.Max(normalSpeed + 0.01f, fastSpeed);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        ValidateSpeedSettings();
    }
#endif
}
