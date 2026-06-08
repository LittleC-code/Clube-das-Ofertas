using ClubeDasOfertas.Web.Domain;
using ClubeDasOfertas.Web.Services;
using Microsoft.AspNetCore.Http;
using System.Text;

var root = FindRepoRoot(AppContext.BaseDirectory);
var workbookPath = Path.Combine(root, "CHECK LIST - Clube Das Ofertas.xlsm");

Assert(File.Exists(workbookPath), "Planilha de exemplo deve existir no workspace.");

Assert(Parsing.TryMoney(" R$        2,49 ", out var sale) && sale == 2.49m, "Parser deve ler moeda pt-BR.");
Assert(Parsing.TryMoney("18,99", out var sale2) && sale2 == 18.99m, "Parser deve ler decimal com virgula.");
Assert(Parsing.TryMoney("83,88/6", out var sale3) && sale3 == 13.98m, "Parser deve avaliar contas em campos de preco.");

var quantityKg = Parsing.ParseQuantity("05Kg");
Assert(quantityKg.IsValid && quantityKg.Quantity == 5m && quantityKg.Unit == "Kg", "Quantidade 05Kg deve virar 5 Kg.");

var quantityUnits = Parsing.ParseQuantity("12 Unidades");
Assert(quantityUnits.IsValid && quantityUnits.Quantity == 12m && quantityUnits.Unit == "Unidades", "Quantidade em unidades deve ser reconhecida.");

var quantityExpression = Parsing.ParseQuantity("20*6 Unidades");
Assert(quantityExpression.IsValid && quantityExpression.Quantity == 120m && quantityExpression.Unit == "Unidades", "Quantidade deve aceitar contas na edicao manual.");

var quantityExpressionWithX = Parsing.ParseQuantity("20x6 Unidades");
Assert(quantityExpressionWithX.IsValid && quantityExpressionWithX.Quantity == 120m && quantityExpressionWithX.Unit == "Unidades", "Quantidade deve aceitar contas com x na edicao manual.");

var quantityExpressionWithUnitBetween = Parsing.ParseQuantity("20 Unidades / 6");
Assert(quantityExpressionWithUnitBetween.IsValid && Math.Round(quantityExpressionWithUnitBetween.Quantity, 3) == 3.333m && quantityExpressionWithUnitBetween.Unit == "Unidades", "Quantidade deve aceitar divisao mesmo com a unidade escrita no meio da conta.");

var quantityExpressionWithCaixas = Parsing.ParseQuantity("120 caixas / 6");
Assert(quantityExpressionWithCaixas.IsValid && quantityExpressionWithCaixas.Quantity == 20m && quantityExpressionWithCaixas.Unit == "Caixas", "Quantidade deve aceitar caixas no meio da conta.");

var quantityExpressionWithFardos = Parsing.ParseQuantity("120 fardos / 6");
Assert(quantityExpressionWithFardos.IsValid && quantityExpressionWithFardos.Quantity == 20m && quantityExpressionWithFardos.Unit == "Fardos", "Quantidade deve aceitar fardos no meio da conta.");

var quantityOnlyBoxes = Parsing.ParseQuantity("Caixas");
Assert(quantityOnlyBoxes.IsValid && quantityOnlyBoxes.Quantity == 1m && quantityOnlyBoxes.Unit == "Caixas", "Quantidade deve aceitar texto puro como Caixas.");

var quantityOnlyBales = Parsing.ParseQuantity("Fardos");
Assert(quantityOnlyBales.IsValid && quantityOnlyBales.Quantity == 1m && quantityOnlyBales.Unit == "Fardos", "Quantidade deve aceitar texto puro como Fardos.");

Assert(TextNormalizer.NormalizeKey("Descri\u00E7\u00E3o Solidus") == "DESCRICAO SOLIDUS", "Normalizacao deve remover acentos.");
Assert(Parsing.CodeType("99738") == "Codigo Unificado", "Codigo curto deve ser classificado como Codigo Unificado.");
Assert(Parsing.CodeType("7896864400031") == "EAN", "Codigo longo deve ser classificado como EAN.");

var importer = new SpreadsheetImporter();
var weightedRule = NewRule("Pesaveis", RuleTypes.Weighted, @"\b(ALHO|MUSSARELA|100\s*G)\b", 10m, "Kg", true);
var fardoRule = NewRule("Fardos", RuleTypes.PackageBale, @"\b(FD|FARDO|FARDOS)\b", 2m, "Unidades", true);
var caixaRule = NewRule("Caixas perfumaria", RuleTypes.PackageBox, @"\b(CX/?\d+|C/?\d+|C\s+\d+|CAIXA|CAIXAS)\b", 3m, "Unidades", true, "Perfumaria");

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
    var worksheetNames = await importer.ListWorksheetNamesAsync(file);
    Assert(worksheetNames.Contains(SpreadsheetImporter.DefaultCampaignSheetName), "Workbook deve expor a aba padrao de campanha.");
}

