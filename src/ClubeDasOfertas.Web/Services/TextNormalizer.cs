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

    public static bool IsAllCatalogItemsFilter(string? value)
    {
        var normalized = NormalizeKey(value ?? string.Empty);
        return string.IsNullOrWhiteSpace(normalized)
            || normalized == "TODOS OS ITENS"
            || normalized == "TODAS AS CATEGORIAS";
    }

    public static string NormalizeCatalogCategoryKey(string? value)
    {
        if (IsAllCatalogItemsFilter(value))
        {
            return "SEM CATEGORIA";
        }

        var normalized = NormalizeKey(value ?? string.Empty);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return "SEM CATEGORIA";
        }

        if (normalized.Contains("MERCEARIA") && normalized.Contains("BASIC"))
        {
            return "MERCEARIA BASICO";
        }

        if (normalized.Contains("MERCEARIA") && normalized.Contains("ALTO") && normalized.Contains("GIRO"))
        {
            return "MERCEARIA ALTO GIRO";
        }

        if (normalized.Contains("MATINA"))
        {
            return "MATINAIS";
        }

        if (normalized.Contains("SOBREMESA"))
        {
            return "SOBREMESAS";
        }

        if ((normalized.Contains("FRIOS") && normalized.Contains("HORTIFRUTI")) || normalized.Contains("HORTIFRUTI"))
        {
            return "HORTIFRUTI";
        }

        if (normalized.Contains("FRIOS"))
        {
            return "FRIOS";
        }

        if (normalized.Contains("BEBIDA"))
        {
            return "BEBIDAS";
        }

        if (normalized.Contains("PERFUMARIA"))
        {
            return "PERFUMARIA";
        }

        if (normalized.Contains("LIMPEZA"))
        {
            return "LIMPEZA";
        }

        if (normalized.Contains("BAZAR"))
        {
            return "BAZAR";
        }

        return normalized;
    }

    public static string DisplayCatalogCategory(string? value)
    {
        return NormalizeCatalogCategoryKey(value) switch
        {
            "MERCEARIA BASICO" => "Mercearia básico",
            "MERCEARIA ALTO GIRO" => "Mercearia alto giro",
            "MATINAIS" => "Matinais",
            "SOBREMESAS" => "Sobremesas",
            "FRIOS" => "Frios",
            "HORTIFRUTI" => "Hortifruti",
            "BEBIDAS" => "Bebidas",
            "PERFUMARIA" => "Perfumaria",
            "LIMPEZA" => "Limpeza",
            "BAZAR" => "Bazar",
            "SEM CATEGORIA" => "Sem categoria",
            _ => value?.Trim() ?? "Sem categoria"
        };
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex Spaces();
}
