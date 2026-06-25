using ClubeDasOfertas.Web.Domain;
using ClubeDasOfertas.Web.Services;
using Npgsql;

namespace ClubeDasOfertas.Web.Data;

public sealed class SchemaInitializer(AppDb db, IConfiguration configuration, AppRepository repository)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await db.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(SchemaSql, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);

        await SeedConfiguredUsersAsync(cancellationToken);
        await SeedRulesAsync(cancellationToken);
        await NormalizeFriendlyRulePatternsAsync(cancellationToken);
        await ReevaluateImportedCampaignItemsAsync(cancellationToken);
    }

    private async Task SeedConfiguredUsersAsync(CancellationToken cancellationToken)
    {
        await EnsureConfiguredUserAsync(
            configuration["App:BootstrapAdminEmail"],
            configuration["App:BootstrapAdminPassword"],
            "Administrador",
            Roles.Admin,
            cancellationToken);

        await EnsureConfiguredUserAsync(
            configuration["App:BootstrapOperatorEmail"],
            configuration["App:BootstrapOperatorPassword"],
            "Operador",
            Roles.Operator,
            cancellationToken);
    }

    private async Task EnsureConfiguredUserAsync(
        string? email,
        string? password,
        string displayName,
        string role,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        await repository.EnsureUserAsync(
            email,
            displayName,
            role,
            password,
            cancellationToken);
    }

    private async Task SeedRulesAsync(CancellationToken cancellationToken)
    {
        var rules = await repository.ListRulesAsync(cancellationToken);
        if (rules.Count > 0)
        {
            await MigrateLegacyPackageRulesAsync(rules, cancellationToken);
            return;
        }

        await repository.AddRuleAsync(new ConversionRule(
            Guid.NewGuid(),
            "Pesáveis e cada 100 g",
            RuleTypes.Weighted,
            "100 G ou CADA 100 G ou PRESUNTO ou MORTADELA ou ALHO A GRANEL ou QUEIJO MUSSARELA ou MUSSARELA ou SALAME",
            @"\b(100\s*G|CADA\s*100\s*G|PRESUNTO|MORTADELA|ALHO\s*A\s*GRANEL|QUEIJO\s*MUSSARELA|MUSSARELA|SALAME)\b",
            10m,
            "Kg",
            "",
            true,
            true,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow), cancellationToken);

        await repository.AddRuleAsync(new ConversionRule(
            Guid.NewGuid(),
            "Fardos",
            RuleTypes.PackageBale,
            "FD ou FARDO ou FARDOS",
            @"\b(FD|FARDO|FARDOS)\b",
            1m,
            "",
            "",
            true,
            true,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow), cancellationToken);

        await repository.AddRuleAsync(new ConversionRule(
            Guid.NewGuid(),
            "Caixas",
            RuleTypes.PackageBox,
            "CX/* ou C/* ou C * ou CAIXA ou CAIXAS",
            @"\b(CX/?\s*\d+|C/?\s*\d+|C\s+\d+|CAIXA|CAIXAS)\b",
            1m,
            "",
            "",
            true,
            true,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow), cancellationToken);
    }

    private async Task MigrateLegacyPackageRulesAsync(IReadOnlyList<ConversionRule> rules, CancellationToken cancellationToken)
    {
        if (rules.Any(x => x.RuleType is RuleTypes.PackageBale or RuleTypes.PackageBox))
        {
            return;
        }

        var legacyRules = rules.Where(x => x.RuleType == RuleTypes.Package).ToList();
        foreach (var legacyRule in legacyRules)
        {
            await repository.AddRuleAsync(new ConversionRule(
                Guid.NewGuid(),
                $"{legacyRule.Name} - Fardos",
                RuleTypes.PackageBale,
                "FD ou FARDO ou FARDOS",
                @"\b(FD|FARDO|FARDOS)\b",
                legacyRule.Multiplier,
                legacyRule.TargetUnit,
                legacyRule.CategoryScope,
                legacyRule.RequiresReview,
                legacyRule.IsActive,
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow), cancellationToken);

            await repository.AddRuleAsync(new ConversionRule(
                Guid.NewGuid(),
                $"{legacyRule.Name} - Caixas",
                RuleTypes.PackageBox,
                "CX/* ou C/* ou C * ou CAIXA ou CAIXAS",
                @"\b(CX/?\s*\d+|C/?\s*\d+|C\s+\d+|CAIXA|CAIXAS)\b",
                legacyRule.Multiplier,
                legacyRule.TargetUnit,
                legacyRule.CategoryScope,
                legacyRule.RequiresReview,
                legacyRule.IsActive,
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow), cancellationToken);

            await repository.SetRuleActiveAsync(legacyRule.Id, false, cancellationToken);
        }
    }

    private async Task<bool> NormalizeFriendlyRulePatternsAsync(CancellationToken cancellationToken)
    {
        var rules = await repository.ListRulesAsync(cancellationToken);
        var updatedAnyRule = false;

        foreach (var rule in rules)
        {
            var patternInput = RulePatternConverter.DisplayPatternInput(rule);
            if (string.IsNullOrWhiteSpace(patternInput))
            {
                continue;
            }

            RulePatternConversion conversion;
            try
            {
                conversion = RulePatternConverter.Convert(patternInput);
            }
            catch (ArgumentException)
            {
                continue;
            }

            if (string.Equals(rule.Pattern, conversion.Pattern, StringComparison.Ordinal)
                && string.Equals(rule.PatternInput, conversion.PatternInput, StringComparison.Ordinal))
            {
                continue;
            }

            var normalizedRule = rule with
            {
                PatternInput = conversion.PatternInput,
                Pattern = conversion.Pattern,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            await repository.UpdateRuleAsync(normalizedRule, cancellationToken);
            updatedAnyRule = true;
        }

        return updatedAnyRule;
    }

    private async Task ReevaluateImportedCampaignItemsAsync(CancellationToken cancellationToken)
    {
        var rules = (await repository.ListRulesAsync(cancellationToken))
            .Where(rule => rule.IsActive)
            .ToList();
        var campaigns = await repository.ListCampaignsAsync(cancellationToken);

        foreach (var campaign in campaigns)
        {
            var items = (await repository.GetCampaignItemsAsync(campaign.Id, cancellationToken)).ToList();
            if (items.Count == 0)
            {
                continue;
            }

            var reEvaluatedItems = new List<CampaignItem>(items.Count);
            var changedAnyItem = false;
            foreach (var item in items)
            {
                if (item.ReviewStatus is ReviewStatus.Approved or ReviewStatus.Rejected)
                {
                    reEvaluatedItems.Add(item);
                    continue;
                }

                var catalogEntry = await ResolveCatalogEntryAsync(item, cancellationToken);
                var reEvaluated = CampaignImportService.EvaluateItem(
                    item.CampaignId,
                    item.ImportBatchId,
                    new RawCampaignRow(
                        item.SourceRow,
                        item.Source,
                        item.OriginalVigency,
                        item.DescriptionTabloid,
                        item.QuantityRaw,
                        item.PriceSaleRaw,
                        item.PriceClubRaw),
                    catalogEntry,
                    rules);

                reEvaluatedItems.Add(reEvaluated with
                {
                    Id = item.Id,
                    CreatedAt = item.CreatedAt,
                    UpdatedAt = DateTimeOffset.UtcNow,
                    DescriptionSolidus = string.IsNullOrWhiteSpace(reEvaluated.DescriptionSolidus) ? item.DescriptionSolidus : reEvaluated.DescriptionSolidus,
                    Barcode = string.IsNullOrWhiteSpace(reEvaluated.Barcode) ? item.Barcode : reEvaluated.Barcode,
                    CodeType = string.IsNullOrWhiteSpace(reEvaluated.CodeType) ? item.CodeType : reEvaluated.CodeType
                });
            }

            changedAnyItem = !items.SequenceEqual(reEvaluatedItems, CampaignItemComparer.Instance);
            reEvaluatedItems = CampaignImportService.MarkDuplicateBarcodes(reEvaluatedItems);
            changedAnyItem = changedAnyItem || !items.SequenceEqual(reEvaluatedItems, CampaignItemComparer.Instance);
            if (changedAnyItem)
            {
                await repository.UpdateCampaignItemsAsync(reEvaluatedItems, cancellationToken);
            }
        }
    }

    private async Task<ProductCatalogEntry?> ResolveCatalogEntryAsync(CampaignItem item, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(item.Barcode))
        {
            var byBarcode = await repository.GetCatalogEntryByBarcodeAsync(item.Barcode, cancellationToken);
            if (byBarcode is not null)
            {
                return byBarcode;
            }
        }

        var matches = await repository.FindCatalogMatchesAsync(item.NormalizedDescriptionTabloid, cancellationToken);
        var exactMatch = matches.FirstOrDefault(match =>
            string.Equals(match.Barcode, item.Barcode, StringComparison.OrdinalIgnoreCase)
            || string.Equals(match.DescriptionSolidus, item.DescriptionSolidus, StringComparison.OrdinalIgnoreCase));
        if (exactMatch is not null)
        {
            return exactMatch;
        }

        if (string.IsNullOrWhiteSpace(item.Barcode) && string.IsNullOrWhiteSpace(item.DescriptionSolidus))
        {
            return null;
        }

        return new ProductCatalogEntry(
            Guid.Empty,
            item.DescriptionTabloid,
            item.NormalizedDescriptionTabloid,
            "",
            item.DescriptionSolidus,
            TextNormalizer.NormalizeKey(item.DescriptionSolidus),
            item.Barcode,
            item.CodeType,
            item.CreatedAt,
            item.UpdatedAt);
    }

    private sealed class CampaignItemComparer : IEqualityComparer<CampaignItem>
    {
        public static CampaignItemComparer Instance { get; } = new();

        public bool Equals(CampaignItem? x, CampaignItem? y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x is null || y is null)
            {
                return false;
            }

            return x.Id == y.Id
                && x.DescriptionTabloid == y.DescriptionTabloid
                && x.NormalizedDescriptionTabloid == y.NormalizedDescriptionTabloid
                && x.QuantityRaw == y.QuantityRaw
                && x.PriceSaleRaw == y.PriceSaleRaw
                && x.PriceClubRaw == y.PriceClubRaw
                && x.Quantity == y.Quantity
                && x.Unit == y.Unit
                && x.OriginalPriceSale == y.OriginalPriceSale
                && x.OriginalPriceClub == y.OriginalPriceClub
                && x.FinalPriceSale == y.FinalPriceSale
                && x.FinalPriceClub == y.FinalPriceClub
                && x.DescriptionSolidus == y.DescriptionSolidus
                && x.Barcode == y.Barcode
                && x.CodeType == y.CodeType
                && x.ReviewRequired == y.ReviewRequired
                && x.ReviewStatus == y.ReviewStatus
                && x.RiskFlags.SequenceEqual(y.RiskFlags)
                && x.BlockingReasons.SequenceEqual(y.BlockingReasons);
        }

        public int GetHashCode(CampaignItem item)
        {
            return item.Id.GetHashCode();
        }
    }

    private const string SchemaSql = """
CREATE TABLE IF NOT EXISTS users (
    id uuid PRIMARY KEY,
    email text NOT NULL UNIQUE,
    display_name text NOT NULL,
    password_hash text NOT NULL,
    role text NOT NULL,
    is_active boolean NOT NULL DEFAULT true,
    created_at timestamptz NOT NULL
);

CREATE TABLE IF NOT EXISTS product_catalog_entries (
    id uuid PRIMARY KEY,
    description_tabloid text NOT NULL,
    normalized_description_tabloid text NOT NULL,
    category text NOT NULL,
    description_solidus text NOT NULL,
    normalized_description_solidus text NOT NULL,
    barcode text NOT NULL,
    code_type text NOT NULL,
    created_at timestamptz NOT NULL,
    updated_at timestamptz NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_catalog_description_barcode
    ON product_catalog_entries (normalized_description_tabloid, barcode);
CREATE INDEX IF NOT EXISTS ix_catalog_description
    ON product_catalog_entries (normalized_description_tabloid);
CREATE INDEX IF NOT EXISTS ix_catalog_solidus
    ON product_catalog_entries (normalized_description_solidus);

CREATE TABLE IF NOT EXISTS conversion_rules (
    id uuid PRIMARY KEY,
    name text NOT NULL,
    rule_type text NOT NULL,
    pattern_input text NOT NULL DEFAULT '',
    pattern text NOT NULL,
    multiplier numeric(12,4) NOT NULL,
    target_unit text NOT NULL,
    category_scope text NOT NULL DEFAULT '',
    requires_review boolean NOT NULL,
    is_active boolean NOT NULL,
    created_at timestamptz NOT NULL,
    updated_at timestamptz NOT NULL
);

ALTER TABLE conversion_rules
    ADD COLUMN IF NOT EXISTS category_scope text NOT NULL DEFAULT '';

ALTER TABLE conversion_rules
    ADD COLUMN IF NOT EXISTS pattern_input text NOT NULL DEFAULT '';

UPDATE conversion_rules
SET pattern_input = pattern
WHERE COALESCE(pattern_input, '') = '';

CREATE TABLE IF NOT EXISTS campaigns (
    id uuid PRIMARY KEY,
    name text NOT NULL,
    valid_from date NOT NULL,
    valid_to date NOT NULL,
    status text NOT NULL,
    created_by uuid NOT NULL REFERENCES users(id),
    created_at timestamptz NOT NULL,
    updated_at timestamptz NOT NULL
);

CREATE TABLE IF NOT EXISTS import_batches (
    id uuid PRIMARY KEY,
    campaign_id uuid NOT NULL REFERENCES campaigns(id) ON DELETE CASCADE,
    original_file_name text NOT NULL,
    imported_by uuid NOT NULL REFERENCES users(id),
    imported_at timestamptz NOT NULL,
    row_count integer NOT NULL
);

CREATE TABLE IF NOT EXISTS campaign_items (
    id uuid PRIMARY KEY,
    campaign_id uuid NOT NULL REFERENCES campaigns(id) ON DELETE CASCADE,
    import_batch_id uuid NOT NULL REFERENCES import_batches(id) ON DELETE CASCADE,
    source_row integer NOT NULL,
    source text NOT NULL,
    original_vigency text NOT NULL,
    description_tabloid text NOT NULL,
    normalized_description_tabloid text NOT NULL,
    quantity_raw text NOT NULL,
    price_sale_raw text NOT NULL DEFAULT '',
    price_club_raw text NOT NULL DEFAULT '',
    quantity numeric(12,3) NOT NULL,
    unit text NOT NULL,
    original_price_sale numeric(12,2) NOT NULL,
    original_price_club numeric(12,2) NOT NULL,
    final_price_sale numeric(12,2) NOT NULL,
    final_price_club numeric(12,2) NOT NULL,
    description_solidus text NOT NULL,
    barcode text NOT NULL,
    code_type text NOT NULL,
    risk_flags text NOT NULL,
    blocking_reasons text NOT NULL,
    review_required boolean NOT NULL,
    review_status text NOT NULL,
    created_at timestamptz NOT NULL,
    updated_at timestamptz NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_campaign_items_campaign
    ON campaign_items (campaign_id);
CREATE INDEX IF NOT EXISTS ix_campaign_items_barcode
    ON campaign_items (barcode);

ALTER TABLE campaign_items
    ADD COLUMN IF NOT EXISTS price_sale_raw text NOT NULL DEFAULT '';

ALTER TABLE campaign_items
    ADD COLUMN IF NOT EXISTS price_club_raw text NOT NULL DEFAULT '';

CREATE TABLE IF NOT EXISTS review_decisions (
    id uuid PRIMARY KEY,
    campaign_id uuid NOT NULL REFERENCES campaigns(id) ON DELETE CASCADE,
    campaign_item_id uuid NOT NULL REFERENCES campaign_items(id) ON DELETE CASCADE,
    decided_by uuid NOT NULL REFERENCES users(id),
    decision text NOT NULL,
    comment text NOT NULL,
    created_at timestamptz NOT NULL
);

CREATE TABLE IF NOT EXISTS export_batches (
    id uuid PRIMARY KEY,
    campaign_id uuid NOT NULL REFERENCES campaigns(id) ON DELETE CASCADE,
    exported_by uuid NOT NULL REFERENCES users(id),
    exported_at timestamptz NOT NULL,
    file_name text NOT NULL,
    content_type text NOT NULL DEFAULT 'text/csv; charset=utf-8',
    storage_kind text NOT NULL DEFAULT 'text',
    content text NOT NULL,
    row_count integer NOT NULL
);

ALTER TABLE export_batches
    ADD COLUMN IF NOT EXISTS content_type text NOT NULL DEFAULT 'text/csv; charset=utf-8';

ALTER TABLE export_batches
    ADD COLUMN IF NOT EXISTS storage_kind text NOT NULL DEFAULT 'text';

CREATE TABLE IF NOT EXISTS audit_logs (
    id uuid PRIMARY KEY,
    actor_user_id uuid NULL REFERENCES users(id),
    actor_email text NOT NULL,
    action text NOT NULL,
    entity_type text NOT NULL,
    entity_id uuid NULL,
    details text NOT NULL,
    created_at timestamptz NOT NULL
);
""";
}
