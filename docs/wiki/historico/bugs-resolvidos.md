---
titulo: bugs-resolvidos
categoria: historico
criado: 2026-06-08
atualizado: 2026-06-09
fontes: []
links: [../sintese/mojibake-codificacao.md]
---

# Bugs resolvidos

## 2026-06-08 â€” Textos com mojibake na UI

- Sintoma: textos da interface apareciam como `descriÃƒÂ§ÃƒÂ£o`, `FormulÃƒÂ¡rio`, `CatÃƒÂ¡logo` e equivalentes.
- Causa raiz: literais de [Program.cs](../src/ClubeDasOfertas.Web/Program.cs) foram regravados com interpretaÃ§Ã£o incorreta de bytes UTF-8 como Windows-1252/Latin-1.
- SoluÃ§Ã£o: restauraÃ§Ã£o dos literais corrompidos em `src/ClubeDasOfertas.Web/Program.cs`, preservando a lÃ³gica existente das rotas e renderizaÃ§Ã£o.
- VerificaÃ§Ã£o: busca por `Ãƒ`, `Ã¢` e `ï¿½` em `src/` e `tests/` sem ocorrÃªncias; `dotnet build ClubeDasOfertas.slnx` concluÃ­do com sucesso.

## 2026-06-08 â€” NÃºmeros invisÃ­veis nos cards da campanha

- Sintoma: os cards de estatÃ­sticas da campanha mostravam apenas os rÃ³tulos, sem os valores numÃ©ricos.
- Causa raiz: no tema `page-campaign`, os cards `.stat` mantinham fundo claro enquanto os nÃºmeros herdavam cor branca do tema escuro, ficando invisÃ­veis.
- SoluÃ§Ã£o: inclusÃ£o de `.stat` nos overrides visuais do tema de campanha em `src/ClubeDasOfertas.Web/Ui/HtmlView.cs` e ajuste da cor do texto secundÃ¡rio dos cards.
- VerificaÃ§Ã£o: `dotnet build ClubeDasOfertas.slnx` e `dotnet run --project tests\ClubeDasOfertas.Tests\ClubeDasOfertas.Tests.csproj` concluÃ­dos com sucesso.
## 2026-06-08 — Quantidade aceita texto puro na edição inline

- Sintoma: no fluxo de edição manual dos itens de fardos e caixas, o campo de quantidade rejeitava valores como `Caixas` ou `Fardos` quando o operador queria apenas indicar a unidade.
- Causa raiz: o parser de quantidade exigia uma expressão com número e não tinha tratamento para unidade escrita em texto puro.
- Solução: `Parsing.ParseQuantity` passou a reconhecer unidades textuais sem número, tratar a quantidade base como `1` e mostrar o resultado no preview antes da confirmação.
- Verificação: `dotnet build ClubeDasOfertas.slnx` e `dotnet run --no-build --project tests\ClubeDasOfertas.Tests\ClubeDasOfertas.Tests.csproj` concluídos com sucesso.

## 2026-06-08 — Preview desaparecia ao reabrir a edição

- Sintoma: o preview da conta aparecia na primeira abertura da edição, mas sumia nas aberturas seguintes do mesmo item.
- Causa raiz: o bloco de preview era condicionado ao estado de revisão do item, então ele podia deixar de ser renderizado depois de salvar e reabrir a linha.
- Solução: o preview passou a depender apenas do fluxo de fardos e caixas, e o JavaScript força uma nova renderização ao reabrir a edição.
- Verificação: `dotnet build ClubeDasOfertas.slnx` e `dotnet run --no-build --project tests\ClubeDasOfertas.Tests\ClubeDasOfertas.Tests.csproj` concluídos com sucesso.

## 2026-06-08 — Quantidade aceita conta com unidade textual no meio

