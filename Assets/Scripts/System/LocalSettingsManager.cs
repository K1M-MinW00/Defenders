using UnityEngine;

public class LocalSettingsManager : MonoBehaviour
{
    private const string SoundKey = "settings_sound";
    private const string LanguageKey = "settings_language";

    public bool SoundEnabled { get; private set; }
    public string LanguageCode { get; private set; }

    private void OnEnable()
    {
        Load();
    }
    public void Load()
    {
        SoundEnabled = PlayerPrefs.GetInt(SoundKey, 1) == 1;
        LanguageCode = PlayerPrefs.GetString(LanguageKey, "ko");
    }

    public void SetSoundEnabled(bool enabled)
    {
        SoundEnabled = enabled;
        PlayerPrefs.SetInt(SoundKey, enabled ? 1 : 0);
        PlayerPrefs.Save();

        // TODO: AudioManager에 반영
    }

    public void SetLanguage(string languageCode)
    {
        LanguageCode = languageCode;
        PlayerPrefs.SetString(LanguageKey, languageCode);
        PlayerPrefs.Save();
        Debug.Log($"{LanguageKey}, {languageCode}");
        // TODO: LocalizationManager에 반영
    }
}

public class LocalSettingsData
{
    public bool SoundEnabled = true;
    public string LanguageCode = "ko";
}