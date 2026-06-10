using ClubeDasOfertas.Web.Domain;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace ClubeDasOfertas.Web.Services;

public sealed class ImportException(string message) : Exception(message);

public sealed partial class SpreadsheetImporter
{
    internal const long MaxUploadBytes = 10 * 1024 * 1024;
    public const string DefaultCampaignSheetName = "Base Clube - CLT";
    public const string DefaultCatalogSheetName = "Base - Cod Barras";

    public async Task<IReadOnlyList<RawCampaignRow>> ReadCampaignRowsAsync(IFormFile file, string preferredSheet = DefaultCampaignSheetName, CancellationToken cancellationToken = default)
    {
        var rows = await ReadRowsAsync(file, preferredSheet, cancellationToken);
        var headerIndex = FindHeaderRow(rows, "DESCRICAO NO TABLOIDE", "VENDA CLUBE");
        if (headerIndex < 0)
        {
            throw new ImportException("Não encontrei o cabeçalho esperado: Fonte, Vigência, Descrição no Tabloide, Quantidade limitada, Venda, Venda Clube.");
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
        var rows = await ReadRowsAsync(file, DefaultCatalogSheetName, cancellationToken);
        var headerIndex = FindHeaderRow(rows, "DESCRICAO TABLOIDE", "COD BARRAS");
        if (headerIndex < 0)
        {
            throw new ImportException("Não encontrei o cabeçalho esperado do catálogo: Descrição Tabloide, Categoria, Descrição Solidus, Cód. Barras.");
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

    public async Task<IReadOnlyList<string>> ListWorksheetNamesAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(file.FileName))
        {
            throw new ImportException("Arquivo sem nome.");
        }

        if (file.Length == 0)
        {
            throw new ImportException("Arquivo vazio.");
        }

        if (file.Length > MaxUploadBytes)
        {
            throw new ImportException("Arquivo excede o limite de 10 MB.");
        }

        await using var stream = file.OpenReadStream();
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory, cancellationToken);
        memory.Position = 0;

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension is not (".xlsx" or ".xlsm"))
        {
            return Array.Empty<string>();
        }

