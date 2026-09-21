using System;

public sealed class SettingsPanelPresenter
{
    public static readonly string[] LanguageLabels = { "한국어", "English" };
    private static readonly string[] LanguageCodes = { "ko", "en" };

    private readonly UserDataRoot userData;
    private readonly GameSettingsManager settings;

    public SettingsPanelPresenter(UserDataRoot userData, GameSettingsManager settings)
    {
        this.userData = userData ?? throw new ArgumentNullException(nameof(userData));
        this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public SettingsPanelViewState Build()
    {
        UserProfileData profile = userData.Profile;
        if (profile == null)
            return null;

        return new SettingsPanelViewState
        {
            ProfileIcon = ProfileIconResolver.ResolveIcon(profile.IconId, userData.Roster),
            Nickname = profile.Nickname,
            UserId = profile.UserId,
            Level = profile.Level,
            SoundEnabled = settings.SoundEnabled,
            LanguageIndex = GetLanguageIndex(settings.LanguageCode),
        };
    }

    public void SetSound(bool enabled)
    {
        settings.SetSound(enabled);
    }

    public void SetLanguage(int index)
    {
        if (index < 0 || index >= LanguageCodes.Length)
            return;

        settings.SetLanguage(LanguageCodes[index]);
    }

    public static int GetLanguageIndex(string languageCode)
    {
        for (int i = 0; i < LanguageCodes.Length; i++)
        {
            if (LanguageCodes[i] == languageCode)
                return i;
        }

        return 0;
    }
}