await using (var stream = File.OpenRead(workbookPath))
{
    var file = new FormFile(stream, 0, stream.Length, "file", Path.GetFileName(workbookPath));
    var campaignRows = await importer.ReadCampaignRowsAsync(file);
    Assert(campaignRows.Count >= 40, "Aba Base Clube - CLT deve produzir linhas de campanha.");
    Assert(campaignRows.Any(x => x.DescriptionTabloid.Contains("Alho", StringComparison.OrdinalIgnoreCase)), "Importacao deve preservar descricoes de itens.");
    Assert(Parsing.TryMoney(campaignRows[0].PriceSaleRaw, out var importedSale) && importedSale == 2.49m, "Valores numericos de XLSM devem ser arredondados para moeda.");
}

await using (var stream = File.OpenRead(workbookPath))
{
    var file = new FormFile(stream, 0, stream.Length, "file", Path.GetFileName(workbookPath));
    await AssertThrowsWhereAsync<ImportException>(
        () => importer.ReadCampaignRowsAsync(file, "Aba inexistente"),
        ex => ex.Message.Contains("Abas disponiveis:", StringComparison.OrdinalIgnoreCase),
        "A importacao deve informar as abas disponiveis quando a aba escolhida nao existir.");
}

var weightedItem = CampaignImportService.EvaluateItem(
    Guid.NewGuid(),
    Guid.NewGuid(),
    new RawCampaignRow(2, "Tabloide", "VIGENCIA 05 A 08", "Alho a Granel cada 100g", "05Kg", "2,49", "1,99"),
    NewCatalog("Alho a Granel cada 100g", "ALHO A GRANEL", "99738"),
    [weightedRule]);

Assert(weightedItem.FinalPriceSale == 24.90m && weightedItem.FinalPriceClub == 19.90m, "Pesaveis devem multiplicar por 10.");
Assert(weightedItem.Unit == "Kg", "Pesaveis devem manter ou definir unidade Kg.");
Assert(weightedItem.ReviewStatus == ReviewStatus.Pending, "Pesaveis com revisao obrigatoria devem ficar pendentes.");
Assert(weightedItem.BlockingReasons.Contains("Conversao de pesavel pendente"), "Pesaveis devem gerar bloqueio de revisao.");

var kgItemWithoutHundredGram = CampaignImportService.EvaluateItem(
    Guid.NewGuid(),
    Guid.NewGuid(),
    new RawCampaignRow(3, "Tabloide", "VIGENCIA 05 A 08", "Queijo Mussarela Kg", "05Kg", "39,90", "34,90"),
    NewCatalog("Queijo Mussarela Kg", "QUEIJO MUSSARELA KG", "7890000000001"),
    [weightedRule]);

Assert(kgItemWithoutHundredGram.FinalPriceSale == 39.90m, "Item ja em Kg nao deve ser multiplicado sem regra de 100g.");
Assert(!kgItemWithoutHundredGram.RiskFlags.Contains("PESAVEL"), "Item ja em Kg sem 100g nao deve ser sinalizado como pesavel convertido.");

var packageItem = CampaignImportService.EvaluateItem(
    Guid.NewGuid(),
    Guid.NewGuid(),
    new RawCampaignRow(4, "App", "VIGENCIA 05 A 08", "Cerveja Long Neck 330ml", "20 Fardos", "41,94", "35,94"),
    NewCatalog("Cerveja Long Neck 330ml", "CERV LONG NECK FD C/6", "7891991304887"),
    [fardoRule]);

Assert(packageItem.RiskFlags.Contains("FARDO_CAIXA"), "Fardo/caixa deve ser detectado.");
Assert(packageItem.RiskFlags.Contains("FARDO"), "Fardo deve receber flag especifica.");
Assert(packageItem.BlockingReasons.Contains("Fardo pendente"), "Fardo deve bloquear exportacao ate revisao.");
Assert(packageItem.PriceSaleRaw == "41,94" && packageItem.PriceClubRaw == "35,94", "Edicao deve preservar os precos originais informados.");
Assert(packageItem.FinalPriceSale == 83.88m && packageItem.FinalPriceClub == 71.88m, "Fardo deve poder aplicar multiplicador proprio.");

var packageItemWithMath = CampaignImportService.EvaluateItem(
    Guid.NewGuid(),
    Guid.NewGuid(),
    new RawCampaignRow(41, "App", "VIGENCIA 05 A 08", "Cerveja Long Neck 330ml", "20*6 Unidades", "83,88/6", "71,88/6"),
    NewCatalog("Cerveja Long Neck 330ml", "CERV LONG NECK FD C/6", "7891991304887"),
    [fardoRule]);

Assert(packageItemWithMath.Quantity == 120m && packageItemWithMath.Unit == "Unidades", "Fardo deve aceitar conta no campo de quantidade.");
Assert(packageItemWithMath.PriceSaleRaw == "83,88/6" && packageItemWithMath.PriceClubRaw == "71,88/6", "Edicao deve preservar a expressao da conta dos precos.");
Assert(packageItemWithMath.FinalPriceSale == 27.96m && packageItemWithMath.FinalPriceClub == 23.96m, "Fardo deve reaplicar a regra depois de calcular os precos informados.");

