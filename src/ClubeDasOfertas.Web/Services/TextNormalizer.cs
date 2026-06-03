using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ClubeDasOfertas.Web.Services;

public static partial class TextNormalizer
{
    public static string NormalizeKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(char.ToUpperInvariant(c));
            }
        }

        return Spaces().Replace(builder.ToString(), " ").Trim();
    }

    public static string EscapeCsv(string value)
    {
        value ??= string.Empty;
        if (!value.Contains(';') && !value.Contains('"') && !value.Contains('\n') && !value.Contains('\r'))
        {
            return value;
        }

        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex Spaces();
}
