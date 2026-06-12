using ClubeDasOfertas.Web.Domain;
using ClubeDasOfertas.Web.Services;
using Npgsql;

namespace ClubeDasOfertas.Web.Data;

public sealed class AppRepository(AppDb db)
{
    public async Task<bool> HasUsersAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await db.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand("SELECT EXISTS (SELECT 1 FROM users);", connection);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is true;
    }

    public async Task<UserAccount?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        await using var connection = await db.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand("""
SELECT id, email, display_name, password_hash, role, is_active, created_at
FROM users
WHERE lower(email) = lower(@email)
LIMIT 1;
""", connection);
        command.Parameters.AddWithValue("@email", email.Trim());

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadUser(reader) : null;
    }

    public async Task<UserAccount?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = await db.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand("""
SELECT id, email, display_name, password_hash, role, is_active, created_at
FROM users
WHERE id = @id
LIMIT 1;
""", connection);
        command.Parameters.AddWithValue("@id", id);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadUser(reader) : null;
    }

    public async Task EnsureUserAsync(string email, string displayName, string role, string password, CancellationToken cancellationToken = default)
    {
        if (await GetUserByEmailAsync(email, cancellationToken) is not null)
        {
            return;
        }

        await CreateUserAsync(email, displayName, role, password, cancellationToken);
    }

    public async Task<UserAccount> CreateUserAsync(string email, string displayName, string role, string password, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var user = new UserAccount(
            Guid.NewGuid(),
            email.Trim(),
            displayName.Trim(),
            PasswordHasher.Hash(password),
            role,
            true,
            now);

        await using var connection = await db.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand("""
INSERT INTO users (id, email, display_name, password_hash, role, is_active, created_at)
VALUES (@id, @email, @display_name, @password_hash, @role, true, @created_at)
ON CONFLICT (email) DO NOTHING;
""", connection);
        command.Parameters.AddWithValue("@id", user.Id);
        command.Parameters.AddWithValue("@email", user.Email);
        command.Parameters.AddWithValue("@display_name", user.DisplayName);
        command.Parameters.AddWithValue("@password_hash", user.PasswordHash);
        command.Parameters.AddWithValue("@role", user.Role);
        command.Parameters.AddWithValue("@created_at", user.CreatedAt);
        await command.ExecuteNonQueryAsync(cancellationToken);

        return user;
    }

    public async Task<IReadOnlyList<Campaign>> ListCampaignsAsync(CancellationToken cancellationToken = default)
    {
        var campaigns = new List<Campaign>();
        await using var connection = await db.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand("""
SELECT id, name, valid_from, valid_to, status, created_by, created_at, updated_at
FROM campaigns
ORDER BY created_at DESC;
""", connection);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            campaigns.Add(ReadCampaign(reader));
        }

        return campaigns;
    }

    public async Task<Campaign?> GetCampaignAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = await db.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand("""
SELECT id, name, valid_from, valid_to, status, created_by, created_at, updated_at
FROM campaigns
WHERE id = @id;
""", connection);
        command.Parameters.AddWithValue("@id", id);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadCampaign(reader) : null;
    }

    public async Task<Campaign> CreateCampaignAsync(string name, DateOnly validFrom, DateOnly validTo, Guid createdBy, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var campaign = new Campaign(Guid.NewGuid(), name.Trim(), validFrom, validTo, CampaignStatus.Draft, createdBy, now, now);

        await using var connection = await db.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand("""
INSERT INTO campaigns (id, name, valid_from, valid_to, status, created_by, created_at, updated_at)
VALUES (@id, @name, @valid_from, @valid_to, @status, @created_by, @created_at, @updated_at);
""", connection);
        command.Parameters.AddWithValue("@id", campaign.Id);
        command.Parameters.AddWithValue("@name", campaign.Name);
        command.Parameters.AddWithValue("@valid_from", campaign.ValidFrom);
        command.Parameters.AddWithValue("@valid_to", campaign.ValidTo);
        command.Parameters.AddWithValue("@status", campaign.Status);
        command.Parameters.AddWithValue("@created_by", campaign.CreatedBy);
        command.Parameters.AddWithValue("@created_at", campaign.CreatedAt);
        command.Parameters.AddWithValue("@updated_at", campaign.UpdatedAt);
        await command.ExecuteNonQueryAsync(cancellationToken);

        return campaign;
    }

    public async Task UpdateCampaignStatusAsync(Guid campaignId, string status, CancellationToken cancellationToken = default)
    {
        await using var connection = await db.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand("""
UPDATE campaigns
SET status = @status, updated_at = @updated_at
WHERE id = @id;
""", connection);
        command.Parameters.AddWithValue("@id", campaignId);
        command.Parameters.AddWithValue("@status", status);
        command.Parameters.AddWithValue("@updated_at", DateTimeOffset.UtcNow);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteCampaignAsync(Guid campaignId, CancellationToken cancellationToken = default)
    {
        await using var connection = await db.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand("""
DELETE FROM campaigns
WHERE id = @id;
""", connection);
        command.Parameters.AddWithValue("@id", campaignId);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ProductCatalogEntry>> SearchCatalogAsync(string query, string category = "", CancellationToken cancellationToken = default)
    {
        var entries = new List<ProductCatalogEntry>();
        var normalized = TextNormalizer.NormalizeKey(query);
        var normalizedCategory = TextNormalizer.IsAllCatalogItemsFilter(category)
            ? string.Empty
            : TextNormalizer.NormalizeCatalogCategoryKey(category);

        await using var connection = await db.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand("""
SELECT id, description_tabloid, normalized_description_tabloid, category, description_solidus,
       normalized_description_solidus, barcode, code_type, created_at, updated_at
FROM product_catalog_entries
WHERE (
    @query = ''
    OR normalized_description_tabloid LIKE '%' || @query || '%'
    OR normalized_description_solidus LIKE '%' || @query || '%'
    OR barcode LIKE '%' || @raw || '%'
  )
ORDER BY description_tabloid, description_solidus
""", connection);
        command.Parameters.AddWithValue("@query", normalized);
        command.Parameters.AddWithValue("@raw", query.Trim());

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var entry = ReadCatalog(reader);
            if (string.IsNullOrWhiteSpace(normalizedCategory) || TextNormalizer.NormalizeCatalogCategoryKey(entry.Category) == normalizedCategory)
            {
                entries.Add(entry);
            }
        }

        return entries;
    }

    public async Task<IReadOnlyList<(string Category, int Count)>> ListCatalogCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var categories = new List<(string Category, int Count)>();
        await using var connection = await db.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand("""
SELECT COALESCE(NULLIF(TRIM(category), ''), 'Sem categoria') AS category_name, COUNT(*)
FROM product_catalog_entries
GROUP BY category_name
ORDER BY COUNT(*) DESC, category_name;
""", connection);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            categories.Add((reader.GetString(0), reader.GetInt32(1)));
        }

        return categories;
    }

    public async Task<IReadOnlyList<ProductCatalogEntry>> FindCatalogMatchesAsync(string normalizedDescription, CancellationToken cancellationToken = default)
    {
        var entries = new List<ProductCatalogEntry>();
        await using var connection = await db.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand("""
SELECT id, description_tabloid, normalized_description_tabloid, category, description_solidus,
       normalized_description_solidus, barcode, code_type, created_at, updated_at
FROM product_catalog_entries
WHERE normalized_description_tabloid = @description
ORDER BY description_solidus;
""", connection);
        command.Parameters.AddWithValue("@description", normalizedDescription);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            entries.Add(ReadCatalog(reader));
        }

        return entries;
    }

    public async Task<ProductCatalogEntry?> GetCatalogEntryByBarcodeAsync(string barcode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(barcode))
        {
            return null;
        }

        await using var connection = await db.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand("""
SELECT id, description_tabloid, normalized_description_tabloid, category, description_solidus,
       normalized_description_solidus, barcode, code_type, created_at, updated_at
FROM product_catalog_entries
WHERE barcode = @barcode
ORDER BY updated_at DESC, created_at DESC
LIMIT 1;
""", connection);
        command.Parameters.AddWithValue("@barcode", barcode.Trim());

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadCatalog(reader) : null;
    }

    public async Task<int> UpsertCatalogAsync(IReadOnlyList<CatalogImportRow> rows, CancellationToken cancellationToken = default)
    {
        var count = 0;
        await using var connection = await db.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        foreach (var row in rows)
        {
            if (string.IsNullOrWhiteSpace(row.DescriptionTabloid) || string.IsNullOrWhiteSpace(row.Barcode))
            {
                continue;
            }

            await using var command = new NpgsqlCommand("""
INSERT INTO product_catalog_entries (
    id, description_tabloid, normalized_description_tabloid, category, description_solidus,
    normalized_description_solidus, barcode, code_type, created_at, updated_at)
VALUES (
    @id, @description_tabloid, @normalized_description_tabloid, @category, @description_solidus,
    @normalized_description_solidus, @barcode, @code_type, @created_at, @updated_at)
ON CONFLICT (normalized_description_tabloid, barcode)
DO UPDATE SET
    description_tabloid = EXCLUDED.description_tabloid,
    category = EXCLUDED.category,
    description_solidus = EXCLUDED.description_solidus,
    normalized_description_solidus = EXCLUDED.normalized_description_solidus,
    code_type = EXCLUDED.code_type,
    updated_at = EXCLUDED.updated_at;
""", connection, transaction);
            command.Parameters.AddWithValue("@id", Guid.NewGuid());
            command.Parameters.AddWithValue("@description_tabloid", row.DescriptionTabloid.Trim());
            command.Parameters.AddWithValue("@normalized_description_tabloid", TextNormalizer.NormalizeKey(row.DescriptionTabloid));
            command.Parameters.AddWithValue("@category", row.Category.Trim());
            command.Parameters.AddWithValue("@description_solidus", row.DescriptionSolidus.Trim());
            command.Parameters.AddWithValue("@normalized_description_solidus", TextNormalizer.NormalizeKey(row.DescriptionSolidus));
            command.Parameters.AddWithValue("@barcode", row.Barcode.Trim());
            command.Parameters.AddWithValue("@code_type", Parsing.CodeType(row.Barcode));
            command.Parameters.AddWithValue("@created_at", DateTimeOffset.UtcNow);
            command.Parameters.AddWithValue("@updated_at", DateTimeOffset.UtcNow);

            await command.ExecuteNonQueryAsync(cancellationToken);
            count++;
        }

        await transaction.CommitAsync(cancellationToken);
        return count;
    }

    public async Task<IReadOnlyList<ConversionRule>> ListRulesAsync(CancellationToken cancellationToken = default)
    {
        var rules = new List<ConversionRule>();
        await using var connection = await db.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand("""
SELECT id, name, rule_type, pattern_input, pattern, multiplier, target_unit, category_scope, requires_review, is_active, created_at, updated_at
FROM conversion_rules
ORDER BY rule_type, name;
""", connection);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            rules.Add(ReadRule(reader));
        }

        return rules;
    }

    public async Task<ConversionRule?> GetRuleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = await db.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand("""
SELECT id, name, rule_type, pattern_input, pattern, multiplier, target_unit, category_scope, requires_review, is_active, created_at, updated_at
FROM conversion_rules
WHERE id = @id
LIMIT 1;
""", connection);
        command.Parameters.AddWithValue("@id", id);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            return ReadRule(reader);
        }

        return null;
    }

    public async Task AddRuleAsync(ConversionRule rule, CancellationToken cancellationToken = default)
    {
        await using var connection = await db.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand("""
INSERT INTO conversion_rules
    (id, name, rule_type, pattern_input, pattern, multiplier, target_unit, category_scope, requires_review, is_active, created_at, updated_at)
VALUES
    (@id, @name, @rule_type, @pattern_input, @pattern, @multiplier, @target_unit, @category_scope, @requires_review, @is_active, @created_at, @updated_at);
""", connection);
        command.Parameters.AddWithValue("@id", rule.Id);
        command.Parameters.AddWithValue("@name", rule.Name);
        command.Parameters.AddWithValue("@rule_type", rule.RuleType);
        command.Parameters.AddWithValue("@pattern_input", rule.PatternInput);
        command.Parameters.AddWithValue("@pattern", rule.Pattern);
        command.Parameters.AddWithValue("@multiplier", rule.Multiplier);
        command.Parameters.AddWithValue("@target_unit", rule.TargetUnit);
        command.Parameters.AddWithValue("@category_scope", rule.CategoryScope);
        command.Parameters.AddWithValue("@requires_review", rule.RequiresReview);
        command.Parameters.AddWithValue("@is_active", rule.IsActive);
        command.Parameters.AddWithValue("@created_at", rule.CreatedAt);
        command.Parameters.AddWithValue("@updated_at", rule.UpdatedAt);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpdateRuleAsync(ConversionRule rule, CancellationToken cancellationToken = default)
    {
        await using var connection = await db.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand("""
UPDATE conversion_rules
SET name = @name,
    rule_type = @rule_type,
    pattern_input = @pattern_input,
    pattern = @pattern,
    multiplier = @multiplier,
    target_unit = @target_unit,
    category_scope = @category_scope,
    requires_review = @requires_review,
    is_active = @is_active,
    updated_at = @updated_at
WHERE id = @id;
""", connection);
        command.Parameters.AddWithValue("@id", rule.Id);
        command.Parameters.AddWithValue("@name", rule.Name);
        command.Parameters.AddWithValue("@rule_type", rule.RuleType);
        command.Parameters.AddWithValue("@pattern_input", rule.PatternInput);
        command.Parameters.AddWithValue("@pattern", rule.Pattern);
        command.Parameters.AddWithValue("@multiplier", rule.Multiplier);
        command.Parameters.AddWithValue("@target_unit", rule.TargetUnit);
        command.Parameters.AddWithValue("@category_scope", rule.CategoryScope);
        command.Parameters.AddWithValue("@requires_review", rule.RequiresReview);
        command.Parameters.AddWithValue("@is_active", rule.IsActive);
        command.Parameters.AddWithValue("@updated_at", rule.UpdatedAt);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task SetRuleActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken = default)
    {
        await using var connection = await db.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand("""
UPDATE conversion_rules
SET is_active = @is_active, updated_at = @updated_at
WHERE id = @id;
""", connection);
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@is_active", isActive);
        command.Parameters.AddWithValue("@updated_at", DateTimeOffset.UtcNow);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task ToggleRuleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = await db.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand("""
UPDATE conversion_rules
SET is_active = NOT is_active, updated_at = @updated_at
WHERE id = @id;
""", connection);
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@updated_at", DateTimeOffset.UtcNow);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteRuleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = await db.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand("""
DELETE FROM conversion_rules
WHERE id = @id;
""", connection);
        command.Parameters.AddWithValue("@id", id);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task ReplaceCampaignItemsAsync(ImportBatch batch, IReadOnlyList<CampaignItem> items, CancellationToken cancellationToken = default)
    {
        await using var connection = await db.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        await using (var delete = new NpgsqlCommand("""
DELETE FROM campaign_items WHERE campaign_id = @campaign_id;
DELETE FROM import_batches WHERE campaign_id = @campaign_id;
""", connection, transaction))
        {
            delete.Parameters.AddWithValue("@campaign_id", batch.CampaignId);
            await delete.ExecuteNonQueryAsync(cancellationToken);
        }

        await using (var insertBatch = new NpgsqlCommand("""
INSERT INTO import_batches (id, campaign_id, original_file_name, imported_by, imported_at, row_count)
VALUES (@id, @campaign_id, @original_file_name, @imported_by, @imported_at, @row_count);
""", connection, transaction))
        {
            insertBatch.Parameters.AddWithValue("@id", batch.Id);
            insertBatch.Parameters.AddWithValue("@campaign_id", batch.CampaignId);
            insertBatch.Parameters.AddWithValue("@original_file_name", batch.OriginalFileName);
            insertBatch.Parameters.AddWithValue("@imported_by", batch.ImportedBy);
            insertBatch.Parameters.AddWithValue("@imported_at", batch.ImportedAt);
            insertBatch.Parameters.AddWithValue("@row_count", batch.RowCount);
            await insertBatch.ExecuteNonQueryAsync(cancellationToken);
        }

        foreach (var item in items)
        {
            await using var insert = new NpgsqlCommand("""
INSERT INTO campaign_items (
    id, campaign_id, import_batch_id, source_row, source, original_vigency, description_tabloid,
    normalized_description_tabloid, quantity_raw, price_sale_raw, price_club_raw, quantity, unit,
    original_price_sale, original_price_club, final_price_sale, final_price_club, description_solidus,
    barcode, code_type, risk_flags, blocking_reasons, review_required, review_status, created_at, updated_at)
VALUES (
    @id, @campaign_id, @import_batch_id, @source_row, @source, @original_vigency, @description_tabloid,
    @normalized_description_tabloid, @quantity_raw, @price_sale_raw, @price_club_raw, @quantity, @unit,
    @original_price_sale, @original_price_club, @final_price_sale, @final_price_club, @description_solidus,
    @barcode, @code_type, @risk_flags, @blocking_reasons, @review_required, @review_status, @created_at, @updated_at);
""", connection, transaction);
            AddCampaignItemParameters(insert, item);
            await insert.ExecuteNonQueryAsync(cancellationToken);
        }

        await using (var updateCampaign = new NpgsqlCommand("""
UPDATE campaigns
SET status = @status, updated_at = @updated_at
WHERE id = @campaign_id;
""", connection, transaction))
        {
            updateCampaign.Parameters.AddWithValue("@campaign_id", batch.CampaignId);
            updateCampaign.Parameters.AddWithValue("@status", CampaignStatus.Imported);
            updateCampaign.Parameters.AddWithValue("@updated_at", DateTimeOffset.UtcNow);
            await updateCampaign.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CampaignItem>> GetCampaignItemsAsync(Guid campaignId, CancellationToken cancellationToken = default)
    {
        var items = new List<CampaignItem>();
        await using var connection = await db.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(ItemSelectSql + " WHERE campaign_id = @campaign_id ORDER BY source_row, description_solidus, barcode;", connection);
        command.Parameters.AddWithValue("@campaign_id", campaignId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(ReadCampaignItem(reader));
        }

        return items;
    }

    public async Task<CampaignItem?> GetCampaignItemAsync(Guid itemId, CancellationToken cancellationToken = default)
    {
        await using var connection = await db.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(ItemSelectSql + " WHERE id = @id LIMIT 1;", connection);
        command.Parameters.AddWithValue("@id", itemId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadCampaignItem(reader) : null;
    }

    public async Task<CampaignStats> GetCampaignStatsAsync(Guid campaignId, CancellationToken cancellationToken = default)
    {
        var items = await GetCampaignItemsAsync(campaignId, cancellationToken);
        return new CampaignStats(
            items.Count,
            items.Count(x => x.BlockingReasons.Count > 0),
            items.Count(x => x.ReviewStatus == ReviewStatus.Pending),
            items.Count(x => string.IsNullOrWhiteSpace(x.Barcode)),
            items.Count(x => x.RiskFlags.Contains("PESAVEL")),
            items.Count(x => x.RiskFlags.Contains("FARDO_CAIXA")));
    }

    public async Task UpdateItemReviewAsync(Guid itemId, bool reviewRequired, string reviewStatus, IReadOnlyList<string> blockingReasons, CancellationToken cancellationToken = default)
    {
        await using var connection = await db.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand("""
UPDATE campaign_items
SET review_required = @review_required,
    review_status = @review_status,
    blocking_reasons = @blocking_reasons,
    updated_at = @updated_at
WHERE id = @id;
""", connection);
        command.Parameters.AddWithValue("@id", itemId);
        command.Parameters.AddWithValue("@review_required", reviewRequired);
        command.Parameters.AddWithValue("@review_status", reviewStatus);
        command.Parameters.AddWithValue("@blocking_reasons", Pack(blockingReasons));
        command.Parameters.AddWithValue("@updated_at", DateTimeOffset.UtcNow);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpdateCampaignItemsAsync(IReadOnlyList<CampaignItem> items, CancellationToken cancellationToken = default)
    {
        if (items.Count == 0)
        {
            return;
        }

        await using var connection = await db.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        await UpdateCampaignItemsInTransactionAsync(items, connection, transaction, cancellationToken);

        await using (var updateCampaign = new NpgsqlCommand("""
UPDATE campaigns
SET updated_at = @updated_at
WHERE id = @campaign_id;
""", connection, transaction))
        {
            updateCampaign.Parameters.AddWithValue("@campaign_id", items[0].CampaignId);
            updateCampaign.Parameters.AddWithValue("@updated_at", DateTimeOffset.UtcNow);
            await updateCampaign.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task AddManualCampaignItemAsync(ImportBatch batch, CampaignItem item, IReadOnlyList<CampaignItem> itemsToUpdate, CancellationToken cancellationToken = default)
    {
        await using var connection = await db.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        await using (var insertBatch = new NpgsqlCommand("""
INSERT INTO import_batches (id, campaign_id, original_file_name, imported_by, imported_at, row_count)
VALUES (@id, @campaign_id, @original_file_name, @imported_by, @imported_at, @row_count);
""", connection, transaction))
        {
            insertBatch.Parameters.AddWithValue("@id", batch.Id);
            insertBatch.Parameters.AddWithValue("@campaign_id", batch.CampaignId);
            insertBatch.Parameters.AddWithValue("@original_file_name", batch.OriginalFileName);
            insertBatch.Parameters.AddWithValue("@imported_by", batch.ImportedBy);
            insertBatch.Parameters.AddWithValue("@imported_at", batch.ImportedAt);
            insertBatch.Parameters.AddWithValue("@row_count", batch.RowCount);
            await insertBatch.ExecuteNonQueryAsync(cancellationToken);
        }

        await UpdateCampaignItemsInTransactionAsync(itemsToUpdate, connection, transaction, cancellationToken);

        await using (var insertItem = new NpgsqlCommand("""
INSERT INTO campaign_items (
    id, campaign_id, import_batch_id, source_row, source, original_vigency, description_tabloid,
    normalized_description_tabloid, quantity_raw, price_sale_raw, price_club_raw, quantity, unit,
    original_price_sale, original_price_club, final_price_sale, final_price_club, description_solidus,
    barcode, code_type, risk_flags, blocking_reasons, review_required, review_status, created_at, updated_at)
VALUES (
    @id, @campaign_id, @import_batch_id, @source_row, @source, @original_vigency, @description_tabloid,
    @normalized_description_tabloid, @quantity_raw, @price_sale_raw, @price_club_raw, @quantity, @unit,
    @original_price_sale, @original_price_club, @final_price_sale, @final_price_club, @description_solidus,
    @barcode, @code_type, @risk_flags, @blocking_reasons, @review_required, @review_status, @created_at, @updated_at);
""", connection, transaction))
        {
            AddCampaignItemParameters(insertItem, item);
            await insertItem.ExecuteNonQueryAsync(cancellationToken);
        }

        await using (var updateCampaign = new NpgsqlCommand("""
UPDATE campaigns
SET status = @status, updated_at = @updated_at
WHERE id = @campaign_id;
""", connection, transaction))
        {
            updateCampaign.Parameters.AddWithValue("@campaign_id", batch.CampaignId);
            updateCampaign.Parameters.AddWithValue("@status", CampaignStatus.Imported);
            updateCampaign.Parameters.AddWithValue("@updated_at", DateTimeOffset.UtcNow);
            await updateCampaign.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task AddReviewDecisionAsync(ReviewDecision decision, CancellationToken cancellationToken = default)
    {
        await using var connection = await db.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand("""
INSERT INTO review_decisions (id, campaign_id, campaign_item_id, decided_by, decision, comment, created_at)
VALUES (@id, @campaign_id, @campaign_item_id, @decided_by, @decision, @comment, @created_at);
""", connection);
        command.Parameters.AddWithValue("@id", decision.Id);
        command.Parameters.AddWithValue("@campaign_id", decision.CampaignId);
        command.Parameters.AddWithValue("@campaign_item_id", decision.CampaignItemId);
        command.Parameters.AddWithValue("@decided_by", decision.DecidedBy);
        command.Parameters.AddWithValue("@decision", decision.Decision);
        command.Parameters.AddWithValue("@comment", decision.Comment);
        command.Parameters.AddWithValue("@created_at", decision.CreatedAt);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task AddAuditAsync(Guid? actorUserId, string actorEmail, string action, string entityType, Guid? entityId, string details, CancellationToken cancellationToken = default)
    {
        await using var connection = await db.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand("""
INSERT INTO audit_logs (id, actor_user_id, actor_email, action, entity_type, entity_id, details, created_at)
VALUES (@id, @actor_user_id, @actor_email, @action, @entity_type, @entity_id, @details, @created_at);
""", connection);
        command.Parameters.AddWithValue("@id", Guid.NewGuid());
        command.Parameters.AddWithValue("@actor_user_id", actorUserId.HasValue ? actorUserId.Value : DBNull.Value);
        command.Parameters.AddWithValue("@actor_email", actorEmail);
        command.Parameters.AddWithValue("@action", action);
        command.Parameters.AddWithValue("@entity_type", entityType);
        command.Parameters.AddWithValue("@entity_id", entityId.HasValue ? entityId.Value : DBNull.Value);
        command.Parameters.AddWithValue("@details", details);
        command.Parameters.AddWithValue("@created_at", DateTimeOffset.UtcNow);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLog>> ListAuditLogsAsync(CancellationToken cancellationToken = default)
    {
        var logs = new List<AuditLog>();
        await using var connection = await db.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand("""
SELECT id, actor_user_id, actor_email, action, entity_type, entity_id, details, created_at
FROM audit_logs
ORDER BY created_at DESC
LIMIT 250;
""", connection);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            logs.Add(new AuditLog(
                reader.GetGuid(0),
                reader.IsDBNull(1) ? null : reader.GetGuid(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetString(4),
                reader.IsDBNull(5) ? null : reader.GetGuid(5),
                reader.GetString(6),
                reader.GetFieldValue<DateTimeOffset>(7)));
        }

        return logs;
    }

    public async Task SaveExportAsync(ExportBatch export, CancellationToken cancellationToken = default)
    {
        await using var connection = await db.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        await using (var command = new NpgsqlCommand("""
INSERT INTO export_batches (id, campaign_id, exported_by, exported_at, file_name, content, row_count)
VALUES (@id, @campaign_id, @exported_by, @exported_at, @file_name, @content, @row_count);
""", connection, transaction))
        {
            command.Parameters.AddWithValue("@id", export.Id);
            command.Parameters.AddWithValue("@campaign_id", export.CampaignId);
            command.Parameters.AddWithValue("@exported_by", export.ExportedBy);
            command.Parameters.AddWithValue("@exported_at", export.ExportedAt);
            command.Parameters.AddWithValue("@file_name", export.FileName);
            command.Parameters.AddWithValue("@content", export.Content);
            command.Parameters.AddWithValue("@row_count", export.RowCount);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        await using (var updateCampaign = new NpgsqlCommand("""
UPDATE campaigns
SET status = @status, updated_at = @updated_at
WHERE id = @campaign_id;
""", connection, transaction))
        {
            updateCampaign.Parameters.AddWithValue("@campaign_id", export.CampaignId);
            updateCampaign.Parameters.AddWithValue("@status", CampaignStatus.Exported);
            updateCampaign.Parameters.AddWithValue("@updated_at", DateTimeOffset.UtcNow);
            await updateCampaign.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<ExportBatch?> GetExportAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = await db.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand("""
SELECT id, campaign_id, exported_by, exported_at, file_name, content, row_count
FROM export_batches
WHERE id = @id;
""", connection);
        command.Parameters.AddWithValue("@id", id);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken)
            ? new ExportBatch(reader.GetGuid(0), reader.GetGuid(1), reader.GetGuid(2), reader.GetFieldValue<DateTimeOffset>(3), reader.GetString(4), reader.GetString(5), reader.GetInt32(6))
            : null;
    }

    public async Task<IReadOnlyList<ExportBatch>> ListExportsAsync(CancellationToken cancellationToken = default)
    {
        var exports = new List<ExportBatch>();
        await using var connection = await db.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand("""
SELECT id, campaign_id, exported_by, exported_at, file_name, content, row_count
FROM export_batches
ORDER BY exported_at DESC
LIMIT 100;
""", connection);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            exports.Add(new ExportBatch(reader.GetGuid(0), reader.GetGuid(1), reader.GetGuid(2), reader.GetFieldValue<DateTimeOffset>(3), reader.GetString(4), reader.GetString(5), reader.GetInt32(6)));
        }

        return exports;
    }

    private static UserAccount ReadUser(NpgsqlDataReader reader)
    {
        return new UserAccount(
            reader.GetGuid(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetString(3),
            reader.GetString(4),
            reader.GetBoolean(5),
            reader.GetFieldValue<DateTimeOffset>(6));
    }

    private static Campaign ReadCampaign(NpgsqlDataReader reader)
    {
        return new Campaign(
            reader.GetGuid(0),
            reader.GetString(1),
            reader.GetFieldValue<DateOnly>(2),
            reader.GetFieldValue<DateOnly>(3),
            reader.GetString(4),
            reader.GetGuid(5),
            reader.GetFieldValue<DateTimeOffset>(6),
            reader.GetFieldValue<DateTimeOffset>(7));
    }

    private static ProductCatalogEntry ReadCatalog(NpgsqlDataReader reader)
    {
        return new ProductCatalogEntry(
            reader.GetGuid(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetString(3),
            reader.GetString(4),
            reader.GetString(5),
            reader.GetString(6),
            reader.GetString(7),
            reader.GetFieldValue<DateTimeOffset>(8),
            reader.GetFieldValue<DateTimeOffset>(9));
    }

    private static ConversionRule ReadRule(NpgsqlDataReader reader)
    {
        return new ConversionRule(
            reader.GetGuid(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetString(3),
            reader.GetString(4),
            reader.GetDecimal(5),
            reader.GetString(6),
            reader.GetString(7),
            reader.GetBoolean(8),
            reader.GetBoolean(9),
            reader.GetFieldValue<DateTimeOffset>(10),
            reader.GetFieldValue<DateTimeOffset>(11));
    }

    private static CampaignItem ReadCampaignItem(NpgsqlDataReader reader)
    {
        return new CampaignItem(
            reader.GetGuid(0),
            reader.GetGuid(1),
            reader.GetGuid(2),
            reader.GetInt32(3),
            reader.GetString(4),
            reader.GetString(5),
            reader.GetString(6),
            reader.GetString(7),
            reader.GetString(8),
            reader.GetString(9),
            reader.GetString(10),
            reader.GetDecimal(11),
            reader.GetString(12),
            reader.GetDecimal(13),
            reader.GetDecimal(14),
            reader.GetDecimal(15),
            reader.GetDecimal(16),
            reader.GetString(17),
            reader.GetString(18),
            reader.GetString(19),
            Unpack(reader.GetString(20)),
            Unpack(reader.GetString(21)),
            reader.GetBoolean(22),
            reader.GetString(23),
            reader.GetFieldValue<DateTimeOffset>(24),
            reader.GetFieldValue<DateTimeOffset>(25));
    }

    private static void AddCampaignItemParameters(NpgsqlCommand command, CampaignItem item)
    {
        command.Parameters.AddWithValue("@id", item.Id);
        command.Parameters.AddWithValue("@campaign_id", item.CampaignId);
        command.Parameters.AddWithValue("@import_batch_id", item.ImportBatchId);
        command.Parameters.AddWithValue("@source_row", item.SourceRow);
        command.Parameters.AddWithValue("@source", item.Source);
        command.Parameters.AddWithValue("@original_vigency", item.OriginalVigency);
        command.Parameters.AddWithValue("@description_tabloid", item.DescriptionTabloid);
        command.Parameters.AddWithValue("@normalized_description_tabloid", item.NormalizedDescriptionTabloid);
        command.Parameters.AddWithValue("@quantity_raw", item.QuantityRaw);
        command.Parameters.AddWithValue("@price_sale_raw", item.PriceSaleRaw);
        command.Parameters.AddWithValue("@price_club_raw", item.PriceClubRaw);
        command.Parameters.AddWithValue("@quantity", item.Quantity);
        command.Parameters.AddWithValue("@unit", item.Unit);
        command.Parameters.AddWithValue("@original_price_sale", item.OriginalPriceSale);
        command.Parameters.AddWithValue("@original_price_club", item.OriginalPriceClub);
        command.Parameters.AddWithValue("@final_price_sale", item.FinalPriceSale);
        command.Parameters.AddWithValue("@final_price_club", item.FinalPriceClub);
        command.Parameters.AddWithValue("@description_solidus", item.DescriptionSolidus);
        command.Parameters.AddWithValue("@barcode", item.Barcode);
        command.Parameters.AddWithValue("@code_type", item.CodeType);
        command.Parameters.AddWithValue("@risk_flags", Pack(item.RiskFlags));
        command.Parameters.AddWithValue("@blocking_reasons", Pack(item.BlockingReasons));
        command.Parameters.AddWithValue("@review_required", item.ReviewRequired);
        command.Parameters.AddWithValue("@review_status", item.ReviewStatus);
        command.Parameters.AddWithValue("@created_at", item.CreatedAt);
        command.Parameters.AddWithValue("@updated_at", item.UpdatedAt);
    }

    private static async Task UpdateCampaignItemsInTransactionAsync(IReadOnlyList<CampaignItem> items, NpgsqlConnection connection, NpgsqlTransaction transaction, CancellationToken cancellationToken)
    {
        foreach (var item in items)
        {
            await using var command = new NpgsqlCommand("""
UPDATE campaign_items
SET source_row = @source_row,
    source = @source,
    original_vigency = @original_vigency,
    description_tabloid = @description_tabloid,
    normalized_description_tabloid = @normalized_description_tabloid,
    quantity_raw = @quantity_raw,
    price_sale_raw = @price_sale_raw,
    price_club_raw = @price_club_raw,
    quantity = @quantity,
    unit = @unit,
    original_price_sale = @original_price_sale,
    original_price_club = @original_price_club,
    final_price_sale = @final_price_sale,
    final_price_club = @final_price_club,
    description_solidus = @description_solidus,
    barcode = @barcode,
    code_type = @code_type,
    risk_flags = @risk_flags,
    blocking_reasons = @blocking_reasons,
    review_required = @review_required,
    review_status = @review_status,
    updated_at = @updated_at
WHERE id = @id;
""", connection, transaction);
            AddCampaignItemParameters(command, item);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static string Pack(IReadOnlyList<string> values)
    {
        return string.Join("|", values.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Replace("|", "/", StringComparison.Ordinal)));
    }

    private static IReadOnlyList<string> Unpack(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? Array.Empty<string>()
            : value.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private const string ItemSelectSql = """
SELECT id, campaign_id, import_batch_id, source_row, source, original_vigency, description_tabloid,
       normalized_description_tabloid, quantity_raw, price_sale_raw, price_club_raw, quantity, unit,
       original_price_sale, original_price_club, final_price_sale, final_price_club, description_solidus,
       barcode, code_type, risk_flags, blocking_reasons, review_required, review_status, created_at, updated_at
FROM campaign_items
""";
}
