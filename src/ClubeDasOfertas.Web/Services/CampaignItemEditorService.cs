using ClubeDasOfertas.Web.Data;
using ClubeDasOfertas.Web.Domain;

namespace ClubeDasOfertas.Web.Services;

public sealed record CampaignItemEditInput(
    string DescriptionTabloid,
    string DescriptionSolidus,
    string Barcode,
    string QuantityRaw,
    string PriceSaleRaw,
    string PriceClubRaw);

public sealed record CampaignItemCreateInput(
    string DescriptionTabloid,
    string DescriptionSolidus,
    string Barcode,
    string QuantityRaw,
    string PriceSaleRaw,
    string PriceClubRaw);

public sealed class CampaignItemEditorService(AppRepository repository)
{
    public async Task<CampaignItem> CreateAsync(Guid campaignId, CampaignItemCreateInput input, UserAccount user, CancellationToken cancellationToken = default)
    {
        var campaign = await repository.GetCampaignAsync(campaignId, cancellationToken)
            ?? throw new InvalidOperationException("Campanha não encontrada.");

        var descriptionTabloid = input.DescriptionTabloid.Trim();
        if (string.IsNullOrWhiteSpace(descriptionTabloid))
        {
            throw new InvalidOperationException("Informe a descrição do item.");
        }

        var quantityRaw = input.QuantityRaw.Trim();
        var priceSaleRaw = input.PriceSaleRaw.Trim();
        var priceClubRaw = input.PriceClubRaw.Trim();
        var source = "Manual";
        var originalVigency = $"{campaign.ValidFrom:dd/MM/yyyy} a {campaign.ValidTo:dd/MM/yyyy}";

        ProductCatalogEntry? autoMatch = null;
        var barcode = input.Barcode.Trim();
        if (!string.IsNullOrWhiteSpace(barcode))
        {
            autoMatch = await repository.GetCatalogEntryByBarcodeAsync(barcode, cancellationToken);
        }

        if (autoMatch is null)
        {
            var matches = await repository.FindCatalogMatchesAsync(TextNormalizer.NormalizeKey(descriptionTabloid), cancellationToken);
            if (matches.Count == 1)
            {
                autoMatch = matches[0];
            }
        }

        var resolvedSolidus = string.IsNullOrWhiteSpace(input.DescriptionSolidus)
            ? autoMatch?.DescriptionSolidus.Trim() ?? ""
            : input.DescriptionSolidus.Trim();

        if (string.IsNullOrWhiteSpace(barcode))
        {
            barcode = autoMatch?.Barcode.Trim() ?? "";
        }

        ProductCatalogEntry? resolvedCatalog = null;
        if (!string.IsNullOrWhiteSpace(barcode))
        {
            resolvedCatalog = new ProductCatalogEntry(
                autoMatch?.Id ?? Guid.Empty,
                descriptionTabloid,
                TextNormalizer.NormalizeKey(descriptionTabloid),
                autoMatch?.Category ?? "",
                resolvedSolidus,
                TextNormalizer.NormalizeKey(resolvedSolidus),
                barcode,
                Parsing.CodeType(barcode),
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow);
        }

        var rules = (await repository.ListRulesAsync(cancellationToken))
            .Where(x => x.IsActive)
            .ToList();

        var existingItems = (await repository.GetCampaignItemsAsync(campaignId, cancellationToken)).ToList();
        var nextSourceRow = existingItems.Count == 0 ? 1 : existingItems.Max(x => x.SourceRow) + 1;
        var batch = new ImportBatch(Guid.NewGuid(), campaign.Id, "Inclusão manual", user.Id, DateTimeOffset.UtcNow, 1);

        var createdItem = CampaignImportService.EvaluateItem(
            campaign.Id,
            batch.Id,
            new RawCampaignRow(
                nextSourceRow,
                source,
                originalVigency,
                descriptionTabloid,
                quantityRaw,
                priceSaleRaw,
                priceClubRaw),
            resolvedCatalog,
            rules);

        if (!string.IsNullOrWhiteSpace(resolvedSolidus))
        {
            createdItem = createdItem with { DescriptionSolidus = resolvedSolidus };
        }

        var campaignItems = existingItems
            .Concat([createdItem])
            .ToList();

        campaignItems = CampaignImportService.MarkDuplicateBarcodes(campaignItems);
        var finalCreatedItem = campaignItems.First(x => x.Id == createdItem.Id);
        var itemsToUpdate = campaignItems.Where(x => x.Id != finalCreatedItem.Id).ToList();

        await repository.AddManualCampaignItemAsync(batch, finalCreatedItem, itemsToUpdate, cancellationToken);
        await repository.AddAuditAsync(
            user.Id,
            user.Email,
            "Incluiu item manualmente",
            "CampaignItem",
            finalCreatedItem.Id,
            $"{finalCreatedItem.DescriptionTabloid} / {finalCreatedItem.Barcode}",
            cancellationToken);

        return finalCreatedItem;
    }

