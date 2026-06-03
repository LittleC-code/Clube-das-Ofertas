using ClubeDasOfertas.Web.Domain;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace ClubeDasOfertas.Web.Services;

public sealed class ImportException(string message) : Exception(message);

public sealed partial class SpreadsheetImporter
{
    public async Task<IReadOnlyList<RawCampaignRow>> ReadCampaignRowsAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        var rows = await ReadRowsAsync(file, "Base Clube - CLT", cancellationToken);
        var headerIndex = FindHeaderRow(rows, "DESCRICAO NO TABLOIDE", "VENDA CLUBE");
        if (headerIndex < 0)
        {
            throw new ImportException("Nao encontrei o cabecalho esperado: Fonte, Vigencia, Descricao no Tabloide, Quantidade limitada, Venda, Venda Clube.");
        }

        var header = rows[headerIndex].Select(TextNormalizer.NormalizeKey).ToList();
        var sourceCol = RequiredColumn(header, "FONTE");
        var vigencyCol = RequiredColumn(header, "VIGENCIA");
        var descriptionCol = RequiredColumn(header, "DESCRICAO NO TABLOIDE");
        var quantityCol = RequiredColumn(header, "QUANTIDADE LIMITADA");
        var priceSaleCol = RequiredColumn(header, "VENDA");
        var priceClubCol = RequiredColumn(header, "VENDA CLUBE");

        var result = new List<RawCampaignRow>();
        for (var i = headerIndex + 1; i < rows.Count; i++)
        {
            var row = rows[i];
            var description = Cell(row, descriptionCol);
            if (string.IsNullOrWhiteSpace(description))
            {
                continue;
            }

            result.Add(new RawCampaignRow(
                i + 1,
                Cell(row, sourceCol),
                Cell(row, vigencyCol),
                description,
                Cell(row, quantityCol),
                Cell(row, priceSaleCol),
                Cell(row, priceClubCol)));
        }

