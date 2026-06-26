using ClubeDasOfertas.Web.Domain;
using ClubeDasOfertas.Web.Services;
using Microsoft.AspNetCore.Http;
using System.IO.Compression;
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
Assert(PermissionMatrix.IsAdmin(Roles.Admin), "Administrador deve ser reconhecido como admin.");
Assert(PermissionMatrix.CanOperateCampaigns(Roles.Admin), "Administrador deve poder operar campanhas.");
Assert(PermissionMatrix.CanOperateCampaigns(Roles.Operator), "Operador deve poder operar campanhas.");
Assert(!PermissionMatrix.CanOperateCampaigns(Roles.User), "Usuário limitado não deve poder operar campanhas.");
Assert(PermissionMatrix.CanManageAdminAreas(Roles.Admin), "Administrador deve poder acessar áreas administrativas.");
Assert(!PermissionMatrix.CanManageAdminAreas(Roles.Operator) && !PermissionMatrix.CanManageAdminAreas(Roles.User), "Perfis não administradores não devem acessar áreas administrativas.");

var importer = new SpreadsheetImporter();
var weightedRule = NewRule("Pesaveis", RuleTypes.Weighted, @"\b(ALHO|MUSSARELA|100\s*G)\b", 10m, "Kg", true);
var fardoRule = NewRule("Fardos", RuleTypes.PackageBale, @"\b(FD|FARDO|FARDOS)\b", 2m, "Unidades", true);
var caixaRule = NewRule("Caixas perfumaria", RuleTypes.PackageBox, @"\b(CX/?\d+|C/?\d+|C\s+\d+|CAIXA|CAIXAS)\b", 3m, "Unidades", true, "Perfumaria");

var friendlyRule = RulePatternConverter.Convert("fardo ou caixa");
Assert(friendlyRule.Pattern == @"(?: (?<!\w)FARDO(?!\w)|(?<!\w)CAIXA(?!\w))".Replace(" ", ""), "Conversor deve transformar alternativas escritas pelo usuario em regex.");

var pipeSeparatedRule = RulePatternConverter.Convert("fardo | caixa");
Assert(pipeSeparatedRule.Pattern == friendlyRule.Pattern, "Separador com pipe deve continuar no fluxo amigavel, sem virar regex bruto.");

var commaSeparatedRule = RulePatternConverter.Convert("fardo, caixa");
Assert(commaSeparatedRule.Pattern == friendlyRule.Pattern, "Separador com virgula deve gerar as mesmas alternativas amigaveis.");

var slashSeparatedWordsRule = RulePatternConverter.Convert("fd/fardo");
Assert(slashSeparatedWordsRule.Pattern == @"(?: (?<!\w)FD(?!\w)|(?<!\w)FARDO(?!\w))".Replace(" ", ""), "Barra entre palavras deve ser entendida como alternativa amigavel.");

var wildcardRule = RulePatternConverter.Convert("cx/*");
Assert(wildcardRule.Pattern == @"(?<!\w)CX\s*/\s*.*(?!\w)", "Conversor deve respeitar coringa e separadores comuns.");

var regexLiteralRule = RulePatternConverter.Convert(@"\b(FD|FARDO)\b");
Assert(regexLiteralRule.IsRegexLiteral && regexLiteralRule.Pattern == @"\b(FD|FARDO)\b", "Regex literal informado pelo usuario deve ser preservado.");

var semanticSolidusRule = RulePatternConverter.Convert("Se conter a palavra CERV na descrição solidus, aplicar a regra");
Assert(semanticSolidusRule.Pattern == @"(?<!\w)CERV(?!\w)", "Frase semantica para Solidus deve extrair apenas o criterio util.");

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
        ex => ex.Message.Contains("Abas disponíveis:", StringComparison.OrdinalIgnoreCase),
        "A importacao deve informar as abas disponiveis quando a aba escolhida nao existir.");
}

