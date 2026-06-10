using ClubeDasOfertas.Web.Domain;
using System.Text.RegularExpressions;

namespace ClubeDasOfertas.Web.Services;

public static partial class RulePatternConverter
{
    public static RulePatternConversion Convert(string input)
    {
        var raw = input?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new ArgumentException("Informe como a regra deve acontecer.", nameof(input));
        }

        var semanticCriterion = ExtractSemanticCriterion(raw);
        var normalizedFriendlyInput = NormalizeFriendlyInput(semanticCriterion ?? raw);

        if (semanticCriterion is null && LooksLikeRegex(raw))
        {
            return new RulePatternConversion(raw, raw, true);
        }

        var alternatives = SplitAlternatives(normalizedFriendlyInput)
            .Select(ToRegexAlternative)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (alternatives.Count == 0)
        {
            throw new ArgumentException("Não foi possível converter a regra informada.", nameof(input));
        }

        var pattern = alternatives.Count == 1
            ? alternatives[0]
            : $"(?:{string.Join('|', alternatives)})";

        return new RulePatternConversion(raw, pattern, false);
    }

    public static string DisplayPatternInput(ConversionRule rule)
    {
        return string.IsNullOrWhiteSpace(rule.PatternInput) ? rule.Pattern : rule.PatternInput;
    }

    public static RuleMatchField ResolveMatchField(ConversionRule rule)
    {
        var normalized = TextNormalizer.NormalizeKey(DisplayPatternInput(rule));
        if (normalized.Contains("DESCRICAO SOLIDUS", StringComparison.Ordinal) || normalized.Contains("COLUNA SOLIDUS", StringComparison.Ordinal))
        {
            return RuleMatchField.Solidus;
        }

        if (normalized.Contains("DESCRICAO TABLOIDE", StringComparison.Ordinal) || normalized.Contains("COLUNA TABLOIDE", StringComparison.Ordinal))
        {
            return RuleMatchField.Tabloid;
        }

        return RuleMatchField.Combined;
    }

    private static IEnumerable<string> SplitAlternatives(string input)
    {
        return AlternativeSeparator()
            .Split(input)
            .Select(part => part.Trim())
            .Where(part => !string.IsNullOrWhiteSpace(part));
    }

    private static string ToRegexAlternative(string input)
    {
        var normalized = TextNormalizer.NormalizeKey(input);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return "";
        }

        var escaped = Regex.Escape(normalized);
        escaped = escaped.Replace(@"\*", ".*");
        escaped = escaped.Replace(@"\ ", @"\s*");
        escaped = escaped.Replace("/", @"\s*/\s*");
        escaped = escaped.Replace(@"\-", @"\s*-\s*");
        escaped = escaped.Replace(@"\+", @"\s*\+\s*");

        return $@"(?<!\w){escaped}(?!\w)";
    }

    private static string NormalizeFriendlyInput(string input)
    {
        return WordSlashSeparator().Replace(input, "$1;$2");
    }

    private static string? ExtractSemanticCriterion(string input)
    {
        var normalized = TextNormalizer.NormalizeKey(input);
        var match = NaturalLanguageCriterion().Match(normalized);
        if (!match.Success)
        {
            return null;
        }

        var value = match.Groups["value"].Value.Trim();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static bool LooksLikeRegex(string input)
    {
        return RegexLiteralHint().IsMatch(input);
    }

    [GeneratedRegex(@"\s*(?:\r?\n|;|,|\||\bou\b)\s*", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex AlternativeSeparator();

    [GeneratedRegex(@"(?<=^|[\s;(])([A-Za-zÀ-ÿ]{1,})(?:\s*)/(?:\s*)([A-Za-zÀ-ÿ]{1,})(?=$|[\s;,)])", RegexOptions.CultureInvariant)]
    private static partial Regex WordSlashSeparator();

    [GeneratedRegex(@"(?:SE\s+)?(?:CONTER|CONTIVER)\s+(?:A\s+PALAVRA|O\s+TERMO)?\s*(?<value>.+?)\s+NA\s+(?:DESCRICAO\s+(?:DO\s+)?)?(?:SOLIDUS|TABLOIDE)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex NaturalLanguageCriterion();

    [GeneratedRegex(@"\\[bBdDsSwW]|[\[\]\(\)\{\}\^\$]|(?<!\\)[+?]|(?<!\.)\.\*|(?<!\\)\{.+?(?<!\\)\}")]
    private static partial Regex RegexLiteralHint();
}

public sealed record RulePatternConversion(
    string PatternInput,
    string Pattern,
    bool IsRegexLiteral);

public enum RuleMatchField
{
    Combined,
    Tabloid,
    Solidus
}
