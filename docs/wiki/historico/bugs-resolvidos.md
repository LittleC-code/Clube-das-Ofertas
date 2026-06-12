---
titulo: bugs-resolvidos
categoria: historico
criado: 2026-06-08
atualizado: 2026-06-12
fontes: []
links: [../sintese/mojibake-codificacao.md]
---

# Bugs resolvidos

## 2026-06-12 - Startup falhava com erro cru de conexao em ::1:5432

- Sintoma: ao iniciar a aplicacao sem o PostgreSQL local disponivel, o processo abortava no `SchemaInitializer` com `NpgsqlException` e `SocketException` apontando `Failed to connect to [::1]:5432`, sem indicar claramente os pre-requisitos operacionais do ambiente.
- Causa raiz: a configuracao padrao usava `Host=localhost`, que pode resolver primeiro para IPv6, enquanto o ambiente local depende de um PostgreSQL levantado separadamente e de variaveis do `.env` carregadas manualmente antes do `dotnet run`.
- Solucao: `AppDb` passou a normalizar `localhost` para `127.0.0.1` na conexao padrao e a encapsular a falha de abertura com uma mensagem acionavel em portugues brasileiro, orientando a subir o banco local e carregar as variaveis do `.env`.
- Verificacao: `Test-NetConnection -ComputerName localhost -Port 5432` confirmou ausencia de listener em `127.0.0.1` e `::1`; `dotnet build ClubeDasOfertas.slnx` ficou bloqueado por restauracao NuGet sem acesso de rede no sandbox.

## 2026-06-12 - Categoria Sobremesa aparecia no singular na UI do catalogo

- Sintoma: partes da interface do catalogo ainda exibiam a categoria como `Sobremesa`, destoando do agrupamento desejado em `Sobremesas` e deixando a legenda do grafico inconsistente.
- Causa raiz: a UI reutilizava o valor bruto vindo de `entry.Category` e do agrupamento lateral, sem uma camada de normalizacao de apresentacao para nomes de categoria.
- Solucao: `Program.cs` passou a centralizar a exibicao das categorias em um helper que normaliza `Sobremesa` para `Sobremesas` e reaproveita esse nome tanto no grafico quanto no filtro lateral e na lista de itens.
- Verificacao: `dotnet build ClubeDasOfertas.slnx` concluido com sucesso.

## 2026-06-12 - Paleta de categorias caia toda na mesma cor de fallback

- Sintoma: mesmo depois do ajuste visual, o catalogo ainda aparecia com a mesma cor em varias categorias, e `Sobremesa` nao se consolidava com `Sobremesas` de forma confiavel.
- Causa raiz: o mapeamento de cor dependia de nomes exatos e a busca por categoria ainda comparava o valor bruto do banco; com isso, variacoes de acento, plural ou composicao textual escapavam do mapeamento canonico e voltavam para a cor padrao.
- Solucao: `TextNormalizer` passou a expor uma chave canonica de categoria e um nome de exibicao compartilhados por `AppRepository` e `Program.cs`; o filtro, o agrupamento lateral, os cards e o grafico agora usam essa mesma normalizacao antes de comparar, agrupar e colorir.
- Verificacao: `dotnet build ClubeDasOfertas.slnx` e `dotnet run --no-build --project tests\\ClubeDasOfertas.Tests\\ClubeDasOfertas.Tests.csproj`.

## 2026-06-12 - Categoria combinada escondia Frios e sumia com o filtro Todos os itens

- Sintoma: `Frios e Hortifruti` passou a aparecer como uma categoria so, `Frios` perdeu identidade propria, e a entrada de visao geral do catalogo deixou de ficar claramente separada das categorias coloridas.
- Causa raiz: a canonizacao anterior juntava qualquer ocorrencia com `HORTIFRUTI` na chave `FRIOS E HORTIFRUTI`, e a navegacao lateral deixou a visao geral apenas como mais um link visualmente parecido com as categorias.
- Solucao: a normalizacao foi revertida para consolidar os casos combinados em `Hortifruti`, mantendo `Frios` como categoria propria; o mapa de cores ganhou dois tons de verde distintos e o filtro `Todos os itens` voltou como entrada explicita, separada das categorias e com tooltip nativa de quantidade/percentual.
- Verificacao: `dotnet build ClubeDasOfertas.slnx` e `dotnet run --no-build --project tests\\ClubeDasOfertas.Tests\\ClubeDasOfertas.Tests.csproj`.

## 2026-06-12 - Selecionar Todos os itens zerava lista e gráfico do catálogo

- Sintoma: ao deixar `Categoria atual: Todos os itens`, a tela do catálogo passava a mostrar `0 itens`, nenhum registro e nenhum gráfico, mesmo havendo base carregada.
- Causa raiz: o valor visual `Todos os itens` estava sendo reaproveitado como valor de filtro no hidden field e no backend, em vez de voltar ao estado vazio que representa ausência de categoria.
- Solucao: `TextNormalizer` ganhou um detector explícito para o filtro geral e `Program.cs`/`AppRepository.cs` passaram a converter `Todos os itens` e `Todas as categorias` em filtro vazio antes de buscar e reidratar a tela.
- Verificacao: `dotnet build ClubeDasOfertas.slnx` e `dotnet run --no-build --project tests\\ClubeDasOfertas.Tests\\ClubeDasOfertas.Tests.csproj`.

## 2026-06-12 - Bordas do gráfico ficavam serrilhadas e sem hover por fatia