        EnsureZipSignature(memory);
        memory.Position = 0;
        return ReadWorksheetNames(memory);
    }

    private static async Task<IReadOnlyList<IReadOnlyList<string>>> ReadRowsAsync(IFormFile file, string preferredSheet, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(file.FileName))
        {
            throw new ImportException("Arquivo sem nome.");
        }

        if (file.Length == 0)
        {
            throw new ImportException("Arquivo vazio.");
        }

        if (file.Length > MaxUploadBytes)
        {
            throw new ImportException("Arquivo excede o limite de 10 MB.");
        }

        await using var stream = file.OpenReadStream();
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory, cancellationToken);
        memory.Position = 0;

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension is ".xlsx" or ".xlsm")
        {
            EnsureZipSignature(memory);
            memory.Position = 0;
            return ReadOpenXmlWorkbook(memory, preferredSheet);
        }

        if (extension is ".csv" or ".txt")
        {
            EnsureTextPayload(memory);
            memory.Position = 0;
            return ReadCsv(memory);
        }

        throw new ImportException("Formato não suportado. Use CSV, XLSX ou XLSM.");
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
        try
        {
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
            var sharedStrings = ReadSharedStrings(archive);
            var selection = ResolveSheetSelection(archive, preferredSheet);
            var sheetEntry = archive.GetEntry(selection.Path) ?? throw new ImportException($"A aba '{selection.Name}' não foi encontrada no arquivo.");

            using var sheetStream = sheetEntry.Open();
            var sheet = LoadXml(sheetStream);
            var rows = new List<string[]>();

            foreach (var rowElement in sheet.Descendants().Where(x => x.Name.LocalName == "row"))
            {
                var rowNumber = int.TryParse((string?)rowElement.Attribute("r"), out var parsedRowNumber) && parsedRowNumber > 0
                    ? parsedRowNumber
                    : rows.Count + 1;
                while (rows.Count < rowNumber - 1)
                {
                    rows.Add(Array.Empty<string>());
                }

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

            ApplyMergedCellValues(sheet, rows);
            return rows.Select(row => (IReadOnlyList<string>)row).ToList();
        }
        catch (InvalidDataException)
        {
            throw new ImportException("O arquivo XLSX/XLSM parece corrompido ou fora do formato esperado.");
        }
        catch (XmlException)
        {
            throw new ImportException("O conteúdo XML do arquivo está inválido ou corrompido.");
        }
    }

    private static IReadOnlyList<string> ReadSharedStrings(ZipArchive archive)
    {
        var entry = archive.GetEntry("xl/sharedStrings.xml");
        if (entry is null)
        {
            return Array.Empty<string>();
        }

        using var stream = entry.Open();
        var document = LoadXml(stream);
        return document.Descendants()
            .Where(x => x.Name.LocalName == "si")
            .Select(si => string.Concat(si.Descendants().Where(x => x.Name.LocalName == "t").Select(x => x.Value)))
            .ToList();
    }

    private static IReadOnlyList<string> ReadWorksheetNames(Stream stream)
    {
        try
        {
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
            return ReadWorkbookSheets(archive)
                .Select(x => x.Name)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();
        }
        catch (InvalidDataException)
        {
            throw new ImportException("O arquivo XLSX/XLSM parece corrompido ou fora do formato esperado.");
        }
        catch (XmlException)
        {
            throw new ImportException("O conteúdo XML do arquivo está inválido ou corrompido.");
        }
    }

    private static (string Name, string Path) ResolveSheetSelection(ZipArchive archive, string preferredSheet)
    {
        var sheets = ReadWorkbookSheets(archive);
        if (sheets.Count == 0)
        {
            throw new ImportException("Nenhuma aba encontrada no arquivo XLSX/XLSM.");
        }

        var normalizedPreferred = TextNormalizer.NormalizeKey(preferredSheet);
        var selected = sheets[0];
        if (!string.IsNullOrWhiteSpace(normalizedPreferred))
        {
            var matchIndex = sheets.FindIndex(x => TextNormalizer.NormalizeKey(x.Name) == normalizedPreferred);
            if (matchIndex < 0)
            {
                var availableSheets = string.Join(", ", sheets.Select(x => x.Name));
                throw new ImportException($"A aba '{preferredSheet}' não foi encontrada. Abas disponíveis: {availableSheets}.");
            }

            selected = sheets[matchIndex];
        }

        var rels = ReadWorkbookRelationships(archive);
        var target = rels.Descendants()
            .FirstOrDefault(x => x.Name.LocalName == "Relationship" && ((string?)x.Attribute("Id") ?? "") == selected.RelationshipId)
            ?.Attribute("Target")
            ?.Value;

        if (string.IsNullOrWhiteSpace(target))
        {
            throw new ImportException($"Não foi possível resolver a aba '{selected.Name}'.");
        }

        target = target.Replace('\\', '/').TrimStart('/');
        return (selected.Name, target.StartsWith("xl/", StringComparison.OrdinalIgnoreCase) ? target : $"xl/{target}");
    }

    private static List<(string Name, string RelationshipId)> ReadWorkbookSheets(ZipArchive archive)
    {
        var workbook = ReadWorkbookXml(archive);
        return workbook.Descendants()
            .Where(x => x.Name.LocalName == "sheet")
            .Select(x => (
                Name: (string?)x.Attribute("name") ?? "",
                RelationshipId: x.Attributes().FirstOrDefault(a => a.Name.LocalName == "id")?.Value ?? ""))
            .ToList();
    }

    private static XDocument ReadWorkbookXml(ZipArchive archive)
    {
        var workbookEntry = archive.GetEntry("xl/workbook.xml") ?? throw new ImportException("Arquivo XLSX/XLSM sem workbook.xml.");
        using var workbookStream = workbookEntry.Open();
        return LoadXml(workbookStream);
    }

    private static XDocument ReadWorkbookRelationships(ZipArchive archive)
    {
        var relationshipsEntry = archive.GetEntry("xl/_rels/workbook.xml.rels") ?? throw new ImportException("Arquivo XLSX/XLSM sem relacionamentos de abas.");
        using var relsStream = relationshipsEntry.Open();
        return LoadXml(relsStream);
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

    private static void ApplyMergedCellValues(XDocument sheet, List<string[]> rows)
    {
        foreach (var mergeReference in sheet.Descendants()
                     .Where(x => x.Name.LocalName == "mergeCell")
                     .Select(x => (string?)x.Attribute("ref"))
                     .Where(x => !string.IsNullOrWhiteSpace(x)))
        {
            if (!TryParseRange(mergeReference!, out var startRow, out var startColumn, out var endRow, out var endColumn))
            {
                continue;
            }

            var topLeftValue = GetCellValue(rows, startRow, startColumn);
            if (string.IsNullOrWhiteSpace(topLeftValue))
            {
                continue;
            }

            for (var row = startRow; row <= endRow; row++)
            {
                EnsureRowCapacity(rows, row, endColumn);
                for (var column = startColumn; column <= endColumn; column++)
                {
                    if (string.IsNullOrWhiteSpace(rows[row - 1][column - 1]))
                    {
                        rows[row - 1][column - 1] = topLeftValue;
                    }
                }
            }
        }
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
            throw new ImportException($"Coluna obrigatória ausente: {name}.");
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

    private static string GetCellValue(List<string[]> rows, int row, int column)
    {
        if (row <= 0 || row > rows.Count)
        {
            return "";
        }

        var cells = rows[row - 1];
        return column > 0 && column <= cells.Length ? cells[column - 1] ?? "" : "";
    }

    private static void EnsureRowCapacity(List<string[]> rows, int row, int columnCount)
    {
        while (rows.Count < row)
        {
            rows.Add(Array.Empty<string>());
        }

        var values = rows[row - 1];
        if (values.Length >= columnCount)
        {
            return;
        }

        var originalLength = values.Length;
        Array.Resize(ref values, columnCount);
        for (var i = originalLength; i < values.Length; i++)
        {
            values[i] = "";
        }

        rows[row - 1] = values;
    }

    private static bool TryParseRange(string reference, out int startRow, out int startColumn, out int endRow, out int endColumn)
    {
        startRow = 0;
        startColumn = 0;
        endRow = 0;
        endColumn = 0;

        var parts = reference.Split(':', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0 || parts.Length > 2)
        {
            return false;
        }

        if (!TryParseCellReference(parts[0], out startRow, out startColumn))
        {
            return false;
        }

        if (parts.Length == 1)
        {
            endRow = startRow;
            endColumn = startColumn;
            return true;
        }

        if (!TryParseCellReference(parts[1], out endRow, out endColumn))
        {
            return false;
        }

        return endRow >= startRow && endColumn >= startColumn;
    }

    private static bool TryParseCellReference(string reference, out int row, out int column)
    {
        row = 0;
        column = ColumnIndex(reference);
        if (column <= 0)
        {
            return false;
        }

        var letters = ColumnLetters().Match(reference).Value;
        var numberPart = reference[letters.Length..];
        return int.TryParse(numberPart, out row) && row > 0;
    }

    private static void EnsureZipSignature(Stream stream)
    {
        var header = new byte[4];
        _ = stream.Read(header, 0, header.Length);
        var isZip =
            header.SequenceEqual(new byte[] { 0x50, 0x4B, 0x03, 0x04 }) ||
            header.SequenceEqual(new byte[] { 0x50, 0x4B, 0x05, 0x06 }) ||
            header.SequenceEqual(new byte[] { 0x50, 0x4B, 0x07, 0x08 });

        if (!isZip)
        {
            throw new ImportException("O arquivo informado não possui assinatura válida de XLSX/XLSM.");
        }
    }

    private static void EnsureTextPayload(Stream stream)
    {
        var max = (int)Math.Min(stream.Length, 512);
        var buffer = new byte[max];
        _ = stream.Read(buffer, 0, buffer.Length);
        if (buffer.Any(b => b == 0))
        {
            throw new ImportException("O arquivo CSV/TXT parece binário ou corrompido.");
        }
    }

    private static XDocument LoadXml(Stream stream)
    {
        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null
        };

        using var reader = XmlReader.Create(stream, settings);
        return XDocument.Load(reader);
    }

    [GeneratedRegex(@"^[A-Za-z]+")]
    private static partial Regex ColumnLetters();
}
