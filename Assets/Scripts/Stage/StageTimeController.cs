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

    public event Action<float> OnSelectedSpeedChanged;
    public event Action<bool> OnPauseChanged;

    private void Awake()
    {
        LoadSavedSpeed();

        // 스테이지 시작 직후는 준비 단계이므로 항상 1배속
        Time.timeScale = 1f;
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }

    public void ToggleSpeed()
    {
        selectedCombatSpeed = Mathf.Approximately(selectedCombatSpeed, normalSpeed)
            ? fastSpeed
            : normalSpeed;

        SaveSpeed();

        OnSelectedSpeedChanged?.Invoke(selectedCombatSpeed);

        // 전투 중일 때만 실제 배속 적용
        if (isCombatPhase && !isPaused)
            ApplySelectedCombatSpeed();
    }

    public void SetSpeed(float speed)
    {
        selectedCombatSpeed = Mathf.Approximately(speed, fastSpeed)
            ? fastSpeed
            : normalSpeed;

        SaveSpeed();

        OnSelectedSpeedChanged?.Invoke(selectedCombatSpeed);

        if (isCombatPhase && !isPaused)
            ApplySelectedCombatSpeed();
    }

    public void EnterCombatPhase()
    {
        isCombatPhase = true;

        if (!isPaused)
            ApplySelectedCombatSpeed();
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

        if (isCombatPhase)
            ApplySelectedCombatSpeed();
        else
            Time.timeScale = normalSpeed;

        OnPauseChanged?.Invoke(false);
    }

    public void TogglePause()
    {
        if (isPaused)
            Resume();
        else
            Pause();
    }

    private void ApplySelectedCombatSpeed()
    {
        Time.timeScale = selectedCombatSpeed;
    }

    private void LoadSavedSpeed()
    {
        selectedCombatSpeed = PlayerPrefs.GetFloat(SavedSpeedKey, normalSpeed);

        if (!Mathf.Approximately(selectedCombatSpeed, fastSpeed))
            selectedCombatSpeed = normalSpeed;
    }

    private void SaveSpeed()
    {
        PlayerPrefs.SetFloat(SavedSpeedKey, selectedCombatSpeed);
        PlayerPrefs.Save();
    }
}