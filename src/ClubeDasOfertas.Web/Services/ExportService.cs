using ClubeDasOfertas.Web.Data;
using ClubeDasOfertas.Web.Domain;
using System.Globalization;
using System.Text;

namespace ClubeDasOfertas.Web.Services;

public sealed class ExportService(AppRepository repository)
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    public async Task<ExportBatch> ExportAsync(Campaign campaign, UserAccount user, CancellationToken cancellationToken = default)
    {
        var items = await repository.GetCampaignItemsAsync(campaign.Id, cancellationToken);
        if (items.Count == 0)
        {
            throw new InvalidOperationException("A campanha não possui itens importados.");
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

    internal static string BuildCsv(Campaign campaign, IReadOnlyList<CampaignItem> items)
    {
        var builder = new StringBuilder();
        builder.AppendLine("codigo_barras;preco_venda;preco_clube;quantidade;unidade;fonte;vigencia_inicio;vigencia_fim;descricao_tabloide;descricao_solidus;tipo_codigo;status_item;status_revisao;revisao_obrigatoria;riscos;pendencias;linha_origem;vigencia_original;preco_original_venda;preco_original_clube");

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
                item.CodeType,
                ItemStatus(item),
                ReviewStatusLabel(item.ReviewStatus),
                item.ReviewRequired ? "Sim" : "Nao",
                JoinValues(item.RiskFlags),
                JoinValues(item.BlockingReasons),
                item.SourceRow.ToString(CultureInfo.InvariantCulture),
                item.OriginalVigency,
                Parsing.MoneyPtBr(item.OriginalPriceSale),
                Parsing.MoneyPtBr(item.OriginalPriceClub)
            };

            builder.AppendLine(string.Join(';', values.Select(TextNormalizer.EscapeCsv)));
        }

        return builder.ToString();
    }

    internal static string ItemStatus(CampaignItem item)
    {
        if (item.BlockingReasons.Count > 0)
        {
            return "Bloqueado";
        }

        return item.ReviewStatus switch
        {
            ReviewStatus.Approved => "Aprovado",
            ReviewStatus.Pending => "Pendente",
            ReviewStatus.Rejected => "Rejeitado",
            _ => "Pronto"
        };
    }

    private static string ReviewStatusLabel(string reviewStatus)
    {
        return reviewStatus switch
        {
            ReviewStatus.NotRequired => "Nao requerido",
            ReviewStatus.Pending => "Pendente",
            ReviewStatus.Approved => "Aprovado",
            ReviewStatus.Rejected => "Rejeitado",
            _ => reviewStatus
        };
    }

    private static string JoinValues(IReadOnlyList<string> values)
    {
        return values.Count == 0 ? "" : string.Join(" | ", values);
    }
}
