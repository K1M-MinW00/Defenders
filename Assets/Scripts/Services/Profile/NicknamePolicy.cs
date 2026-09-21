using System.Globalization;
using System.Text;

public static class NicknamePolicy
{
    public const int MinLength = 2;
    public const int MaxLength = 12;

    public static string Normalize(string nickname)
    {
        return nickname?.Trim().Normalize(NormalizationForm.FormKC) ?? string.Empty;
    }

    public static bool IsValid(string nickname)
    {
        string normalized = Normalize(nickname);
        if (normalized.Length < MinLength || normalized.Length > MaxLength)
            return false;

        foreach (char character in normalized)
        {
            UnicodeCategory category = char.GetUnicodeCategory(character);
            if (char.IsControl(character) || category == UnicodeCategory.LineSeparator ||
                category == UnicodeCategory.ParagraphSeparator)
                return false;
        }

        return true;
    }
}
