using ClubeDasOfertas.Web.Data;
using ClubeDasOfertas.Web.Domain;
using System.Globalization;
using System.Text;

namespace ClubeDasOfertas.Web.Services;

public sealed class ExportBlockedException(IReadOnlyList<CampaignItem> blockedItems)
    : Exception("Exportacao bloqueada por pendencias criticas.")
{
    public IReadOnlyList<CampaignItem> BlockedItems { get; } = blockedItems;
}

public sealed class ExportService(AppRepository repository)
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    public async Task<ExportBatch> ExportAsync(Campaign campaign, UserAccount user, CancellationToken cancellationToken = default)
    {
        var items = await repository.GetCampaignItemsAsync(campaign.Id, cancellationToken);
        if (items.Count == 0)
        {
            throw new InvalidOperationException("A campanha nao possui itens importados.");
        }

        var blocked = items.Where(x => x.BlockingReasons.Count > 0).ToList();
        if (blocked.Count > 0)
        {
            throw new ExportBlockedException(blocked);
        }

        var content = BuildCsv(campaign, items);
        var export = new ExportBatch(
            Guid.NewGuid(),
            campaign.Id,
            user.Id,
            DateTimeOffset.UtcNow,
            $"clube_ofertas_{campaign.ValidFrom:yyyyMMdd}_{campaign.ValidTo:yyyyMMdd}.csv",
            content,
            items.Count);

        await repository.SaveExportAsync(export, cancellationToken);
        await repository.AddAuditAsync(user.Id, user.Email, "Exportou CSV", "Campaign", campaign.Id, export.FileName, cancellationToken);
        return export;
    }

    private static string BuildCsv(Campaign campaign, IReadOnlyList<CampaignItem> items)
    {
        var builder = new StringBuilder();
        builder.AppendLine("codigo_barras;preco_venda;preco_clube;quantidade;unidade;fonte;vigencia_inicio;vigencia_fim;descricao_tabloide;descricao_solidus;tipo_codigo");

        foreach (var item in items)
        {
            var values = new[]
            {
                item.Barcode,
                Parsing.MoneyPtBr(item.FinalPriceSale),
                Parsing.MoneyPtBr(item.FinalPriceClub),
                item.Quantity.ToString("0.###", PtBr),
                item.Unit,
                item.Source,
                Parsing.DatePtBr(campaign.ValidFrom),
                Parsing.DatePtBr(campaign.ValidTo),
                item.DescriptionTabloid,
                item.DescriptionSolidus,
                item.CodeType
            };

            builder.AppendLine(string.Join(';', values.Select(TextNormalizer.EscapeCsv)));
        }

        return builder.ToString();
    }
}
