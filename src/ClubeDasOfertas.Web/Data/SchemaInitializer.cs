using ClubeDasOfertas.Web.Domain;
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
            "Pesaveis e cada 100g",
            RuleTypes.Weighted,
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
    content text NOT NULL,
    row_count integer NOT NULL
);

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