    public async Task<CampaignItem> SaveAsync(Guid itemId, CampaignItemEditInput input, UserAccount user, CancellationToken cancellationToken = default)
    {
        var currentItem = await repository.GetCampaignItemAsync(itemId, cancellationToken)
            ?? throw new InvalidOperationException("Item não encontrado.");

        var rules = (await repository.ListRulesAsync(cancellationToken))
            .Where(x => x.IsActive)
            .ToList();

        var editedItem = await BuildEditedItemAsync(currentItem, input, rules, cancellationToken);
        var campaignItems = (await repository.GetCampaignItemsAsync(currentItem.CampaignId, cancellationToken)).ToList();

        for (var index = 0; index < campaignItems.Count; index++)
        {
            if (campaignItems[index].Id == editedItem.Id)
            {
                campaignItems[index] = editedItem;
                break;
            }
        }

        campaignItems = CampaignImportService.MarkDuplicateBarcodes(campaignItems);
        await repository.UpdateCampaignItemsAsync(campaignItems, cancellationToken);
        await repository.UpdateCampaignStatusAsync(currentItem.CampaignId, CampaignStatus.Imported, cancellationToken);

        var savedItem = campaignItems.First(x => x.Id == itemId);
        await repository.AddAuditAsync(
            user.Id,
            user.Email,
            "Editou item manualmente",
            "CampaignItem",
            savedItem.Id,
            $"{savedItem.DescriptionTabloid} / {savedItem.Barcode}",
            cancellationToken);

        return savedItem;
    }

    private async Task<CampaignItem> BuildEditedItemAsync(
        CampaignItem currentItem,
        CampaignItemEditInput input,
        IReadOnlyList<ConversionRule> rules,
        CancellationToken cancellationToken)
    {
        var descriptionTabloid = input.DescriptionTabloid.Trim();
        if (string.IsNullOrWhiteSpace(descriptionTabloid))
        {
            throw new InvalidOperationException("Informe a descrição do item.");
        }

        var quantityRaw = input.QuantityRaw.Trim();
        var priceSaleRaw = input.PriceSaleRaw.Trim();
        var priceClubRaw = input.PriceClubRaw.Trim();

        ProductCatalogEntry? autoMatch = null;
        var matches = await repository.FindCatalogMatchesAsync(TextNormalizer.NormalizeKey(descriptionTabloid), cancellationToken);
        if (matches.Count == 1)
        {
            autoMatch = matches[0];
        }

        var resolvedSolidus = string.IsNullOrWhiteSpace(input.DescriptionSolidus)
            ? autoMatch?.DescriptionSolidus.Trim() ?? ""
            : input.DescriptionSolidus.Trim();

        var resolvedBarcode = string.IsNullOrWhiteSpace(input.Barcode)
            ? autoMatch?.Barcode.Trim() ?? ""
            : input.Barcode.Trim();

        ProductCatalogEntry? resolvedCatalog = null;
        if (!string.IsNullOrWhiteSpace(resolvedBarcode))
        {
            resolvedCatalog = new ProductCatalogEntry(
                autoMatch?.Id ?? Guid.Empty,
                descriptionTabloid,
                TextNormalizer.NormalizeKey(descriptionTabloid),
                autoMatch?.Category ?? "",
                resolvedSolidus,
                TextNormalizer.NormalizeKey(resolvedSolidus),
                resolvedBarcode,
                Parsing.CodeType(resolvedBarcode),
                currentItem.CreatedAt,
                DateTimeOffset.UtcNow);
        }

        var reEvaluated = CampaignImportService.EvaluateItem(
            currentItem.CampaignId,
            currentItem.ImportBatchId,
            new RawCampaignRow(
                currentItem.SourceRow,
                currentItem.Source,
                currentItem.OriginalVigency,
                descriptionTabloid,
                quantityRaw,
                priceSaleRaw,
                priceClubRaw),
            resolvedCatalog,
            rules);

        return reEvaluated with
        {
            Id = currentItem.Id,
            CreatedAt = currentItem.CreatedAt,
            UpdatedAt = DateTimeOffset.UtcNow,
            DescriptionSolidus = resolvedCatalog is null && !string.IsNullOrWhiteSpace(resolvedSolidus)
                ? resolvedSolidus
                : reEvaluated.DescriptionSolidus
        };
    }
}
