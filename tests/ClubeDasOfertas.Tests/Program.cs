using ClubeDasOfertas.Web.Services;
using Microsoft.AspNetCore.Http;

var root = FindRepoRoot(AppContext.BaseDirectory);
var workbookPath = Path.Combine(root, "CHECK LIST - Clube Das Ofertas.xlsm");

Assert(File.Exists(workbookPath), "Planilha de exemplo deve existir no workspace.");

Assert(Parsing.TryMoney(" R$        2,49 ", out var sale) && sale == 2.49m, "Parser deve ler moeda pt-BR.");
Assert(Parsing.TryMoney("18,99", out var sale2) && sale2 == 18.99m, "Parser deve ler decimal com virgula.");

var quantityKg = Parsing.ParseQuantity("05Kg");
Assert(quantityKg.IsValid && quantityKg.Quantity == 5m && quantityKg.Unit == "Kg", "Quantidade 05Kg deve virar 5 Kg.");

var quantityUnits = Parsing.ParseQuantity("12 Unidades");
Assert(quantityUnits.IsValid && quantityUnits.Quantity == 12m && quantityUnits.Unit == "Unidades", "Quantidade em unidades deve ser reconhecida.");

Assert(TextNormalizer.NormalizeKey("Descrição Solidus") == "DESCRICAO SOLIDUS", "Normalizacao deve remover acentos.");
Assert(Parsing.CodeType("99738") == "Codigo Unificado", "Codigo curto deve ser classificado como Codigo Unificado.");
Assert(Parsing.CodeType("7896864400031") == "EAN", "Codigo longo deve ser classificado como EAN.");

var importer = new SpreadsheetImporter();

await using (var stream = File.OpenRead(workbookPath))
{
    var file = new FormFile(stream, 0, stream.Length, "file", Path.GetFileName(workbookPath));
    var catalogRows = await importer.ReadCatalogRowsAsync(file);
    Assert(catalogRows.Count > 2500, "Catalogo importado da aba Base - Cod Barras deve ter milhares de linhas.");
    Assert(catalogRows.Any(x => x.DescriptionSolidus.Contains("ARROZ", StringComparison.OrdinalIgnoreCase)), "Catalogo deve conter descricoes Solidus.");
}

await using (var stream = File.OpenRead(workbookPath))
{
    var file = new FormFile(stream, 0, stream.Length, "file", Path.GetFileName(workbookPath));
    var campaignRows = await importer.ReadCampaignRowsAsync(file);
    Assert(campaignRows.Count >= 40, "Aba Base Clube - CLT deve produzir linhas de campanha.");
    Assert(campaignRows.Any(x => x.DescriptionTabloid.Contains("Alho", StringComparison.OrdinalIgnoreCase)), "Importacao deve preservar descricoes de itens.");
    Assert(Parsing.TryMoney(campaignRows[0].PriceSaleRaw, out var importedSale) && importedSale == 2.49m, "Valores numericos de XLSM devem ser arredondados para moeda.");
}

Console.WriteLine("All tests passed.");

static void Assert(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}

static string FindRepoRoot(string start)
{
    var directory = new DirectoryInfo(start);
    while (directory is not null)
    {
        if (File.Exists(Path.Combine(directory.FullName, "CHECK LIST - Clube Das Ofertas.xlsm")))
        {
            return directory.FullName;
        }

        directory = directory.Parent;
    }

    throw new DirectoryNotFoundException("Nao encontrei a raiz do repositorio.");
}
