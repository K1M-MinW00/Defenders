using System;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public sealed class GameSettingsManager : PersistentSingleton<GameSettingsManager>
{
    private const string SoundKey = "Setting_Sound";
    private const string LanguageKey = "Setting_Language";
    private const string PushKey = "Setting_Push";


    public bool SoundEnabled { get; private set; }
    public string LanguageCode { get; private set; }

    public event Action<bool> OnSoundChanged;
    public event Action<string> OnLanguageChanged;
    public event Action OnSettingsChanged;

    public bool PushEnabled { get; private set; }


    protected override void OnSingletonAwake()
    {
        Load();
        Apply();
    }

    private void Load()
    {
        SoundEnabled = PlayerPrefs.GetInt(SoundKey, 1) == 1;
        LanguageCode = ResolveInitialLanguageCode();

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
        if (languageCode != "ko" && languageCode != "en")
            return;

        if (LanguageCode == languageCode)
            return;

        LanguageCode = languageCode;

        PlayerPrefs.SetString(LanguageKey, LanguageCode);
        PlayerPrefs.Save();

        ApplyLanguage();

        OnLanguageChanged?.Invoke(languageCode);
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
        var initialization = LocalizationSettings.InitializationOperation;
        if (initialization.IsDone)
        {
            ApplyInitializedLanguage();
            return;
        }

        initialization.Completed += _ => ApplyInitializedLanguage();
    }

    private void ApplyInitializedLanguage()
    {
        Locale locale = LocalizationSettings.AvailableLocales.GetLocale(LanguageCode);
        if (locale == null)
        {
            locale = LocalizationSettings.AvailableLocales.GetLocale("ko");
            Debug.LogWarning($"[GameSettingsManager] Locale '{LanguageCode}' is unavailable. Falling back to Korean.", this);
        }

        if (locale != null && LocalizationSettings.SelectedLocale != locale)
            LocalizationSettings.SelectedLocale = locale;
    }

    private static string ResolveInitialLanguageCode()
    {
        if (PlayerPrefs.HasKey(LanguageKey))
        {
            string savedLanguage = PlayerPrefs.GetString(LanguageKey);
            if (savedLanguage == "ko" || savedLanguage == "en")
                return savedLanguage;
        }

        return SupportedDeviceLocaleSelector.GetLanguageCode(Application.systemLanguage);
    }
}
