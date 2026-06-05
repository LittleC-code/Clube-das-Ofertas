using ClubeDasOfertas.Web.Data;
using ClubeDasOfertas.Web.Domain;
using System.Text.RegularExpressions;

namespace ClubeDasOfertas.Web.Services;

public sealed partial class CampaignImportService(
    SpreadsheetImporter importer,
    AppRepository repository)
{
    public async Task<ImportBatch> ImportAsync(Campaign campaign, IFormFile file, UserAccount user, string preferredSheet = SpreadsheetImporter.DefaultCampaignSheetName, CancellationToken cancellationToken = default)
    {
        var selectedSheet = string.IsNullOrWhiteSpace(preferredSheet) ? SpreadsheetImporter.DefaultCampaignSheetName : preferredSheet.Trim();
        var rawRows = await importer.ReadCampaignRowsAsync(file, selectedSheet, cancellationToken);
        var rules = (await repository.ListRulesAsync(cancellationToken)).Where(x => x.IsActive).ToList();
        var items = new List<CampaignItem>();
        var batch = new ImportBatch(Guid.NewGuid(), campaign.Id, file.FileName, user.Id, DateTimeOffset.UtcNow, rawRows.Count);

        foreach (var row in rawRows)
        {
            var normalizedDescription = TextNormalizer.NormalizeKey(row.DescriptionTabloid);
            var matches = await repository.FindCatalogMatchesAsync(normalizedDescription, cancellationToken);

            if (matches.Count == 0)
            {
                items.Add(EvaluateItem(campaign.Id, batch.Id, row, null, rules));
                continue;
            }

            foreach (var match in matches)
            {
                items.Add(EvaluateItem(campaign.Id, batch.Id, row, match, rules));
            }
        }

        items = MarkDuplicateBarcodes(items);
        await repository.ReplaceCampaignItemsAsync(batch, items, cancellationToken);
        await repository.AddAuditAsync(user.Id, user.Email, "Importou campanha", "Campaign", campaign.Id, $"{file.FileName} / aba {selectedSheet} ({items.Count} linhas processadas)", cancellationToken);

        return batch;
    }

    internal static CampaignItem EvaluateItem(
        Guid campaignId,
        Guid batchId,
        RawCampaignRow row,
        ProductCatalogEntry? catalogEntry,
        IReadOnlyList<ConversionRule> rules)
    {
        var now = DateTimeOffset.UtcNow;
        var risks = new List<string>();
        var blockers = new List<string>();
        var parsedQuantity = Parsing.ParseQuantity(row.QuantityRaw);
        var saleOk = Parsing.TryMoney(row.PriceSaleRaw, out var salePrice);
        var clubOk = Parsing.TryMoney(row.PriceClubRaw, out var clubPrice);

        if (catalogEntry is null)
        {
            risks.Add("SEM_CATALOGO");
            blockers.Add("Produto sem catalogo/codigo");
        }

        if (!saleOk || !clubOk || salePrice <= 0m || clubPrice <= 0m)
        {
            risks.Add("PRECO_INVALIDO");
            blockers.Add("Preco invalido ou zerado");
        }

        if (!parsedQuantity.IsValid)
        {
            risks.Add("QUANTIDADE_INVALIDA");
            blockers.Add("Quantidade invalida");
        }

        var combinedText = TextNormalizer.NormalizeKey($"{row.DescriptionTabloid} {catalogEntry?.DescriptionSolidus ?? ""}");
        var weightedRule = rules.FirstOrDefault(x => x.RuleType == RuleTypes.Weighted && RuleMatches(x, combinedText));
        var packageRule = rules.FirstOrDefault(x => x.RuleType == RuleTypes.Package && RuleMatches(x, combinedText));

        var finalSale = salePrice;
        var finalClub = clubPrice;
        var unit = parsedQuantity.Unit;
        var hasHundredGram = HundredGram().IsMatch(combinedText);
        var originalIsKg = parsedQuantity.Unit.Equals("Kg", StringComparison.OrdinalIgnoreCase);
        var requiresReview = false;
        var reviewStatus = ReviewStatus.NotRequired;

        if (weightedRule is not null && (!originalIsKg || hasHundredGram))
        {
            risks.Add("PESAVEL");
            finalSale = Math.Round(salePrice * weightedRule.Multiplier, 2, MidpointRounding.AwayFromZero);
            finalClub = Math.Round(clubPrice * weightedRule.Multiplier, 2, MidpointRounding.AwayFromZero);
            unit = string.IsNullOrWhiteSpace(weightedRule.TargetUnit) ? "Kg" : weightedRule.TargetUnit;

            if (weightedRule.RequiresReview)
            {
                requiresReview = true;
                reviewStatus = ReviewStatus.Pending;
                blockers.Add("Conversao de pesavel pendente");
            }
        }

        if (packageRule is not null)
        {
            risks.Add("FARDO_CAIXA");
            if (packageRule.RequiresReview)
            {
                requiresReview = true;
                reviewStatus = ReviewStatus.Pending;
                blockers.Add("Fardo/caixa pendente");
            }
        }

        return new CampaignItem(
            Guid.NewGuid(),
            campaignId,
            batchId,
            row.SourceRow,
            row.Source.Trim(),
            row.OriginalVigency.Trim(),
            row.DescriptionTabloid.Trim(),
            TextNormalizer.NormalizeKey(row.DescriptionTabloid),
            row.QuantityRaw.Trim(),
            parsedQuantity.Quantity,
            unit,
            salePrice,
            clubPrice,
            finalSale,
            finalClub,
            catalogEntry?.DescriptionSolidus.Trim() ?? "",
            catalogEntry?.Barcode.Trim() ?? "",
            catalogEntry?.CodeType.Trim() ?? "",
            risks.Distinct().ToList(),
            blockers.Distinct().ToList(),
            requiresReview,
            reviewStatus,
            now,
            now);
    }

    internal static List<CampaignItem> MarkDuplicateBarcodes(List<CampaignItem> items)
    {
        items = items
            .Select(item => item with
            {
                RiskFlags = item.RiskFlags
                    .Where(flag => !string.Equals(flag, "DUPLICIDADE", StringComparison.OrdinalIgnoreCase))
                    .ToList()
            })
            .ToList();

        var duplicateBarcodes = items
            .Where(x => !string.IsNullOrWhiteSpace(x.Barcode))
            .GroupBy(x => x.Barcode)
            .Where(x => x.Count() > 1)
            .Select(x => x.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (duplicateBarcodes.Count == 0)
        {
            return items;
        }

        return items
            .Select(item =>
            {
                if (!duplicateBarcodes.Contains(item.Barcode))
                {
                    return item;
                }

                var risks = item.RiskFlags.ToList();
                risks.Add("DUPLICIDADE");
                return item with { RiskFlags = risks.Distinct().ToList() };
            })
            .ToList();
    }

    private static bool RuleMatches(ConversionRule rule, string normalizedText)
    {
        try
        {
            return Regex.IsMatch(normalizedText, rule.Pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(150));
        }
        catch (ArgumentException)
        {
            return normalizedText.Contains(TextNormalizer.NormalizeKey(rule.Pattern), StringComparison.OrdinalIgnoreCase);
        }
    }

    [GeneratedRegex(@"(100\s*G|CADA\s*100\s*G)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex HundredGram();
}