- Sintoma: entradas como `120 caixas / 6` e `120 fardos / 6` falhavam na edição manual, embora `120 unidades / 6` funcionasse.
- Causa raiz: o sanitizador da expressão preservava o `x` dentro de palavras como `caixas` e o interpretava como multiplicação, quebrando a conta.
- Solução: o parser e o preview passaram a remover palavras da unidade antes da avaliação, preservando apenas `x` isolado quando ele realmente representa multiplicação.
- Verificação: `dotnet build ClubeDasOfertas.slnx` e `dotnet run --no-build --project tests\ClubeDasOfertas.Tests\ClubeDasOfertas.Tests.csproj` concluídos com sucesso.

## 2026-06-08 — Conta de preço sumia ao reabrir a edição

- Sintoma: depois de salvar um item com conta em `Preço venda` ou `Preço clube`, a próxima abertura da edição mostrava apenas o valor final convertido, sem a expressão original e sem um preview confiável para conferência.
- Causa raiz: o formulário de edição lia os preços finais já calculados em vez de reaproveitar a expressão crua digitada pelo operador, e o banco ainda não persistia esses textos separados.
- Solução: `campaign_items` passou a guardar `price_sale_raw` e `price_club_raw`, o repositório passou a ler e gravar esses campos, e a tela de edição voltou a exibir a expressão original enquanto a listagem mantém o valor convertido.
- Verificação: `dotnet build ClubeDasOfertas.slnx` e `dotnet run --no-build --project tests\ClubeDasOfertas.Tests\ClubeDasOfertas.Tests.csproj` concluídos com sucesso.

## 2026-06-09 - Detalhe da campanha repetia importacao e espalhava acoes

- Sintoma: depois de criar a campanha, a tela de detalhe repetia o formulario de importacao, gastava uma coluna inteira com vigencia e deixava as acoes isoladas no fim da tabela, enquanto as descricoes continuavam presas em uma unica linha.
- Causa raiz: `RenderCampaignDetails` ainda agrupava importacao, revisao e exportacao na mesma secao, e o CSS da tabela aplicava `white-space: nowrap` para todas as celulas sem excecao.
- Solucao: a importacao foi removida da tela de detalhe, a vigencia subiu para baixo do titulo da campanha, as descricoes ganharam uma classe com quebra controlada e os botoes foram movidos para junto de preco final e quantidade.
- Verificacao: `dotnet build ClubeDasOfertas.slnx`.

## 2026-06-09 — Fonte sumia em planilha pronta e exportação travava

- Sintoma: ao importar uma planilha pronta, itens do mesmo bloco mesclado perdiam a informação de `Tabloide` ou `App`, e a exportação ficava bloqueada enquanto existissem pendências de revisão.
- Causa raiz: o leitor de XLSX/XLSM considerava apenas células explícitas, sem replicar o valor dos intervalos definidos em `mergeCells`; além disso, `ExportService` tratava qualquer `blocking_reasons` como bloqueio duro.
- Solução: o importador passou a propagar o valor da célula superior esquerda para o intervalo mesclado, preservando `fonte` e `vigencia`, e a exportação passou a seguir com aviso na interface e com colunas adicionais de status, riscos e pendências no CSV.
- Verificação: `dotnet build ClubeDasOfertas.slnx` e `dotnet run --no-build --project tests\ClubeDasOfertas.Tests\ClubeDasOfertas.Tests.csproj` concluídos com sucesso.

## 2026-06-09 - UI misturava acentos, labels crus e textos inconsistentes

- Sintoma: a interface exibia uma combinacao de termos sem acento, labels tecnicos crus como `SEM_CATALOGO`, funcoes em ingles no historico e pluralizacoes improvisadas como `item(ns)` e `campanha(s)`.
- Causa raiz: os textos visiveis estavam espalhados entre `Program.cs`, `HtmlView.cs`, seeds e mensagens de servico, sem uma camada de apresentacao que corrigisse dados legados ou padronizasse os rótulos.
- Solucao: a UI passou a usar textos revisados em portugues brasileiro, com mapeamento de badges, historico e filtros para exibicao mais clara, alem da correcao das mensagens que sobem da importacao e da revisao.
- Verificacao: `dotnet build ClubeDasOfertas.slnx` e `dotnet run --project tests\ClubeDasOfertas.Tests\ClubeDasOfertas.Tests.csproj`.