await using (var stream = CreateMergedCampaignWorkbook())
{
    var file = new FormFile(stream, 0, stream.Length, "file", "mesclado.xlsx");
    var mergedRows = await importer.ReadCampaignRowsAsync(file);
    Assert(mergedRows.Count == 2, "Workbook de teste com merge deve produzir duas linhas de campanha.");
    Assert(mergedRows[0].Source == "Tabloide" && mergedRows[1].Source == "Tabloide", "Celulas mescladas da fonte devem propagar o valor para todas as linhas.");
    Assert(mergedRows[1].OriginalVigency == "VIGENCIA 05 A 08", "Celulas mescladas da vigencia devem propagar o valor para todas as linhas.");
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
Assert(packageItem.Quantity == 10m, "Fardo deve dividir a quantidade pelo multiplicador.");
Assert(packageItem.PriceSaleRaw == "41,94" && packageItem.PriceClubRaw == "35,94", "Edicao deve preservar os precos originais informados.");
Assert(packageItem.FinalPriceSale == 83.88m && packageItem.FinalPriceClub == 71.88m, "Fardo deve poder aplicar multiplicador proprio.");

var semanticSolidusConversionRule = NewRule(
    "Cervejas no solidus",
    RuleTypes.PackageBale,
    semanticSolidusRule.Pattern,
    2m,
    "Unidades",
    true,
    patternInput: "Se conter a palavra CERV na descrição solidus, aplicar a regra");

var semanticSolidusItem = CampaignImportService.EvaluateItem(
    Guid.NewGuid(),
    Guid.NewGuid(),
    new RawCampaignRow(42, "App", "VIGENCIA 05 A 08", "Produto generico", "10 Unidades", "10,00", "8,00"),
    NewCatalog("Produto generico", "CERV LONG NECK FD C/6", "7891991304887"),
    [semanticSolidusConversionRule]);

Assert(semanticSolidusItem.RiskFlags.Contains("FARDO_CAIXA"), "Regra semantica apontando para Solidus deve casar quando o texto estiver apenas na descricao Solidus.");
Assert(semanticSolidusItem.BlockingReasons.Contains("Fardo pendente"), "Regra semantica para Solidus deve acionar revisao quando configurada para isso.");
Assert(semanticSolidusItem.FinalPriceSale == 20.00m && semanticSolidusItem.FinalPriceClub == 16.00m, "Regra semantica para Solidus deve aplicar o multiplicador configurado.");

var packageItemWithMath = CampaignImportService.EvaluateItem(
    Guid.NewGuid(),
    Guid.NewGuid(),
    new RawCampaignRow(41, "App", "VIGENCIA 05 A 08", "Cerveja Long Neck 330ml", "20*6 Unidades", "83,88/6", "71,88/6"),
    NewCatalog("Cerveja Long Neck 330ml", "CERV LONG NECK FD C/6", "7891991304887"),
    [fardoRule]);

Assert(packageItemWithMath.Quantity == 60m && packageItemWithMath.Unit == "Unidades", "Fardo deve dividir a quantidade pelo multiplicador mesmo com conta no campo.");
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

var exportCampaign = new Campaign(Guid.NewGuid(), "Campanha teste", new DateOnly(2026, 6, 5), new DateOnly(2026, 6, 8), CampaignStatus.Imported, Guid.NewGuid(), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
var exportCsv = ExportService.BuildCsv(exportCampaign, [weightedItem, packageItem]);
Assert(exportCsv.StartsWith("codigo_barras;preco_venda;preco_clube;quantidade;unidade;fonte;", StringComparison.Ordinal), "Cabecalho do CSV deve manter as colunas originais do CRM.");
Assert(exportCsv.Contains("status_item;status_revisao;revisao_obrigatoria;riscos;pendencias;linha_origem;vigencia_original;preco_original_venda;preco_original_clube", StringComparison.Ordinal), "Cabecalho do CSV deve incluir o contexto operacional novo.");
Assert(exportCsv.Contains("Tabloide", StringComparison.Ordinal) && exportCsv.Contains("App", StringComparison.Ordinal), "CSV deve manter a fonte do item para diferenciar tabloide e aplicativo.");
Assert(exportCsv.Contains("Conversao de pesavel pendente", StringComparison.Ordinal) && exportCsv.Contains("Fardo pendente", StringComparison.Ordinal), "CSV deve exportar as pendencias dos itens.");
Assert(exportCsv.Contains("PESAVEL", StringComparison.Ordinal) && exportCsv.Contains("FARDO_CAIXA", StringComparison.Ordinal), "CSV deve exportar os riscos dos itens.");

var manualItem = weightedItem with
{
    Source = "Manual",
    OriginalVigency = "05/06/2026 a 08/06/2026",
    SourceRow = 99
};

var manualExportCsv = ExportService.BuildCsv(exportCampaign, [manualItem]);
Assert(manualExportCsv.Contains(";Manual;", StringComparison.Ordinal), "CSV deve preservar a fonte Manual dos itens incluidos manualmente.");
Assert(manualExportCsv.Contains("05/06/2026 a 08/06/2026", StringComparison.Ordinal), "CSV deve preservar a vigencia original dos itens incluidos manualmente.");

var subsetExportCsv = ExportService.BuildCsv(exportCampaign, [manualItem], ["codigo_barras", "fonte", "descricao_solidus"]);
Assert(subsetExportCsv.StartsWith("codigo_barras;fonte;descricao_solidus", StringComparison.Ordinal), "CSV personalizado deve manter apenas as colunas selecionadas, na ordem padrao da exportacao.");
Assert(!subsetExportCsv.Contains("status_item", StringComparison.Ordinal), "CSV personalizado nao deve incluir colunas que nao foram selecionadas.");

var lojasPresetColumns = ExportService.GetPresetColumns("lojas");
var lojasExportCsv = ExportService.BuildCsv(exportCampaign, [manualItem], lojasPresetColumns);
Assert(lojasExportCsv.Contains("fonte;", StringComparison.Ordinal)
    && lojasExportCsv.Contains("vigencia_original;", StringComparison.Ordinal)
    && lojasExportCsv.Contains("descricao_tabloide;", StringComparison.Ordinal)
    && lojasExportCsv.Contains("descricao_solidus;", StringComparison.Ordinal)
    && lojasExportCsv.Contains("codigo_barras", StringComparison.Ordinal), "Preset de lojas deve selecionar o bloco operacional equivalente ao envio para lojas.");
Assert(!lojasExportCsv.Contains("status_item", StringComparison.Ordinal) && !lojasExportCsv.Contains("tipo_codigo", StringComparison.Ordinal), "Preset de lojas nao deve incluir colunas internas do CRM.");

var itemWithInvalidCharacters = weightedItem with
{
    DescriptionTabloid = "Oferta\u0001 com controle",
    DescriptionSolidus = "SOLIDUS\u0002 TESTE"
};

var lojasWorkbook = ExportService.BuildStoreWorkbook(exportCampaign, [itemWithInvalidCharacters, weightedItem with { Barcode = "7890000000999" }]);
using (var workbookStream = new MemoryStream(lojasWorkbook))
using (var archive = new ZipArchive(workbookStream, ZipArchiveMode.Read, leaveOpen: false))
{
    var workbookXml = ReadEntry(archive, "xl/workbook.xml");
    var sheetXml = ReadEntry(archive, "xl/worksheets/sheet1.xml");
    Assert(workbookXml.Contains("Tabloide - 05.06.2026", StringComparison.Ordinal), "XLSX de lojas deve criar a aba com referencia de tabloide e data.");
    Assert(sheetXml.Contains("Clube Das Ofertas - Campanha teste", StringComparison.Ordinal), "XLSX de lojas deve trazer o titulo visual da campanha.");
    Assert(sheetXml.Contains("Descricao no Tabloide", StringComparison.Ordinal) && sheetXml.Contains("Cod Barras", StringComparison.Ordinal), "XLSX de lojas deve conter o bloco operacional de A ate H.");
    Assert(sheetXml.Contains("<mergeCell ref=\"A3:A4\"/>", StringComparison.Ordinal), "XLSX de lojas deve mesclar fonte repetida para lembrar a planilha base.");
    Assert(sheetXml.IndexOf("<autoFilter", StringComparison.Ordinal) < sheetXml.IndexOf("<mergeCells", StringComparison.Ordinal), "XLSX de lojas deve manter autoFilter antes de mergeCells para evitar reparo no Excel.");
    Assert(!sheetXml.Contains("\u0001", StringComparison.Ordinal) && !sheetXml.Contains("\u0002", StringComparison.Ordinal), "XLSX de lojas nao deve escrever caracteres invalidos de XML nas celulas.");
}

var internalWorkbook = ExportService.BuildInternalWorkbook(
    exportCampaign,
    [weightedItem, packageItem],
    new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
    {
        [weightedItem.NormalizedDescriptionTabloid] = 1,
        [packageItem.NormalizedDescriptionTabloid] = 2
    });
using (var workbookStream = new MemoryStream(internalWorkbook))
using (var archive = new ZipArchive(workbookStream, ZipArchiveMode.Read, leaveOpen: false))
{
    var workbookXml = ReadEntry(archive, "xl/workbook.xml");
    var sheetXml = ReadEntry(archive, "xl/worksheets/sheet1.xml");
    Assert(workbookXml.Contains("Interno - 05.06.2026", StringComparison.Ordinal), "XLSX interno deve criar aba propria para o uso interno e CRM.");
    Assert(sheetXml.Contains("Codigo Unificado ou EAN", StringComparison.Ordinal), "XLSX interno deve espelhar os parametros auxiliares da planilha original em J1.");
    Assert(sheetXml.Contains("Ao enviar as lojas, enviar da coluna A ate H.", StringComparison.Ordinal), "XLSX interno deve manter o recado operacional da planilha base.");
    Assert(sheetXml.Contains("2. Item esta nas ofertas do tabloide?", StringComparison.Ordinal), "XLSX interno deve manter a cabecalho de controle da coluna N.");
    Assert(sheetXml.Contains("7896864400031;24,90;19,90;5", StringComparison.Ordinal), "XLSX interno deve preencher o concatenado usado no cadastro do CRM.");
    Assert(sheetXml.Contains("Bloqueado", StringComparison.Ordinal), "XLSX interno deve preencher a coluna de status com o estado atual do item.");
    Assert(sheetXml.Contains(">1</t>", StringComparison.Ordinal) && sheetXml.Contains(">2</t>", StringComparison.Ordinal), "XLSX interno deve levar a contagem de correspondencias do catalogo para a coluna K.");
}

AssertThrows<InvalidOperationException>(() => ExportService.BuildCsv(exportCampaign, [manualItem], []), "Exportacao sem nenhuma coluna deve ser rejeitada.");

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

static void AssertThrows<TException>(Action action, string message)
    where TException : Exception
{
    try
    {
        action();
    }
    catch (TException)
    {
        return;
    }

    throw new InvalidOperationException(message);
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

static ConversionRule NewRule(string name, string type, string pattern, decimal multiplier, string targetUnit, bool requiresReview, string categoryScope = "", string? patternInput = null)
{
    var now = DateTimeOffset.UtcNow;
    return new ConversionRule(Guid.NewGuid(), name, type, patternInput ?? pattern, pattern, multiplier, targetUnit, categoryScope, requiresReview, true, now, now);
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

static MemoryStream CreateMergedCampaignWorkbook()
{
    var stream = new MemoryStream();
    using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
    {
        WriteEntry(archive, "[Content_Types].xml", """
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
  <Default Extension="xml" ContentType="application/xml"/>
  <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
  <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
</Types>
""");
        WriteEntry(archive, "_rels/.rels", """
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
</Relationships>
""");
        WriteEntry(archive, "xl/workbook.xml", """
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
  <sheets>
    <sheet name="Base Clube - CLT" sheetId="1" r:id="rId1"/>
  </sheets>
</workbook>
""");
        WriteEntry(archive, "xl/_rels/workbook.xml.rels", """
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
</Relationships>
""");
        WriteEntry(archive, "xl/worksheets/sheet1.xml", """
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
  <sheetData>
    <row r="1">
      <c r="A1" t="inlineStr"><is><t>FONTE</t></is></c>
      <c r="B1" t="inlineStr"><is><t>VIGENCIA</t></is></c>
      <c r="C1" t="inlineStr"><is><t>DESCRICAO NO TABLOIDE</t></is></c>
      <c r="D1" t="inlineStr"><is><t>QUANTIDADE LIMITADA</t></is></c>
      <c r="E1" t="inlineStr"><is><t>VENDA</t></is></c>
      <c r="F1" t="inlineStr"><is><t>VENDA CLUBE</t></is></c>
    </row>
    <row r="2">
      <c r="A2" t="inlineStr"><is><t>Tabloide</t></is></c>
      <c r="B2" t="inlineStr"><is><t>VIGENCIA 05 A 08</t></is></c>
      <c r="C2" t="inlineStr"><is><t>Arroz tipo 1</t></is></c>
      <c r="D2" t="inlineStr"><is><t>1 Unidades</t></is></c>
      <c r="E2" t="inlineStr"><is><t>10,00</t></is></c>
      <c r="F2" t="inlineStr"><is><t>9,00</t></is></c>
    </row>
    <row r="3">
      <c r="C3" t="inlineStr"><is><t>Feijao carioca</t></is></c>
      <c r="D3" t="inlineStr"><is><t>2 Unidades</t></is></c>
      <c r="E3" t="inlineStr"><is><t>12,00</t></is></c>
      <c r="F3" t="inlineStr"><is><t>11,00</t></is></c>
    </row>
  </sheetData>
  <mergeCells count="2">
    <mergeCell ref="A2:A3"/>
    <mergeCell ref="B2:B3"/>
  </mergeCells>
</worksheet>
""");
    }

    stream.Position = 0;
    return stream;
}

static void WriteEntry(ZipArchive archive, string path, string content)
{
    var entry = archive.CreateEntry(path);
    using var writer = new StreamWriter(entry.Open(), new UTF8Encoding(false));
    writer.Write(content);
}

static string ReadEntry(ZipArchive archive, string path)
{
    var entry = archive.GetEntry(path) ?? throw new InvalidOperationException($"Entrada '{path}' nao encontrada no arquivo.");
    using var reader = new StreamReader(entry.Open(), Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
    return reader.ReadToEnd();
}