var caixaItem = CampaignImportService.EvaluateItem(
    Guid.NewGuid(),
    Guid.NewGuid(),
    new RawCampaignRow(5, "App", "VIGENCIA 05 A 08", "Absorvente basico", "12 Unidades", "10,00", "8,00"),
    NewCatalog("Absorvente basico", "ABS SUAVE CX/12", "7890000000002", "Perfumaria"),
    [caixaRule]);

Assert(caixaItem.RiskFlags.Contains("CAIXA"), "Caixa deve receber flag especifica.");
Assert(caixaItem.BlockingReasons.Contains("Caixa pendente"), "Caixa deve bloquear exportacao ate revisao.");
Assert(caixaItem.FinalPriceSale == 30.00m && caixaItem.FinalPriceClub == 24.00m, "Caixa deve poder aplicar multiplicador proprio.");

var caixaCategoriaErrada = CampaignImportService.EvaluateItem(
    Guid.NewGuid(),
    Guid.NewGuid(),
    new RawCampaignRow(6, "App", "VIGENCIA 05 A 08", "Absorvente basico", "12 Unidades", "10,00", "8,00"),
    NewCatalog("Absorvente basico", "ABS SUAVE CX/12", "7890000000003", "Mercearia"),
    [caixaRule]);

Assert(!caixaCategoriaErrada.RiskFlags.Contains("CAIXA"), "Regra de caixa com categoria alvo nao deve aplicar fora da categoria.");

var duplicateItems = CampaignImportService.MarkDuplicateBarcodes(
[
    weightedItem with { Barcode = "7891" },
    packageItem with { Barcode = "7891" }
]);
Assert(duplicateItems.All(x => x.RiskFlags.Contains("DUPLICIDADE")), "Codigos repetidos devem ser marcados como duplicidade.");

var duplicateClearedItems = CampaignImportService.MarkDuplicateBarcodes(
[
    duplicateItems[0] with { Barcode = "7891" },
    duplicateItems[1] with { Barcode = "7892" }
]);
Assert(duplicateClearedItems.All(x => !x.RiskFlags.Contains("DUPLICIDADE")), "Duplicidade antiga deve ser removida quando os codigos deixam de colidir.");

await using (var stream = new MemoryStream([0x50, 0x4B, 0x03, 0x04]))
{
    var file = new FormFile(stream, 0, SpreadsheetImporter.MaxUploadBytes + 1, "file", "grande.xlsx");
    await AssertThrowsAsync<ImportException>(() => importer.ReadCampaignRowsAsync(file), "Arquivo acima do limite deve ser rejeitado.");
}

await using (var stream = new MemoryStream(Encoding.UTF8.GetBytes("not-a-zip")))
{
    var file = new FormFile(stream, 0, stream.Length, "file", "invalido.xlsx");
    await AssertThrowsAsync<ImportException>(() => importer.ReadCampaignRowsAsync(file), "XLSX com assinatura invalida deve ser rejeitado.");
}

await using (var stream = new MemoryStream([65, 0, 66]))
{
    var file = new FormFile(stream, 0, stream.Length, "file", "binario.csv");
    await AssertThrowsAsync<ImportException>(() => importer.ReadCampaignRowsAsync(file), "CSV binario deve ser rejeitado.");
}

Console.WriteLine("All tests passed.");

static void Assert(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}

static async Task AssertThrowsAsync<TException>(Func<Task> action, string message)
    where TException : Exception
{
    try
    {
        await action();
    }
    catch (TException)
    {
        return;
    }

    throw new InvalidOperationException(message);
}

static async Task AssertThrowsWhereAsync<TException>(Func<Task> action, Func<TException, bool> predicate, string message)
    where TException : Exception
{
    try
    {
        await action();
    }
    catch (TException ex) when (predicate(ex))
    {
        return;
    }
    catch (TException)
    {
        throw new InvalidOperationException(message);
    }

    throw new InvalidOperationException(message);
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

static ConversionRule NewRule(string name, string type, string pattern, decimal multiplier, string targetUnit, bool requiresReview, string categoryScope = "")
{
    var now = DateTimeOffset.UtcNow;
    return new ConversionRule(Guid.NewGuid(), name, type, pattern, multiplier, targetUnit, categoryScope, requiresReview, true, now, now);
}

static ProductCatalogEntry NewCatalog(string descriptionTabloid, string descriptionSolidus, string barcode, string category = "Categoria")
{
    var now = DateTimeOffset.UtcNow;
    return new ProductCatalogEntry(
        Guid.NewGuid(),
        descriptionTabloid,
        TextNormalizer.NormalizeKey(descriptionTabloid),
        category,
        descriptionSolidus,
        TextNormalizer.NormalizeKey(descriptionSolidus),
        barcode,
        Parsing.CodeType(barcode),
        now,
        now);
}
