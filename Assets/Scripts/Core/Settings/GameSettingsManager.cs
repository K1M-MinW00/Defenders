using System;
using UnityEngine;

public class GameSettingsManager : MonoBehaviour
{
    public static GameSettingsManager Instance { get; private set; }

    private const string SoundKey = "Setting_Sound";
    private const string LanguageKey = "Setting_Language";
    
    private const string VibrationKey = "Setting_Vibration";
    private const string PushKey = "Setting_Push";


    public bool SoundEnabled { get; private set; }
    public string LanguageCode { get; private set; }

    public event Action<bool> OnSoundChanged;
    public event Action<string> OnLanguageChanged;
    public event Action OnSettingsChanged;

    public bool VibrationEnabled { get; private set; }
    public bool PushEnabled { get; private set; }


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Load();
        Apply();
    }

    private void Load()
    {
        SoundEnabled = PlayerPrefs.GetInt(SoundKey, 1) == 1;
        LanguageCode = PlayerPrefs.GetString(LanguageKey, "ko");

        // 
        VibrationEnabled = PlayerPrefs.GetInt(VibrationKey, 1) == 1;
        PushEnabled = PlayerPrefs.GetInt(PushKey, 1) == 1;
    }

    public void SetSound(bool enabled)
    {
        if (SoundEnabled == enabled)
            return;

        SoundEnabled = enabled;
        PlayerPrefs.SetInt(SoundKey, enabled ? 1 : 0);
        PlayerPrefs.Save();

        ApplySound();
        OnSoundChanged?.Invoke(enabled);
    }

    public void SetLanguage(string languageCode)
    {
        if (string.IsNullOrEmpty(languageCode))
            return;

        if (LanguageCode == languageCode)
            return;

        LanguageCode = languageCode;

        PlayerPrefs.SetString(LanguageKey, LanguageCode);
        PlayerPrefs.Save();

        ApplyLanguage();

        OnLanguageChanged?.Invoke(languageCode);
    }

    public void SetVibration(bool enabled)
    {
        VibrationEnabled = enabled;
        PlayerPrefs.SetInt(VibrationKey, enabled ? 1 : 0);
        PlayerPrefs.Save();

        OnSettingsChanged?.Invoke();
    }

    public void SetPush(bool enabled)
    {
        PushEnabled = enabled;
        PlayerPrefs.SetInt(PushKey, enabled ? 1 : 0);
        PlayerPrefs.Save();

        OnSettingsChanged?.Invoke();
    }

    private void Apply()
    {
        ApplySound();
        ApplyLanguage();
    }

    private void ApplySound()
    {
        AudioListener.volume = SoundEnabled ? 1f : 0f;
    }

    private void ApplyLanguage()
    {
        // LocalizationManager.Instance.SetLanguage(LangaugeCode);
    }
}