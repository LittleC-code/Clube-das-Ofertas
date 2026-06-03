using System.Globalization;
using System.Text.RegularExpressions;

namespace ClubeDasOfertas.Web.Services;

public static partial class Parsing
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    public static bool TryMoney(string value, out decimal money)
    {
        money = 0m;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var cleaned = value
            .Replace("R$", "", StringComparison.OrdinalIgnoreCase)
            .Replace("\u00a0", " ")
            .Trim();

        cleaned = MoneyCharacters().Replace(cleaned, "");

        if (string.IsNullOrWhiteSpace(cleaned))
        {
            return false;
        }

        if (cleaned.Contains(','))
        {
            cleaned = cleaned.Replace(".", "");
            var parsed = decimal.TryParse(cleaned, NumberStyles.Number, PtBr, out money);
            money = Math.Round(money, 2, MidpointRounding.AwayFromZero);
            return parsed;
        }

        var invariantParsed = decimal.TryParse(cleaned, NumberStyles.Number, CultureInfo.InvariantCulture, out money);
        money = Math.Round(money, 2, MidpointRounding.AwayFromZero);
        return invariantParsed;
    }

    public static (decimal Quantity, string Unit, bool IsValid) ParseQuantity(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return (0m, "Unidades", false);
        }

        var match = FirstNumber().Match(value);
        if (!match.Success)
        {
            return (0m, "Unidades", false);
        }

        var number = match.Value.Replace(',', '.');
        if (!decimal.TryParse(number, NumberStyles.Number, CultureInfo.InvariantCulture, out var quantity))
        {
            return (0m, "Unidades", false);
        }

        var normalized = TextNormalizer.NormalizeKey(value);
        var unit = normalized switch
        {
            var s when s.Contains("KG") => "Kg",
            var s when s.Contains("FARDO") || s.Contains("FARDOS") || s.Contains("FD") => "Fardos",
            var s when s.Contains("CAIXA") || s.Contains("CX") => "Caixas",
            _ => "Unidades"
        };

        return (quantity, unit, quantity > 0m);
    }

    public static string CodeType(string barcode)
    {
        var digits = DigitsOnly().Replace(barcode ?? string.Empty, "");
        if (digits.Length == 0)
        {
            return string.Empty;
        }

        return digits.Length < 6 ? "Codigo Unificado" : "EAN";
    }

    public static string MoneyPtBr(decimal value)
    {
        return value.ToString("0.##", PtBr);
    }

    public static string DatePtBr(DateOnly value)
    {
        return value.ToString("dd.MM.yyyy", PtBr);
    }

    [GeneratedRegex(@"[^\d,.-]")]
    private static partial Regex MoneyCharacters();

    [GeneratedRegex(@"\d+(?:[,.]\d+)?")]
    private static partial Regex FirstNumber();

    [GeneratedRegex(@"\D")]
    private static partial Regex DigitsOnly();
}
