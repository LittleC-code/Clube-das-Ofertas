using ClubeDasOfertas.Web.Data;
using ClubeDasOfertas.Web.Domain;
using System.Globalization;
using System.IO.Compression;
using System.Security;
using System.Text;

namespace ClubeDasOfertas.Web.Services;

public sealed class ExportService(AppRepository repository)
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");
    private const string CsvContentType = "text/csv; charset=utf-8";
    private const string XlsxContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    private static readonly ExportColumnDefinition[] ExportColumns =
    [
        new("codigo_barras", "codigo_barras", "Codigo de barras"),
        new("preco_venda", "preco_venda", "Preco venda"),
        new("preco_clube", "preco_clube", "Preco clube"),
        new("quantidade", "quantidade", "Quantidade"),
        new("unidade", "unidade", "Unidade"),
        new("fonte", "fonte", "Fonte"),
        new("vigencia_inicio", "vigencia_inicio", "Vigencia inicio"),
        new("vigencia_fim", "vigencia_fim", "Vigencia fim"),
        new("descricao_tabloide", "descricao_tabloide", "Descricao tabloide"),
        new("descricao_solidus", "descricao_solidus", "Descricao Solidus"),
        new("tipo_codigo", "tipo_codigo", "Tipo de codigo"),
        new("status_item", "status_item", "Status do item"),
        new("status_revisao", "status_revisao", "Status da revisao"),
        new("revisao_obrigatoria", "revisao_obrigatoria", "Revisao obrigatoria"),
        new("riscos", "riscos", "Riscos"),
        new("pendencias", "pendencias", "Pendencias"),
        new("linha_origem", "linha_origem", "Linha de origem"),
        new("vigencia_original", "vigencia_original", "Vigencia original"),
        new("preco_original_venda", "preco_original_venda", "Preco original venda"),
        new("preco_original_clube", "preco_original_clube", "Preco original clube")
    ];
    private static readonly ExportPresetDefinition[] ExportPresets =
    [
        new(
            "lojas",
            "Exportar para lojas",
            "Usa o bloco operacional de envio externo com as colunas equivalentes de A ate H.",
            ["fonte", "vigencia_original", "descricao_tabloide", "quantidade", "preco_venda", "preco_clube", "descricao_solidus", "codigo_barras"]),
        new(
            "interno-crm",
            "Exportar para uso interno e CRM",
            "Mantem o conjunto completo para revisao interna e apoio ao concatenado do CRM.",
            ExportColumns.Select(static column => column.Key).ToArray())
    ];

    public static IReadOnlyList<ExportColumnDefinition> AvailableColumns => ExportColumns;
    public static IReadOnlyList<ExportPresetDefinition> AvailablePresets => ExportPresets;

    public async Task<ExportBatch> ExportAsync(Campaign campaign, UserAccount user, IReadOnlyList<string>? selectedColumns = null, CancellationToken cancellationToken = default)
    {
        var items = await repository.GetCampaignItemsAsync(campaign.Id, cancellationToken);
        if (items.Count == 0)
        {
            throw new InvalidOperationException("A campanha não possui itens importados.");
        }

        var normalizedColumns = NormalizeSelectedColumns(selectedColumns);
        var content = BuildCsv(campaign, items, normalizedColumns);
        var export = new ExportBatch(
            Guid.NewGuid(),
            campaign.Id,
            user.Id,
            DateTimeOffset.UtcNow,
            $"clube_ofertas_{campaign.ValidFrom:yyyyMMdd}_{campaign.ValidTo:yyyyMMdd}.csv",
            CsvContentType,
            "text",
            content,
            items.Count);

        await repository.SaveExportAsync(export, cancellationToken);
        await repository.AddAuditAsync(user.Id, user.Email, "Exportou CSV", "Campaign", campaign.Id, export.FileName, cancellationToken);
        return export;
    }

    public async Task<ExportBatch> ExportStoreWorkbookAsync(Campaign campaign, UserAccount user, CancellationToken cancellationToken = default)
    {
        var items = await repository.GetCampaignItemsAsync(campaign.Id, cancellationToken);
        if (items.Count == 0)
        {
            throw new InvalidOperationException("A campanha não possui itens importados.");
        }

        var bytes = BuildStoreWorkbook(campaign, items);
        var export = new ExportBatch(
            Guid.NewGuid(),
            campaign.Id,
            user.Id,
            DateTimeOffset.UtcNow,
            $"clube_ofertas_lojas_{campaign.ValidFrom:yyyyMMdd}_{campaign.ValidTo:yyyyMMdd}.xlsx",
            XlsxContentType,
            "base64",
            Convert.ToBase64String(bytes),
            items.Count);

        await repository.SaveExportAsync(export, cancellationToken);
        await repository.AddAuditAsync(user.Id, user.Email, "Exportou XLSX", "Campaign", campaign.Id, export.FileName, cancellationToken);
        return export;
    }

    public async Task<ExportBatch> ExportInternalWorkbookAsync(Campaign campaign, UserAccount user, CancellationToken cancellationToken = default)
    {
        var items = await repository.GetCampaignItemsAsync(campaign.Id, cancellationToken);
        if (items.Count == 0)
        {
            throw new InvalidOperationException("A campanha nÃ£o possui itens importados.");
        }

        var normalizedDescriptions = items
            .Select(item => item.NormalizedDescriptionTabloid)
            .Where(static description => !string.IsNullOrWhiteSpace(description))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var catalogCounts = await repository.CountCatalogMatchesByNormalizedDescriptionsAsync(normalizedDescriptions, cancellationToken);
        var bytes = BuildInternalWorkbook(campaign, items, catalogCounts);
        var export = new ExportBatch(
            Guid.NewGuid(),
            campaign.Id,
            user.Id,
            DateTimeOffset.UtcNow,
            $"clube_ofertas_interno_{campaign.ValidFrom:yyyyMMdd}_{campaign.ValidTo:yyyyMMdd}.xlsx",
            XlsxContentType,
            "base64",
            Convert.ToBase64String(bytes),
            items.Count);

        await repository.SaveExportAsync(export, cancellationToken);
        await repository.AddAuditAsync(user.Id, user.Email, "Exportou XLSX interno", "Campaign", campaign.Id, export.FileName, cancellationToken);
        return export;
    }

    internal static string BuildCsv(Campaign campaign, IReadOnlyList<CampaignItem> items, IReadOnlyList<string>? selectedColumns = null)
    {
        var columns = NormalizeSelectedColumns(selectedColumns);
        var builder = new StringBuilder();
        builder.AppendLine(string.Join(';', columns.Select(GetColumnHeader)));

        foreach (var item in items)
        {
            var values = columns.Select(column => TextNormalizer.EscapeCsv(GetColumnValue(column, campaign, item)));
            builder.AppendLine(string.Join(';', values));
        }

        return builder.ToString();
    }

    internal static byte[] BuildStoreWorkbook(Campaign campaign, IReadOnlyList<CampaignItem> items)
    {
        var storeColumns = GetPresetColumns("lojas")
            .Select(key => ExportColumns.First(column => string.Equals(column.Key, key, StringComparison.OrdinalIgnoreCase)))
            .ToArray();
        var rows = items
            .Select(item => storeColumns.Select(column => GetColumnValue(column.Key, campaign, item)).ToArray())
            .ToList();

        using var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            WriteEntry(archive, "[Content_Types].xml", """
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
  <Default Extension="xml" ContentType="application/xml"/>
  <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
  <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
  <Override PartName="/xl/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"/>
</Types>
""");
            WriteEntry(archive, "_rels/.rels", """
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
</Relationships>
""");
            WriteEntry(archive, "xl/workbook.xml", $"""
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
  <sheets>
    <sheet name="{EscapeXml($"Tabloide - {campaign.ValidFrom:dd.MM.yyyy}")}" sheetId="1" r:id="rId1"/>
  </sheets>
</workbook>
""");
            WriteEntry(archive, "xl/_rels/workbook.xml.rels", """
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
  <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/>
</Relationships>
""");
            WriteEntry(archive, "xl/styles.xml", BuildStoreStylesXml());
            WriteEntry(archive, "xl/worksheets/sheet1.xml", BuildStoreWorksheetXml(campaign, storeColumns, rows));
        }

        stream.Position = 0;
        return stream.ToArray();
    }

    internal static byte[] BuildInternalWorkbook(
        Campaign campaign,
        IReadOnlyList<CampaignItem> items,
        IReadOnlyDictionary<string, int>? catalogMatchCounts = null)
    {
        var counts = catalogMatchCounts ?? new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var rows = items
            .Select(item => BuildInternalWorkbookRow(campaign, item, items, counts))
            .ToList();

        using var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            WriteEntry(archive, "[Content_Types].xml", """
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
  <Default Extension="xml" ContentType="application/xml"/>
  <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
  <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
  <Override PartName="/xl/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"/>
</Types>
""");
            WriteEntry(archive, "_rels/.rels", """
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
</Relationships>
""");
            WriteEntry(archive, "xl/workbook.xml", $"""
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
  <sheets>
    <sheet name="{EscapeXml($"Interno - {campaign.ValidFrom:dd.MM.yyyy}")}" sheetId="1" r:id="rId1"/>
  </sheets>
</workbook>
""");
            WriteEntry(archive, "xl/_rels/workbook.xml.rels", """
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
  <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/>
</Relationships>
""");
            WriteEntry(archive, "xl/styles.xml", BuildStoreStylesXml());
            WriteEntry(archive, "xl/worksheets/sheet1.xml", BuildInternalWorksheetXml(campaign, rows));
        }

        stream.Position = 0;
        return stream.ToArray();
    }

    public static IReadOnlyList<string> NormalizeSelectedColumns(IEnumerable<string>? selectedColumns)
    {
        if (selectedColumns is null)
        {
            return ExportColumns.Select(static column => column.Key).ToArray();
        }

        var validKeys = ExportColumns
            .Select(static column => column.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var normalized = selectedColumns
            .Select(column => column?.Trim() ?? "")
            .Where(static column => column.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(validKeys.Contains)
            .ToArray();

        if (normalized.Length == 0)
        {
            throw new InvalidOperationException("Selecione pelo menos uma coluna para exportar o CSV.");
        }

        var hadAnyInput = selectedColumns.Any(column => !string.IsNullOrWhiteSpace(column));
        if (hadAnyInput && normalized.Length == 0)
        {
            throw new InvalidOperationException("Nenhuma coluna de exportação reconhecida foi selecionada.");
        }

        return normalized;
    }

    public static IReadOnlyList<string> GetPresetColumns(string presetKey)
    {
        var preset = ExportPresets.FirstOrDefault(preset => string.Equals(preset.Key, presetKey, StringComparison.OrdinalIgnoreCase));
        if (preset is null)
        {
            throw new InvalidOperationException($"Preset de exportacao desconhecido: {presetKey}.");
        }

        return preset.ColumnKeys;
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

    private static string GetColumnHeader(string key)
    {
        return ExportColumns.First(column => string.Equals(column.Key, key, StringComparison.OrdinalIgnoreCase)).Header;
    }

    private static string GetColumnValue(string key, Campaign campaign, CampaignItem item)
    {
        return key switch
        {
            "codigo_barras" => item.Barcode,
            "preco_venda" => Parsing.MoneyPtBr(item.FinalPriceSale),
            "preco_clube" => Parsing.MoneyPtBr(item.FinalPriceClub),
            "quantidade" => item.Quantity.ToString("0.###", PtBr),
            "unidade" => item.Unit,
            "fonte" => item.Source,
            "vigencia_inicio" => Parsing.DatePtBr(campaign.ValidFrom),
            "vigencia_fim" => Parsing.DatePtBr(campaign.ValidTo),
            "descricao_tabloide" => item.DescriptionTabloid,
            "descricao_solidus" => item.DescriptionSolidus,
            "tipo_codigo" => item.CodeType,
            "status_item" => ItemStatus(item),
            "status_revisao" => ReviewStatusLabel(item.ReviewStatus),
            "revisao_obrigatoria" => item.ReviewRequired ? "Sim" : "Nao",
            "riscos" => JoinValues(item.RiskFlags),
            "pendencias" => JoinValues(item.BlockingReasons),
            "linha_origem" => item.SourceRow.ToString(CultureInfo.InvariantCulture),
            "vigencia_original" => item.OriginalVigency,
            "preco_original_venda" => Parsing.MoneyPtBr(item.OriginalPriceSale),
            "preco_original_clube" => Parsing.MoneyPtBr(item.OriginalPriceClub),
            _ => throw new InvalidOperationException($"Coluna de exportação desconhecida: {key}.")
        };
    }

    private static string BuildStoreWorksheetXml(Campaign campaign, IReadOnlyList<ExportColumnDefinition> columns, IReadOnlyList<string[]> rows)
    {
        var maxRow = rows.Count + 2;
        var lastColumn = ColumnLetter(columns.Count);
        var xml = new StringBuilder();
        xml.AppendLine("""<?xml version="1.0" encoding="UTF-8" standalone="yes"?>""");
        xml.AppendLine("""<worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">""");
        xml.AppendLine($"""  <dimension ref="A1:{lastColumn}{maxRow}"/>""");
        xml.AppendLine("""  <sheetViews><sheetView workbookViewId="0"><pane ySplit="2" topLeftCell="A3" activePane="bottomLeft" state="frozen"/></sheetView></sheetViews>""");
        xml.AppendLine("""  <cols>""");
        foreach (var (width, index) in GetStoreColumnWidths(columns.Count).Select((width, index) => (width, index)))
        {
            xml.AppendLine($"""    <col min="{index + 1}" max="{index + 1}" width="{width.ToString(CultureInfo.InvariantCulture)}" customWidth="1"/>""");
        }
        xml.AppendLine("""  </cols>""");
        xml.AppendLine("""  <sheetData>""");
        xml.AppendLine("""    <row r="1" ht="26" customHeight="1">""");
        xml.AppendLine($"""      {InlineStringCell("A1", $"Clube Das Ofertas - {campaign.Name}", 1)}""");
        for (var i = 1; i < columns.Count; i++)
        {
            xml.AppendLine($"""      <c r="{ColumnLetter(i + 1)}1" s="1"/>""");
        }
        xml.AppendLine("""    </row>""");
        xml.AppendLine("""    <row r="2" ht="22" customHeight="1">""");
        for (var i = 0; i < columns.Count; i++)
        {
            xml.AppendLine($"""      {InlineStringCell($"{ColumnLetter(i + 1)}2", DisplayStoreHeader(columns[i].Key), 2)}""");
        }
        xml.AppendLine("""    </row>""");

        for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        {
            var rowNumber = rowIndex + 3;
            xml.AppendLine($"""    <row r="{rowNumber}" ht="18" customHeight="1">""");
            for (var columnIndex = 0; columnIndex < columns.Count; columnIndex++)
            {
                var key = columns[columnIndex].Key;
                var value = rows[rowIndex][columnIndex];
                var style = key is "preco_venda" or "preco_clube" ? 4 : key is "quantidade" ? 5 : columnIndex <= 1 ? 3 : 3;
                xml.AppendLine($"""      {InlineStringCell($"{ColumnLetter(columnIndex + 1)}{rowNumber}", value, style)}""");
            }
            xml.AppendLine("""    </row>""");
        }

        xml.AppendLine("""  </sheetData>""");
        xml.AppendLine($"""  <autoFilter ref="A2:{lastColumn}{maxRow}"/>""");
        xml.AppendLine($"""  <mergeCells count="{1 + CountStoreMergeRanges(rows)}">""");
        xml.AppendLine($"""    <mergeCell ref="A1:{lastColumn}1"/>""");
        foreach (var merge in GetStoreMergeRanges(rows))
        {
            xml.AppendLine($"""    <mergeCell ref="{merge}"/>""");
        }
        xml.AppendLine("""  </mergeCells>""");
        xml.AppendLine("""  <pageMargins left="0.4" right="0.4" top="0.6" bottom="0.6" header="0.3" footer="0.3"/>""");
        xml.AppendLine("""</worksheet>""");
        return xml.ToString();
    }

    private static string BuildInternalWorksheetXml(Campaign campaign, IReadOnlyList<string[]> rows)
    {
        const int columnCount = 20;
        var maxRow = rows.Count + 2;
        var lastColumn = ColumnLetter(columnCount);
        var xml = new StringBuilder();
        xml.AppendLine("""<?xml version="1.0" encoding="UTF-8" standalone="yes"?>""");
        xml.AppendLine("""<worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">""");
        xml.AppendLine($"""  <dimension ref="A1:{lastColumn}{maxRow}"/>""");
        xml.AppendLine("""  <sheetViews><sheetView workbookViewId="0"><pane ySplit="2" topLeftCell="A3" activePane="bottomLeft" state="frozen"/></sheetView></sheetViews>""");
        xml.AppendLine("""  <cols>""");
        foreach (var (width, index) in GetInternalColumnWidths().Select((width, index) => (width, index)))
        {
            xml.AppendLine($"""    <col min="{index + 1}" max="{index + 1}" width="{width.ToString(CultureInfo.InvariantCulture)}" customWidth="1"/>""");
        }
        xml.AppendLine("""  </cols>""");
        xml.AppendLine("""  <sheetData>""");
        xml.AppendLine("""    <row r="1" ht="28" customHeight="1">""");
        for (var i = 0; i < columnCount; i++)
        {
            var reference = $"{ColumnLetter(i + 1)}1";
            var value = GetInternalTitleRowValue(campaign, i);
            xml.AppendLine($"""      {InlineStringCell(reference, value, i is 0 or >= 9 ? 1 : 3)}""");
        }
        xml.AppendLine("""    </row>""");
        xml.AppendLine("""    <row r="2" ht="38" customHeight="1">""");
        for (var i = 0; i < columnCount; i++)
        {
            var reference = $"{ColumnLetter(i + 1)}2";
            var value = GetInternalHeaderRowValue(i);
            xml.AppendLine($"""      {InlineStringCell(reference, value, 2)}""");
        }
        xml.AppendLine("""    </row>""");

        for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        {
            var rowNumber = rowIndex + 3;
            xml.AppendLine($"""    <row r="{rowNumber}" ht="19" customHeight="1">""");
            for (var columnIndex = 0; columnIndex < columnCount; columnIndex++)
            {
                var reference = $"{ColumnLetter(columnIndex + 1)}{rowNumber}";
                var value = rows[rowIndex][columnIndex];
                var style = columnIndex is 4 or 5 or 9 or 10 or 14 or 15 or 16 or 17 or 19 ? 4 : 3;
                xml.AppendLine($"""      {InlineStringCell(reference, value, style)}""");
            }
            xml.AppendLine("""    </row>""");
        }

        xml.AppendLine("""  </sheetData>""");
        xml.AppendLine($"""  <autoFilter ref="A2:{lastColumn}{maxRow}"/>""");
        xml.AppendLine("""  <pageMargins left="0.4" right="0.4" top="0.6" bottom="0.6" header="0.3" footer="0.3"/>""");
        xml.AppendLine("""</worksheet>""");
        return xml.ToString();
    }

    private static string[] BuildInternalWorkbookRow(
        Campaign campaign,
        CampaignItem item,
        IReadOnlyList<CampaignItem> allItems,
        IReadOnlyDictionary<string, int> catalogMatchCounts)
    {
        var quantityValue = item.Quantity.ToString("0.###", PtBr);
        var saleValue = Parsing.MoneyPtBr(item.FinalPriceSale);
        var clubValue = Parsing.MoneyPtBr(item.FinalPriceClub);
        var codeType = string.IsNullOrWhiteSpace(item.CodeType) ? Parsing.CodeType(item.Barcode) : item.CodeType;
        var catalogCount = catalogMatchCounts.TryGetValue(item.NormalizedDescriptionTabloid, out var count) ? count : 0;
        var sameAsTabloid = IsAlsoInTabloid(item, allItems);

        return
        [
            item.Source,
            item.OriginalVigency,
            item.DescriptionTabloid,
            item.QuantityRaw,
            saleValue,
            clubValue,
            item.DescriptionSolidus,
            item.Barcode,
            "",
            codeType,
            catalogCount == 0 ? "" : catalogCount.ToString(CultureInfo.InvariantCulture),
            "",
            $"{item.Source} {campaign.ValidFrom:dd.MM.yyyy}",
            sameAsTabloid ? "Sim" : "-",
            BuildConcatenatedOffer(item.Barcode, saleValue, clubValue, quantityValue),
            ItemStatus(item),
            quantityValue,
            item.Unit,
            "",
            ""
        ];
    }

    private static IEnumerable<string> GetStoreMergeRanges(IReadOnlyList<string[]> rows)
    {
        if (rows.Count == 0)
        {
            yield break;
        }

        foreach (var columnIndex in new[] { 0, 1 })
        {
            var start = 0;
            while (start < rows.Count)
            {
                var value = rows[start][columnIndex];
                var end = start + 1;
                while (end < rows.Count && string.Equals(rows[end][columnIndex], value, StringComparison.Ordinal))
                {
                    end++;
                }

                if (end - start > 1 && !string.IsNullOrWhiteSpace(value))
                {
                    yield return $"{ColumnLetter(columnIndex + 1)}{start + 3}:{ColumnLetter(columnIndex + 1)}{end + 2}";
                }

                start = end;
            }
        }
    }

    private static int CountStoreMergeRanges(IReadOnlyList<string[]> rows) => GetStoreMergeRanges(rows).Count();

    private static IReadOnlyList<double> GetStoreColumnWidths(int columnCount)
    {
        var baseWidths = new[] { 14d, 26d, 44d, 18d, 12d, 14d, 42d, 22d };
        return baseWidths.Take(columnCount).ToArray();
    }

    private static IReadOnlyList<double> GetInternalColumnWidths()
    {
        return
        [
            14d, 22d, 38d, 18d, 12d, 12d, 32d, 18d, 4d, 18d,
            16d, 4d, 18d, 19d, 26d, 16d, 12d, 12d, 4d, 10d
        ];
    }

    private static string GetInternalTitleRowValue(Campaign campaign, int columnIndex)
    {
        return columnIndex switch
        {
            0 => $"Clube Das Ofertas - {campaign.Name}",
            9 => "Codigo Unificado ou EAN",
            10 => "Quantos codigos de barra esse item tem na base?",
            11 => ";",
            12 => "Ao enviar as lojas, enviar da coluna A ate H. Da coluna I ate T nao precisa ser enviada, pois sao informacoes internas da Rede",
            18 => ";",
            19 => "Orientacoes",
            _ => ""
        };
    }

    private static string GetInternalHeaderRowValue(int columnIndex)
    {
        return columnIndex switch
        {
            0 => "Fonte",
            1 => "Vigencia",
            2 => "Descricao no Tabloide",
            3 => "Quantidade limitada",
            4 => "Venda",
            5 => "Venda Clube",
            6 => "Descricao Solidus",
            7 => "Cod Barras",
            12 => "3. Vigencia",
            13 => "2. Item esta nas ofertas do tabloide?",
            14 => "Concatenado",
            15 => "1. Status",
            16 => "Quantidade",
            17 => "Unidade",
            19 => "1",
            _ => ""
        };
    }

    private static bool IsAlsoInTabloid(CampaignItem item, IReadOnlyList<CampaignItem> allItems)
    {
        if (string.Equals(item.Source, "Tabloide", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return allItems.Any(other =>
            !ReferenceEquals(other, item)
            && string.Equals(other.Source, "Tabloide", StringComparison.OrdinalIgnoreCase)
            && (
                (!string.IsNullOrWhiteSpace(item.Barcode)
                    && string.Equals(other.Barcode, item.Barcode, StringComparison.OrdinalIgnoreCase))
                || (!string.IsNullOrWhiteSpace(item.NormalizedDescriptionTabloid)
                    && string.Equals(other.NormalizedDescriptionTabloid, item.NormalizedDescriptionTabloid, StringComparison.OrdinalIgnoreCase))));
    }

    private static string BuildConcatenatedOffer(string barcode, string saleValue, string clubValue, string quantityValue)
    {
        return string.Join(';', [barcode, saleValue, clubValue, quantityValue]);
    }

    private static string DisplayStoreHeader(string key)
    {
        return key switch
        {
            "fonte" => "Fonte",
            "vigencia_original" => "Vigencia",
            "descricao_tabloide" => "Descricao no Tabloide",
            "quantidade" => "Quantidade limitada",
            "preco_venda" => "Venda",
            "preco_clube" => "Venda Clube",
            "descricao_solidus" => "Descricao Solidus",
            "codigo_barras" => "Cod Barras",
            _ => GetColumnHeader(key)
        };
    }

    private static string BuildStoreStylesXml()
    {
        return """
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
  <fonts count="3">
    <font><sz val="11"/><name val="Calibri"/></font>
    <font><b/><sz val="15"/><color rgb="FFFFFFFF"/><name val="Calibri"/></font>
    <font><b/><sz val="11"/><color rgb="FFFFFFFF"/><name val="Calibri"/></font>
  </fonts>
  <fills count="4">
    <fill><patternFill patternType="none"/></fill>
    <fill><patternFill patternType="gray125"/></fill>
    <fill><patternFill patternType="solid"><fgColor rgb="FF0C82BC"/><bgColor indexed="64"/></patternFill></fill>
    <fill><patternFill patternType="solid"><fgColor rgb="FF1F5E9C"/><bgColor indexed="64"/></patternFill></fill>
  </fills>
  <borders count="2">
    <border><left/><right/><top/><bottom/><diagonal/></border>
    <border>
      <left style="thin"><color rgb="FFD7E3F0"/></left>
      <right style="thin"><color rgb="FFD7E3F0"/></right>
      <top style="thin"><color rgb="FFD7E3F0"/></top>
      <bottom style="thin"><color rgb="FFD7E3F0"/></bottom>
      <diagonal/>
    </border>
  </borders>
  <cellStyleXfs count="1">
    <xf numFmtId="0" fontId="0" fillId="0" borderId="0"/>
  </cellStyleXfs>
  <cellXfs count="6">
    <xf numFmtId="0" fontId="0" fillId="0" borderId="0" xfId="0"/>
    <xf numFmtId="0" fontId="1" fillId="3" borderId="1" xfId="0" applyFont="1" applyFill="1" applyBorder="1" applyAlignment="1"><alignment horizontal="center" vertical="center"/></xf>
    <xf numFmtId="0" fontId="2" fillId="2" borderId="1" xfId="0" applyFont="1" applyFill="1" applyBorder="1" applyAlignment="1"><alignment horizontal="center" vertical="center" wrapText="1"/></xf>
    <xf numFmtId="0" fontId="0" fillId="0" borderId="1" xfId="0" applyBorder="1" applyAlignment="1"><alignment vertical="center" wrapText="1"/></xf>
    <xf numFmtId="0" fontId="0" fillId="0" borderId="1" xfId="0" applyBorder="1" applyAlignment="1"><alignment horizontal="center" vertical="center"/></xf>
    <xf numFmtId="0" fontId="0" fillId="0" borderId="1" xfId="0" applyBorder="1" applyAlignment="1"><alignment horizontal="center" vertical="center"/></xf>
  </cellXfs>
  <cellStyles count="1">
    <cellStyle name="Normal" xfId="0" builtinId="0"/>
  </cellStyles>
</styleSheet>
""";
    }

    private static string InlineStringCell(string reference, string value, int styleId)
    {
        return $"""<c r="{reference}" s="{styleId}" t="inlineStr"><is><t>{EscapeXml(value)}</t></is></c>""";
    }

    private static string ColumnLetter(int index)
    {
        var builder = new StringBuilder();
        var current = index;
        while (current > 0)
        {
            current--;
            builder.Insert(0, (char)('A' + (current % 26)));
            current /= 26;
        }

        return builder.ToString();
    }

    private static void WriteEntry(ZipArchive archive, string path, string content)
    {
        var entry = archive.CreateEntry(path);
        using var writer = new StreamWriter(entry.Open(), new UTF8Encoding(false));
        writer.Write(content);
    }

    private static string EscapeXml(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "";
        }

        var sanitized = new string(value.Where(IsValidXmlChar).ToArray());
        return SecurityElement.Escape(sanitized) ?? "";
    }

    private static bool IsValidXmlChar(char value)
    {
        return value == '\t'
            || value == '\n'
            || value == '\r'
            || (value >= ' ' && value <= '\uD7FF')
            || (value >= '\uE000' && value <= '\uFFFD');
    }

    public sealed record ExportColumnDefinition(
        string Key,
        string Header,
        string Label);

    public sealed record ExportPresetDefinition(
        string Key,
        string Label,
        string Description,
        IReadOnlyList<string> ColumnKeys);
}
