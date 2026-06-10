namespace ClubeDasOfertas.Web.Domain;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Operator = "Operador";
}

public static class CampaignStatus
{
    public const string Draft = "Rascunho";
    public const string Imported = "Importado";
    public const string Exported = "Exportado";
}

public static class RuleTypes
{
    public const string Weighted = "Pesavel";
    public const string Package = "FardoCaixa";
    public const string PackageBale = "Fardo";
    public const string PackageBox = "Caixa";
}

public static class ReviewStatus
{
    public const string NotRequired = "NaoRequer";
    public const string Pending = "Pendente";
    public const string Approved = "Aprovado";
    public const string Rejected = "Rejeitado";
}

public sealed record UserAccount(
    Guid Id,
    string Email,
    string DisplayName,
    string PasswordHash,
    string Role,
    bool IsActive,
    DateTimeOffset CreatedAt);

public sealed record ProductCatalogEntry(
    Guid Id,
    string DescriptionTabloid,
    string NormalizedDescriptionTabloid,
    string Category,
    string DescriptionSolidus,
    string NormalizedDescriptionSolidus,
    string Barcode,
    string CodeType,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record ConversionRule(
    Guid Id,
    string Name,
    string RuleType,
    string PatternInput,
    string Pattern,
    decimal Multiplier,
    string TargetUnit,
    string CategoryScope,
    bool RequiresReview,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record Campaign(
    Guid Id,
    string Name,
    DateOnly ValidFrom,
    DateOnly ValidTo,
    string Status,
    Guid CreatedBy,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record ImportBatch(
    Guid Id,
    Guid CampaignId,
    string OriginalFileName,
    Guid ImportedBy,
    DateTimeOffset ImportedAt,
    int RowCount);

public sealed record CampaignItem(
    Guid Id,
    Guid CampaignId,
    Guid ImportBatchId,
    int SourceRow,
    string Source,
    string OriginalVigency,
    string DescriptionTabloid,
    string NormalizedDescriptionTabloid,
    string QuantityRaw,
    string PriceSaleRaw,
    string PriceClubRaw,
    decimal Quantity,
    string Unit,
    decimal OriginalPriceSale,
    decimal OriginalPriceClub,
    decimal FinalPriceSale,
    decimal FinalPriceClub,
    string DescriptionSolidus,
    string Barcode,
    string CodeType,
    IReadOnlyList<string> RiskFlags,
    IReadOnlyList<string> BlockingReasons,
    bool ReviewRequired,
    string ReviewStatus,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record ReviewDecision(
    Guid Id,
    Guid CampaignId,
    Guid CampaignItemId,
    Guid DecidedBy,
    string Decision,
    string Comment,
    DateTimeOffset CreatedAt);

public sealed record ExportBatch(
    Guid Id,
    Guid CampaignId,
    Guid ExportedBy,
    DateTimeOffset ExportedAt,
    string FileName,
    string Content,
    int RowCount);

public sealed record AuditLog(
    Guid Id,
    Guid? ActorUserId,
    string ActorEmail,
    string Action,
    string EntityType,
    Guid? EntityId,
    string Details,
    DateTimeOffset CreatedAt);

public sealed record CampaignStats(
    int TotalItems,
    int BlockingItems,
    int PendingReviewItems,
    int MissingCodeItems,
    int WeightedItems,
    int PackageItems);

public sealed record RawCampaignRow(
    int SourceRow,
    string Source,
    string OriginalVigency,
    string DescriptionTabloid,
    string QuantityRaw,
    string PriceSaleRaw,
    string PriceClubRaw);

public sealed record CatalogImportRow(
    int SourceRow,
    string DescriptionTabloid,
    string Category,
    string DescriptionSolidus,
    string Barcode);
