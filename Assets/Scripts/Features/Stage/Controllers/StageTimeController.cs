using System;
using UnityEngine;

public class StageTimeController : MonoBehaviour
{
    private const string SavedSpeedKey = "Stage_CombatSpeed";

    [Header("Speed")]
    [SerializeField] private float normalSpeed = 1f;
    [SerializeField] private float fastSpeed = 1.5f;

    private float selectedCombatSpeed = 1f;
    private bool isCombatPhase;
    private bool isPaused;

    public float SelectedCombatSpeed => selectedCombatSpeed;
    public bool IsCombatPhase => isCombatPhase;
    public bool IsPaused => isPaused;

    public event Action<float> OnSpeedChanged;
    public event Action<bool> OnPauseChanged;

    public void Initialize()
    {
        selectedCombatSpeed = PlayerPrefs.GetFloat(SavedSpeedKey,normalSpeed);

        if (!Mathf.Approximately(selectedCombatSpeed, fastSpeed))
            selectedCombatSpeed = normalSpeed;

        Time.timeScale = 1f;

        OnSpeedChanged?.Invoke(selectedCombatSpeed);
        OnPauseChanged?.Invoke(false);
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }

    public void ToggleSpeed()
    {
        selectedCombatSpeed = Mathf.Approximately(selectedCombatSpeed, normalSpeed) ? fastSpeed : normalSpeed;

        PlayerPrefs.SetFloat(SavedSpeedKey, selectedCombatSpeed);
        PlayerPrefs.Save();

        // 전투 중일 때만 실제 배속 적용
        if (isCombatPhase && !isPaused)
            Time.timeScale = selectedCombatSpeed;

        OnSpeedChanged?.Invoke(selectedCombatSpeed);
    }

    public void EnterCombatPhase()
    {
        isCombatPhase = true;

        if (!isPaused)
            Time.timeScale = selectedCombatSpeed;
    }

    public void ExitCombatPhase()
    {
        isCombatPhase = false;

        if (!isPaused)
            Time.timeScale = normalSpeed;
    }

    public void Pause()
    {
        if (isPaused)
            return;

        isPaused = true;
        Time.timeScale = 0f;

        OnPauseChanged?.Invoke(true);
    }

    public void Resume()
    {
        if (!isPaused)
            return;

        isPaused = false;

        Time.timeScale = isCombatPhase ? selectedCombatSpeed : normalSpeed;

        OnPauseChanged?.Invoke(false);
    }

    public void TogglePause()
    {
        if (isPaused)
            Resume();
        else
            Pause();
    }
}