using System;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

[Serializable]
public sealed class SupportedDeviceLocaleSelector : IStartupLocaleSelector
{
    public Locale GetStartupLocale(ILocalesProvider availableLocales)
    {
        string localeCode = GetLanguageCode(Application.systemLanguage);
        return availableLocales.GetLocale(localeCode);
    }

    public static string GetLanguageCode(SystemLanguage systemLanguage)
    {
        return systemLanguage == SystemLanguage.Korean ? "ko" : "en";
    }
}
