using System;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class LocalizationService
{
    public const string KoreanCode = "ko";
    public const string EnglishCode = "en";

    public static event Action<string> LocaleChanged;

    static LocalizationService()
    {
        LocalizationSettings.SelectedLocaleChanged += HandleSelectedLocaleChanged;
    }

    public static string CurrentLanguageCode =>
        GetSupportedLanguageCode(LocalizationSettings.SelectedLocale?.Identifier.Code);

    public static bool IsSupportedLanguageCode(string languageCode)
    {
        return string.Equals(languageCode, KoreanCode, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(languageCode, EnglishCode, StringComparison.OrdinalIgnoreCase);
    }

    public static string GetSupportedLanguageCode(string localeCode)
    {
        if (!string.IsNullOrWhiteSpace(localeCode) &&
            localeCode.StartsWith(KoreanCode, StringComparison.OrdinalIgnoreCase))
        {
            return KoreanCode;
        }

        return EnglishCode;
    }

    public static bool TrySetLanguage(string languageCode)
    {
        if (!IsSupportedLanguageCode(languageCode) || GameSettingsManager.Instance == null)
            return false;

        GameSettingsManager.Instance.SetLanguage(languageCode.ToLowerInvariant());
        return true;
    }

    public static string Get(LocalizationKey key, params object[] arguments)
    {
        if (!ValidateKey(key))
            return GetMissingKeyText(key);

        return LocalizationSettings.StringDatabase.GetLocalizedString(
            key.Table,
            key.Entry,
            LocalizationSettings.SelectedLocale,
            FallbackBehavior.UseFallback,
            arguments);
    }

    public static string Get(string table, string entry, params object[] arguments)
    {
        return Get(new LocalizationKey(table, entry), arguments);
    }

    public static AsyncOperationHandle<string> GetAsync(LocalizationKey key, params object[] arguments)
    {
        if (!key.IsValid)
            throw new ArgumentException("A localization table and entry key are required.", nameof(key));

        return LocalizationSettings.StringDatabase.GetLocalizedStringAsync(
            key.Table,
            key.Entry,
            LocalizationSettings.SelectedLocale,
            FallbackBehavior.UseFallback,
            arguments);
    }

    public static AsyncOperationHandle<string> GetAsync(
        string table,
        string entry,
        params object[] arguments)
    {
        return GetAsync(new LocalizationKey(table, entry), arguments);
    }

    private static bool ValidateKey(LocalizationKey key)
    {
        if (key.IsValid)
            return true;

        Debug.LogWarning($"[LocalizationService] Invalid key: {key}");
        return false;
    }

    private static string GetMissingKeyText(LocalizationKey key)
    {
        string table = string.IsNullOrWhiteSpace(key.Table) ? "?" : key.Table;
        string entry = string.IsNullOrWhiteSpace(key.Entry) ? "?" : key.Entry;
        return $"[{table}:{entry}]";
    }

    private static void HandleSelectedLocaleChanged(Locale locale)
    {
        LocaleChanged?.Invoke(GetSupportedLanguageCode(locale?.Identifier.Code));
    }
}