- Sintoma: o donut de categorias mostrava divisões visuais ruins entre as cores e não permitia descobrir a categoria exata ao passar o cursor sobre cada fatia.
- Causa raiz: a versão com `conic-gradient` desenhava toda a pizza como um único fundo CSS, sem elementos interativos por segmento e com separação visual limitada entre as cores.
- Solucao: o gráfico foi refeito em SVG com uma fatia por `circle`/stroke, gaps controlados entre segmentos e tooltip flutuante compartilhada entre fatias e linhas da legenda, mostrando categoria, quantidade e percentual.
- Verificacao: `dotnet build ClubeDasOfertas.slnx` e `dotnet run --no-build --project tests\\ClubeDasOfertas.Tests\\ClubeDasOfertas.Tests.csproj`.

## 2026-06-12 - Recorte branco aparecia entre as fatias do donut

- Sintoma: depois da troca para SVG, algumas divisões do gráfico exibiam um recorte branco visível entre as cores, inclusive em cenários com apenas uma categoria.
- Causa raiz: os gaps artificiais entre segmentos e o círculo-base claro ainda apareciam por baixo das fatias, expondo o fundo do gráfico.
- Solucao: os segmentos passaram a fechar sem gap visual e o track claro foi removido do SVG; o stroke agora usa `butt` e, quando ha apenas uma categoria, a fatia ocupa a circunferência inteira sem emenda aparente.
- Verificacao: `dotnet build ClubeDasOfertas.slnx` e `dotnet run --no-build --project tests\\ClubeDasOfertas.Tests\\ClubeDasOfertas.Tests.csproj`.

## 2026-06-11 - Tooltip das regras ficou presa na tabela

- Sintoma: ao passar o cursor sobre `Como acontece`, a informacao da regra ainda parecia uma barra vazia ou parte da propria lista, e a tabela seguia arriscando rolagem extra no hover.
- Causa raiz: o conteudo completo ainda estava sendo desenhado dentro do `tablewrap`, o que prendia o balao ao fluxo da tabela e deixava a leitura inconsistente quando o container rolavel recalculava o layout.
- Solucao: o preview resumido ficou na celula e o conteudo completo passou para um `<template>`; o balao agora e um overlay fixo no `body`, posicionado por JS sobre a propria celula e fora do fluxo da lista.
- Verificacao: `dotnet build ClubeDasOfertas.slnx` e `dotnet run --project tests\ClubeDasOfertas.Tests\ClubeDasOfertas.Tests.csproj`.

## 2026-06-11 - Coluna de pendencias ficava escondida na lista de campanhas

- Sintoma: na pagina de campanhas, a coluna `Pendencias` ficava espremida ou parcialmente escondida, exigindo rolagem horizontal para enxergar a grade completa em larguras comuns de desktop.
- Causa raiz: a listagem resumida usava larguras minimas, gaps, badges e botoes ainda grandes demais para a soma das seis colunas, especialmente sob os overrides tipograficos da `page-campaign`.
- Solucao: o CSS da lista de campanhas em `src/ClubeDasOfertas.Web/Ui/HtmlView.cs` foi compactado com colunas mais estreitas, fonte menor nos cabecalhos e textos auxiliares, badges menores e acoes mais enxutas, mantendo o layout responsivo existente.
- Verificacao: `dotnet build ClubeDasOfertas.slnx` concluido com sucesso.

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

## 2026-06-10 - Regra amigavel com pipe nao acionava revisao

- Sintoma: regras escritas de forma amigavel com alternativas como `Fardo | Caixa` eram salvas, mas itens que deveriam casar com esse criterio passavam sem revisao.
- Causa raiz: o conversor passou a aceitar `|` como separador amigavel, mas o detector ainda tratava a presenca de `|` como indicio suficiente de regex literal em alguns fluxos; isso fazia a entrada pular a conversao amigavel e permanecer como uma expressao inadequada para o matching esperado.
- Solucao: o detector de regex literal foi refinado para preservar apenas sinais realmente caracteristicos de regex avancado, enquanto `|`, virgula e barra entre palavras continuam no fluxo amigavel e viram alternativas regex validas.
- Verificacao: `dotnet build ClubeDasOfertas.slnx` e `dotnet run --project tests\ClubeDasOfertas.Tests\ClubeDasOfertas.Tests.csproj`.

## 2026-06-10 - Regra semantica em descricao solidus nao retroagia para campanhas ja importadas

- Sintoma: uma regra como `Se conter a palavra CERV na descricao solidus, aplicar a regra` era normalizada para `CERV`, mas itens da campanha `09.06.2026 F` que ja tinham sido importados continuavam com `review_required = false` e `review_status = NaoRequer`.
- Causa raiz: a primeira correcao atuava apenas na linha de `conversion_rules`; os `campaign_items` antigos permaneciam com o resultado da avaliacao antiga, e o startup so reprocessava campanhas quando detectava uma nova mudanca naquele mesmo boot.
- Solucao: o startup passou a sincronizar itens importados com as regras ativas em toda inicializacao, reaproveitando `CampaignImportService.EvaluateItem` e preservando itens ja `Aprovado` ou `Rejeitado` para nao sobrescrever decisoes humanas.
- Verificacao: `dotnet run --project tests\\ClubeDasOfertas.Tests\\ClubeDasOfertas.Tests.csproj`, `dotnet build ClubeDasOfertas.slnx` e consulta real ao PostgreSQL mostrando itens com `description_solidus` contendo `CERV` marcados como `Pendente` na campanha `09.06.2026 F`.
