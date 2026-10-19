public static class MailTitleResolver
{
    public static string Resolve(MailData mail, string languageCode = null)
    {
        if (mail == null)
            return string.Empty;

        string language = string.IsNullOrWhiteSpace(languageCode)
            ? LocalizationService.CurrentLanguageCode
            : LocalizationService.GetSupportedLanguageCode(languageCode);
        string localized = language == LocalizationService.KoreanCode
            ? mail.TitleKo
            : mail.TitleEn;

        if (!string.IsNullOrWhiteSpace(localized))
            return localized;
        if (!string.IsNullOrWhiteSpace(mail.Title))
            return mail.Title;

        return language == LocalizationService.KoreanCode
            ? mail.TitleEn ?? string.Empty
            : mail.TitleKo ?? string.Empty;
    }
}
