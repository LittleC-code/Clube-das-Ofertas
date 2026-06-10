# Spec - exportacao flexivel com fonte preservada

## Objetivo

Permitir exportar a campanha mesmo quando ainda existirem itens com pendencias de validacao, avisando o operador antes da acao, enriquecendo o CSV com contexto operacional e corrigindo a leitura de planilhas prontas quando a coluna de fonte vier em celulas mescladas.

## Arquivos esperados no escopo

- `src/ClubeDasOfertas.Web/Services/SpreadsheetImporter.cs`
- `src/ClubeDasOfertas.Web/Services/ExportService.cs`
- `src/ClubeDasOfertas.Web/Program.cs`
- `src/ClubeDasOfertas.Web/Ui/HtmlView.cs`
- `tests/ClubeDasOfertas.Tests/Program.cs`
- `docs/wiki/dominio/exportacao.md`
- `docs/wiki/dominio/item.md`
- `docs/wiki/historico/bugs-resolvidos.md`
- `docs/wiki/wiki-log.md`

## Mudancas previstas

- Propagar o valor da celula superior esquerda para todo o intervalo de merge ao ler XLSX/XLSM.
- Remover o bloqueio duro da exportacao por `blocking_reasons`.
- Exibir aviso de exportacao com pendencias na interface.
- Incluir no CSV colunas operacionais de status, riscos, pendencias, vigencia e fonte.
- Mostrar `Fonte` e `Vigencia` como colunas explicitas na tabela da campanha.
- Reforcar o destaque visual de riscos e pendencias com badges coloridos.

## Fora de escopo

- Alterar o formato final de download de CSV para XLSX.
- Mudar regras de matching ou de conversao de pesaveis/fardos/caixas.
- Reestruturar o schema do banco.

## Verificacao alvo

- `dotnet build ClubeDasOfertas.slnx`
- `dotnet run --project tests\\ClubeDasOfertas.Tests\\ClubeDasOfertas.Tests.csproj`
