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

        var parsed = TryEvaluateDecimalExpression(cleaned, out money);
        money = Math.Round(money, 2, MidpointRounding.AwayFromZero);
        return parsed;
    }

    public static (decimal Quantity, string Unit, bool IsValid) ParseQuantity(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return (0m, "Unidades", false);
        }

        var normalized = TextNormalizer.NormalizeKey(value);
        var unitOnly = ResolveQuantityUnit(normalized);
        if (!ContainsDigit(value) && !string.IsNullOrWhiteSpace(unitOnly))
        {
            return (1m, unitOnly, true);
        }

        var cleaned = SanitizeQuantityExpression(value);
        if (string.IsNullOrWhiteSpace(cleaned))
        {
            return (0m, "Unidades", false);
        }

        if (!TryEvaluateDecimalExpression(cleaned, out var quantity))
        {
            return (0m, "Unidades", false);
        }

        var unit = string.IsNullOrWhiteSpace(unitOnly) ? "Unidades" : unitOnly;

        return (quantity, unit, quantity > 0m);
    }

    private static bool TryEvaluateDecimalExpression(string value, out decimal result)
    {
        result = 0m;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var parser = new DecimalExpressionParser(NormalizeExpressionOperators(value));
        if (!parser.TryParse(out result))
        {
            return false;
        }

        return true;
    }

    private static string NormalizeExpressionOperators(string value)
    {
        return value
            .Replace('x', '*')
            .Replace('X', '*')
            .Replace('×', '*')
            .Replace('÷', '/');
    }

    private static string SanitizeQuantityExpression(string value)
    {
        var trimmed = value.Trim();
        var withoutWords = LetterSequences().Replace(trimmed, match =>
        {
            return match.Value.Length == 1 && match.Value.Equals("x", StringComparison.OrdinalIgnoreCase)
                ? match.Value
                : " ";
        });

        return QuantityExpressionCharacters().Replace(withoutWords, "");
    }

    private static bool ContainsDigit(string value)
    {
        foreach (var character in value)
        {
            if (char.IsDigit(character))
            {
                return true;
            }
        }

        return false;
    }

    private static string ResolveQuantityUnit(string normalized)
    {
        return normalized switch
        {
            var s when s.Contains("KG") => "Kg",
            var s when s.Contains("FARDO") || s.Contains("FARDOS") || s.Contains("FD") => "Fardos",
            var s when s.Contains("CAIXA") || s.Contains("CAIXAS") || s.Contains("CX") => "Caixas",
            var s when s.Contains("UNIDADE") || s.Contains("UNIDADES") => "Unidades",
            _ => ""
        };
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

    [GeneratedRegex(@"[^\d,.\-+*/()xX×÷]")]
    private static partial Regex MoneyCharacters();

    [GeneratedRegex(@"[^\d\s,.\-+*/()xX×÷]")]
    private static partial Regex QuantityExpressionCharacters();

    [GeneratedRegex(@"\p{L}+")]
    private static partial Regex LetterSequences();

    [GeneratedRegex(@"\D")]
    private static partial Regex DigitsOnly();

    private sealed class DecimalExpressionParser(string input)
    {
        private readonly string _input = input;
        private int _index;

        public bool TryParse(out decimal value)
        {
            value = 0m;

            if (!TryParseExpression(out value))
            {
                return false;
            }

            SkipWhitespace();
            return _index == _input.Length;
        }

        private bool TryParseExpression(out decimal value)
        {
            if (!TryParseTerm(out value))
            {
                return false;
            }

            while (true)
            {
                SkipWhitespace();
                if (Match('+'))
                {
                    if (!TryParseTerm(out var right))
                    {
                        return false;
                    }

                    value += right;
                    continue;
                }

                if (Match('-'))
                {
                    if (!TryParseTerm(out var right))
                    {
                        return false;
                    }

                    value -= right;
                    continue;
                }

                return true;
            }
        }

        private bool TryParseTerm(out decimal value)
        {
            if (!TryParseFactor(out value))
            {
                return false;
            }

            while (true)
            {
                SkipWhitespace();
                if (Match('*'))
                {
                    if (!TryParseFactor(out var right))
                    {
                        return false;
                    }

                    value *= right;
                    continue;
                }

                if (Match('/'))
                {
                    if (!TryParseFactor(out var right) || right == 0m)
                    {
                        return false;
                    }

                    value /= right;
                    continue;
                }

                return true;
            }
        }

        private bool TryParseFactor(out decimal value)
        {
            SkipWhitespace();

            if (Match('+'))
            {
                return TryParseFactor(out value);
            }

            if (Match('-'))
            {
                if (!TryParseFactor(out value))
                {
                    return false;
                }

                value = -value;
                return true;
            }

            if (Match('('))
            {
                if (!TryParseExpression(out value))
                {
                    return false;
                }

                SkipWhitespace();
                return Match(')');
            }

            return TryParseNumber(out value);
        }

        private bool TryParseNumber(out decimal value)
        {
            value = 0m;
            SkipWhitespace();

            var start = _index;
            while (_index < _input.Length)
            {
                var current = _input[_index];
                if (char.IsDigit(current) || current is '.' or ',')
                {
                    _index++;
                    continue;
                }

                break;
            }

            if (start == _index)
            {
                return false;
            }

            var token = _input[start.._index];
            var normalized = NormalizeNumberToken(token);
            return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
        }

        private static string NormalizeNumberToken(string token)
        {
            var sanitized = token.Trim();
            if (sanitized.Contains(','))
            {
                sanitized = sanitized.Replace(".", "", StringComparison.Ordinal)
                    .Replace(',', '.');
            }

            return sanitized;
        }

        private void SkipWhitespace()
        {
            while (_index < _input.Length && char.IsWhiteSpace(_input[_index]))
            {
                _index++;
            }
        }

        private bool Match(char expected)
        {
            if (_index >= _input.Length || _input[_index] != expected)
            {
                return false;
            }

            _index++;
            return true;
        }
    }
}