        return result;
    }

    public async Task<IReadOnlyList<CatalogImportRow>> ReadCatalogRowsAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        var rows = await ReadRowsAsync(file, "Base - Cod Barras", cancellationToken);
        var headerIndex = FindHeaderRow(rows, "DESCRICAO TABLOIDE", "COD BARRAS");
        if (headerIndex < 0)
        {
            throw new ImportException("Nao encontrei o cabecalho esperado do catalogo: Descricao Tabloide, Categoria, Descricao Solidus, Cod Barras.");
        }

        var header = rows[headerIndex].Select(TextNormalizer.NormalizeKey).ToList();
        var descriptionCol = RequiredColumn(header, "DESCRICAO TABLOIDE");
        var categoryCol = OptionalColumn(header, "CATEGORIA");
        var solidusCol = RequiredColumn(header, "DESCRICAO SOLIDUS");
        var barcodeCol = RequiredColumn(header, "COD BARRAS");

        var result = new List<CatalogImportRow>();
        for (var i = headerIndex + 1; i < rows.Count; i++)
        {
            var row = rows[i];
            var description = Cell(row, descriptionCol);
            var barcode = Cell(row, barcodeCol);
            if (string.IsNullOrWhiteSpace(description) || string.IsNullOrWhiteSpace(barcode))
            {
                continue;
            }

            result.Add(new CatalogImportRow(
                i + 1,
                description,
                categoryCol >= 0 ? Cell(row, categoryCol) : "",
                Cell(row, solidusCol),
                barcode));
        }

        return result;
    }

    private static async Task<IReadOnlyList<IReadOnlyList<string>>> ReadRowsAsync(IFormFile file, string preferredSheet, CancellationToken cancellationToken)
    {
        if (file.Length == 0)
        {
            throw new ImportException("Arquivo vazio.");
        }

        await using var stream = file.OpenReadStream();
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory, cancellationToken);
        memory.Position = 0;

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension is ".xlsx" or ".xlsm")
        {
            return ReadOpenXmlWorkbook(memory, preferredSheet);
        }

        if (extension is ".csv" or ".txt")
        {
            return ReadCsv(memory);
        }

        throw new ImportException("Formato nao suportado. Use CSV, XLSX ou XLSM.");
    }

    private static IReadOnlyList<IReadOnlyList<string>> ReadCsv(Stream stream)
    {
        var bytes = new byte[stream.Length];
        _ = stream.Read(bytes, 0, bytes.Length);
        var utf8 = Encoding.UTF8.GetString(bytes);
        var text = utf8.Contains('\uFFFD') ? Encoding.Latin1.GetString(bytes) : utf8;
        var firstLine = text.Split(["\r\n", "\n", "\r"], StringSplitOptions.None).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? "";
        var separator = firstLine.Count(c => c == ';') >= firstLine.Count(c => c == ',') ? ';' : ',';
        return ParseDelimited(text, separator);
    }

    private static IReadOnlyList<IReadOnlyList<string>> ParseDelimited(string text, char separator)
    {
        var rows = new List<IReadOnlyList<string>>();
        var row = new List<string>();
        var cell = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < text.Length && text[i + 1] == '"')
                {
                    cell.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == separator && !inQuotes)
            {
                row.Add(cell.ToString());
                cell.Clear();
            }
            else if ((c == '\r' || c == '\n') && !inQuotes)
            {
                row.Add(cell.ToString());
                cell.Clear();
                if (row.Any(x => !string.IsNullOrWhiteSpace(x)))
                {
                    rows.Add(row);
                }

                row = [];
                if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
                {
                    i++;
                }
            }
            else
            {
                cell.Append(c);
            }
        }

        row.Add(cell.ToString());
        if (row.Any(x => !string.IsNullOrWhiteSpace(x)))
        {
            rows.Add(row);
        }

        return rows;
    }

    private static IReadOnlyList<IReadOnlyList<string>> ReadOpenXmlWorkbook(Stream stream, string preferredSheet)
    {
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
        var sharedStrings = ReadSharedStrings(archive);
        var sheetPath = ResolveSheetPath(archive, preferredSheet);
        var sheetEntry = archive.GetEntry(sheetPath) ?? throw new ImportException($"A aba '{preferredSheet}' nao foi encontrada no arquivo.");

        using var sheetStream = sheetEntry.Open();
        var sheet = XDocument.Load(sheetStream);
        var rows = new List<IReadOnlyList<string>>();

        foreach (var rowElement in sheet.Descendants().Where(x => x.Name.LocalName == "row"))
        {
            var cells = new Dictionary<int, string>();
            foreach (var cellElement in rowElement.Elements().Where(x => x.Name.LocalName == "c"))
            {
                var reference = (string?)cellElement.Attribute("r") ?? "";
                var column = ColumnIndex(reference);
                if (column <= 0)
                {
                    continue;
                }

                cells[column] = ReadCellValue(cellElement, sharedStrings);
            }

            if (cells.Count == 0)
            {
                rows.Add(Array.Empty<string>());
                continue;
            }

            var max = cells.Keys.Max();
            var values = new string[max];
            for (var i = 1; i <= max; i++)
            {
                values[i - 1] = cells.TryGetValue(i, out var value) ? value : "";
            }

            rows.Add(values);
        }

        return rows;
    }

    private static IReadOnlyList<string> ReadSharedStrings(ZipArchive archive)
    {
        var entry = archive.GetEntry("xl/sharedStrings.xml");
        if (entry is null)
        {
            return Array.Empty<string>();
        }

        using var stream = entry.Open();
        var document = XDocument.Load(stream);
        return document.Descendants()
            .Where(x => x.Name.LocalName == "si")
            .Select(si => string.Concat(si.Descendants().Where(x => x.Name.LocalName == "t").Select(x => x.Value)))
            .ToList();
    }

    private static string ResolveSheetPath(ZipArchive archive, string preferredSheet)
    {
        var workbookEntry = archive.GetEntry("xl/workbook.xml") ?? throw new ImportException("Arquivo XLSX/XLSM sem workbook.xml.");
        var relationshipsEntry = archive.GetEntry("xl/_rels/workbook.xml.rels") ?? throw new ImportException("Arquivo XLSX/XLSM sem relacionamentos de abas.");

        using var workbookStream = workbookEntry.Open();
        using var relsStream = relationshipsEntry.Open();
        var workbook = XDocument.Load(workbookStream);
        var rels = XDocument.Load(relsStream);
        var preferred = TextNormalizer.NormalizeKey(preferredSheet);

        var sheets = workbook.Descendants()
            .Where(x => x.Name.LocalName == "sheet")
            .Select(x => new
            {
                Name = (string?)x.Attribute("name") ?? "",
                RelationshipId = x.Attributes().FirstOrDefault(a => a.Name.LocalName == "id")?.Value ?? ""
            })
            .ToList();

        var selected = sheets.FirstOrDefault(x => TextNormalizer.NormalizeKey(x.Name) == preferred) ?? sheets.FirstOrDefault();
        if (selected is null)
        {
            throw new ImportException("Nenhuma aba encontrada no arquivo XLSX/XLSM.");
        }

        var target = rels.Descendants()
            .FirstOrDefault(x => x.Name.LocalName == "Relationship" && ((string?)x.Attribute("Id") ?? "") == selected.RelationshipId)
            ?.Attribute("Target")
            ?.Value;

        if (string.IsNullOrWhiteSpace(target))
        {
            throw new ImportException($"Nao foi possivel resolver a aba '{selected.Name}'.");
        }

        target = target.Replace('\\', '/').TrimStart('/');
        return target.StartsWith("xl/", StringComparison.OrdinalIgnoreCase) ? target : $"xl/{target}";
    }

    private static string ReadCellValue(XElement cell, IReadOnlyList<string> sharedStrings)
    {
        var type = (string?)cell.Attribute("t") ?? "";
        if (type == "inlineStr")
        {
            return string.Concat(cell.Descendants().Where(x => x.Name.LocalName == "t").Select(x => x.Value)).Trim();
        }

        var raw = cell.Elements().FirstOrDefault(x => x.Name.LocalName == "v")?.Value ?? "";
        if (type == "s" && int.TryParse(raw, out var sharedStringIndex) && sharedStringIndex >= 0 && sharedStringIndex < sharedStrings.Count)
        {
            return sharedStrings[sharedStringIndex].Trim();
        }

        return raw.Trim();
    }

    private static int ColumnIndex(string reference)
    {
        var match = ColumnLetters().Match(reference);
        if (!match.Success)
        {
            return 0;
        }

        var index = 0;
        foreach (var c in match.Value)
        {
            index = (index * 26) + (char.ToUpperInvariant(c) - 'A' + 1);
        }

        return index;
    }

    private static int FindHeaderRow(IReadOnlyList<IReadOnlyList<string>> rows, params string[] requiredMarkers)
    {
        for (var i = 0; i < rows.Count; i++)
        {
            var normalized = rows[i].Select(TextNormalizer.NormalizeKey).ToList();
            if (requiredMarkers.All(marker => normalized.Contains(marker)))
            {
                return i;
            }
        }

        return -1;
    }

    private static int RequiredColumn(IReadOnlyList<string> header, string name)
    {
        var index = OptionalColumn(header, name);
        if (index < 0)
        {
            throw new ImportException($"Coluna obrigatoria ausente: {name}.");
        }

        return index;
    }

    private static int OptionalColumn(IReadOnlyList<string> header, string name)
    {
        for (var i = 0; i < header.Count; i++)
        {
            if (header[i] == name)
            {
                return i;
            }
        }

        return -1;
    }

    private static string Cell(IReadOnlyList<string> row, int index)
    {
        return index >= 0 && index < row.Count ? row[index].Trim() : "";
    }

    [GeneratedRegex(@"^[A-Za-z]+")]
    private static partial Regex ColumnLetters();
}
