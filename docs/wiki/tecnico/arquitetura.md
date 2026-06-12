---
titulo: arquitetura
categoria: tecnico
criado: 2026-06-09
atualizado: 2026-06-12
fontes: []
links: [../dominio/exportacao.md, ../dominio/campanha.md, ../dominio/item.md, ../historico/bugs-resolvidos.md]
---

# Arquitetura

## 2026-06-12 - Bootstrap do banco ficou mais explicito no startup

- `AppDb` passou a normalizar a connection string padrao de `localhost` para `127.0.0.1`, reduzindo o risco de falha quando o PostgreSQL local aceita conexoes apenas em IPv4.
- Quando a abertura de conexao falha, a excecao agora sobe como `InvalidOperationException` com orientacao operacional em portugues brasileiro, apontando os dois pre-requisitos do ambiente local: banco em execucao e variaveis do `.env` carregadas no processo que roda o `dotnet run`.
- A mudanca nao mascara a falha real do Npgsql; ela apenas preserva a excecao original como inner exception e troca a mensagem de topo por um diagnostico acionavel.

## 2026-06-12 - Campanhas passaram a aceitar inclusao manual de itens

- `Program.cs` ganhou a rota `POST /campaigns/{campaignId}/items/add` e um formulario na tela de detalhe da campanha para inserir um item sem reimportar a planilha.
- `CampaignItemEditorService` passou a criar o item manual, reaproveitar `CampaignImportService.EvaluateItem`, marcar a origem como `Manual` e registrar a inclusao no audit log.
- `AppRepository` passou a persistir esse fluxo em uma transacao propria, incluindo um `ImportBatch` sintetico, a atualizacao de itens afetados e a insercao do novo item na mesma unidade de trabalho.
- A reavaliacao de duplicidade continua acontecendo antes da gravacao final, para que um item manual novo nao deixe codigos repetidos sem marcacao.

## 2026-06-12 - Regras de fardo e caixa passaram a dividir quantidade

- `CampaignImportService.EvaluateItem` agora aplica a regra de package tanto no preco quanto na quantidade: o preco continua multiplicando pelo fator da regra e a quantidade passa a ser dividida pelo mesmo fator antes de ir para a tabela e para o CSV.
- A divisao acontece somente quando uma regra de `Fardo`, `Caixa` ou a regra legada equivalente e encontrada, preservando o fluxo dos demais tipos de conversao.
- Os testes passaram a cobrir a nova saida de quantidade para garantir que a conversao de package continue coerente ao lado do alerta e da multiplicacao de preco.

## 2026-06-12 - Grafico do catalogo ganhou paleta fixa por categoria

- A renderizacao do grafico de categorias em `Program.cs` deixou de usar uma paleta ciclica e passou a mapear cores fixas por nome de categoria, mantendo a leitura consistente entre filtros e recargas.
- A normalizacao de exibicao agora trata `Sobremesa` como `Sobremesas` na legenda do grafico, no filtro lateral e nos cards dos itens, sem exigir migracao de dados persistidos.
- O ajuste visual da lista `Campanhas criadas` permaneceu concentrado nos overrides de `HtmlView.cs`, com aumento discreto de fonte para cabecalhos, textos auxiliares, badges e botoes sem desfazer a compactacao recente da grade.
- No refinamento seguinte, a mesma cor da categoria passou a aparecer tambem na navegacao lateral e nos cards dos itens do catalogo, com borda de destaque, ponto colorido e badge da categoria para que a mudanca fique perceptivel mesmo antes de chegar ao grafico.
- A correção definitiva exigiu subir a canonizacao para `TextNormalizer`: filtro, lista lateral, cards e grafico agora compartilham a mesma chave normalizada de categoria, evitando que variantes como `Sobremesa` ou nomes com acento/formatacao diferente caiam todas na cor de fallback.
- No ajuste seguinte, a categoria combinada `Frios e Hortifruti` foi revertida: a canonizacao passou a preservar `Frios` como categoria propria e a consolidar os casos combinados em `Hortifruti`, com dois tons de verde diferentes no mapa de cores.
- O filtro lateral voltou a expor uma entrada explicita de `Todos os itens`, separada das categorias coloridas, e os links/linhas do grafico passaram a carregar tooltip nativa com quantidade e percentual para facilitar a leitura quando a visao geral estiver selecionada.
- Para o filtro geral continuar funcional apos esse ajuste, `Todos os itens` e `Todas as categorias` passaram a ser tratados como ausencia de filtro antes de chegar ao repositório e antes de reidratar o formulario; isso evita que a string da UI seja reaproveitada como se fosse uma categoria real.
- O grafico de categorias deixou de usar `conic-gradient` e passou a ser desenhado em SVG com segmentos de stroke e pequenos gaps entre as fatias; isso melhorou a nitidez das divisoes e permitiu hover direto na fatia com tooltip flutuante de categoria, quantidade e percentual, reutilizando o mesmo balão tambem na legenda.
- No ajuste seguinte, os gaps artificiais e o track claro foram removidos do SVG para evitar recortes brancos entre as fatias; o donut passou a fechar os segmentos sem vazamento do fundo, inclusive quando so existe uma categoria.

## 2026-06-11 - Overlay fixo da tooltip das regras com template

- A coluna `Como acontece` ficou com apenas o preview resumido na celula, enquanto o conteudo completo passou a ficar guardado em um `<template>` para nao participar do fluxo da tabela.
- O balao agora e um overlay fixo no `body`, reposicionado por `getBoundingClientRect` da celula em `hover` e `focus`, o que evita que o `tablewrap` crie barra extra ou recorte o conteudo.
- O overlay continua sobre a propria regra, mas deixou de depender de HTML inline visivel dentro da grade para nao virar parte da lista.

## 2026-06-11 - Tooltip da regra voltou para HTML e CSS

- A informação de `Como acontece` na tabela de `Regras` voltou a ser renderizada junto da propria celula, com exibição controlada por `:hover` e `:focus` no CSS.
- O balão deixou de depender de JavaScript para preencher texto ou calcular posição, o que elimina o risco de tooltip vazia e mantém o conteúdo sempre disponível no DOM.

## 2026-06-11 - Tooltip da regra recebeu Base64 para evitar balao vazio

- O overlay da regra passou a receber `pattern_input` e `pattern_regex` via Base64, reduzindo o risco de atributo HTML mal interpretado deixar a tooltip vazia.
- O JS decodifica os valores e ainda usa a preview visivel da celula como fallback, mantendo a informacao sobreposta sem voltar para o fluxo da tabela.

## 2026-06-11 - Tooltip da regra virou overlay flutuante

- A previa expandida de `Como acontece` na tabela de `Regras` passou a ser um overlay fixo sobreposto ao `body`, posicionado por `getBoundingClientRect` da célula.
- O overlay deixou de participar da altura da tabela, eliminando a barra de rolagem extra na ultima linha e mantendo a informacao como contexto visual externo à listagem.
- A celula ficou apenas como gatilho e preview resumido, sem carregar o balao dentro da grade.

## 2026-06-11 - Tooltip da regra centralizada sobre a propria linha

- A tooltip de `Regras` foi recalibrada para ficar centrada sobre a propria celula, em vez de buscar ancoragem acima ou abaixo da linha.
- A posicao continua sendo calculada no `body` por JS, mas agora o overlay usa o centro do gatilho como referencia visual principal.

## 2026-06-11 - Lista principal de regras mostra apenas o essencial

- A tabela principal de `Regras` passou a exibir somente `Status`, `Nome`, `Como acontece`, `Multiplicador` e as acoes de `Editar`, `Alternar` e `Excluir`.
- Campos operacionais menos consultados na leitura rapida, como `Tipo`, `Unidade alvo`, `Categorias alvo` e `Exigir revisao`, permaneceram acessiveis apenas dentro do formulario de edicao inline da propria regra.
- O `colspan` da linha expansivel e as larguras da `rules-tablewrap` foram recalibrados para a nova grade, reduzindo ruído visual sem alterar a logica de criacao, edicao ou alternancia das regras.

## 2026-06-11 - Lista de campanhas compactada para manter colunas visiveis

- A grade resumida de campanhas em `HtmlView.cs` teve as larguras minimas das colunas recalibradas para priorizar a visibilidade conjunta de `Campanha`, `Vigencia`, `Status`, `Itens`, `Pendencias` e `Acoes`.
- A compactacao ficou restrita a tipografia, `gap`, `padding` e badges da propria listagem, sem mudar a estrutura mobile nem a logica de renderizacao em `Program.cs`.
- A pagina `page-campaign` recebeu overrides especificos para preservar a fonte menor da listagem mesmo com os estilos tipograficos mais amplos usados no restante da tela.

## 2026-06-10 - Campanhas e regras ganharam filtros e acoes mais operacionais

- A tela de detalhe da campanha passou a aceitar busca textual por descricao, `Solidus`, codigo, fonte e numero da linha de origem, com o campo posicionado ao lado do bloco de exportacao.
- O filtro textual foi integrado ao mesmo fluxo dos filtros por status/risco, inclusive nas acoes de confirmar, rejeitar, editar e confirmar todos os itens visiveis, para evitar perda de contexto durante a revisao.
- A listagem resumida de campanhas foi recalibrada no CSS para caber melhor no painel sem exigir rolagem horizontal em cenarios comuns.
- A tabela de `Regras` ganhou layout proprio para quebrar descricoes longas sem abrir barra horizontal e passou a oferecer exclusao explicita, alem de editar e alternar status.
- No ajuste fino seguinte, a listagem de campanhas teve as larguras minimas e a area de acoes compactadas mais uma vez para atacar a barra horizontal residual, enquanto a coluna `Como acontece` das regras passou a usar previa curta com expansao no hover e `title` com o texto completo.

## 2026-06-10 - Tema do site migrou para a nova identidade visual

- `HtmlView.cs` passou a aplicar de fato os tokens definidos no manual visual novo, com base preta no cabecalho, destaque amarelo (`#FFED00`), acoes vermelhas (`#A21815`) e superficies claras de apoio.
- A reestilizacao concentrou a troca no bloco CSS server-side da propria view para manter a mudanca coesa em campanhas, catalogo, badges, botoes, tabelas e estados de hover.
- A pagina de campanhas manteve seus overrides tipograficos, mas foi realinhada para o mesmo sistema cromatico da navegacao principal, evitando que o topo ficasse com uma paleta diferente do restante do site.

## 2026-06-10 - Manual visual recomposto para a marca Clube Das Ofertas

- Foi criado o documento [Manual-Identidade-Visual-Clube-Das-Ofertas.md](../../Manual-Identidade-Visual-Clube-Das-Ofertas.md) para servir como base de reestilizacao do site.
- O manual reaproveita a organizacao editorial do `Manual Logo Das Rede.pdf` como referencia estrutural e fixa a nova paleta principal a partir de `Clube Das Ofertas fundo redondo.png`.
- A paleta operacional definida para o produto ficou centrada em `#000000`, `#FFED00`, `#A21815` e `#FFFFFF`, com tokens auxiliares prontos para aplicacao em CSS.

## 2026-06-10 - README explica encerramento completo do servidor local

- O fluxo operacional do `README.md` passou a documentar que `dotnet run` pode deixar `ClubeDasOfertas.Web.exe` ativo mesmo depois de a sessao principal aparentar ter parado.
- A secao de execucao agora traz tres comandos separados: parada do `dotnet run`, limpeza de instancia residual do executavel web e uma verificacao final para confirmar que nenhum processo do servico ficou aberto.

## 2026-06-10 - Logo do rodape com fundo transparente

- O asset `clube-das-ofertas-secundaria.png` usado no rodape fixo foi regravado com canal alfa, removendo o fundo branco incorporado na exportacao anterior.
- A mudanca preserva o mesmo arquivo e o mesmo markup da UI; apenas a imagem publica em `wwwroot` mudou para se integrar melhor ao fundo do site.

## 2026-06-10 - Startup sincroniza campanhas com regras ativas

- O `SchemaInitializer` passou a fazer duas conciliacoes no startup: primeiro normaliza regras amigaveis antigas para regenerar o regex correto; em seguida reavalia itens de campanha ja importados com base nas regras ativas atuais.
- A reavaliacao reaproveita a mesma logica de `CampaignImportService.EvaluateItem`, entao o comportamento de importacao nova e de campanhas historicas fica alinhado.
- Para evitar perda de decisao humana, itens com status `Aprovado` ou `Rejeitado` nao sao sobrescritos por essa sincronizacao automatica; apenas itens ainda sem decisao final sao recalculados.
- Quando o catalogo original nao pode ser recarregado integralmente, a sincronizacao preserva `barcode`, `descricao solidus` e `code_type` ja gravados no item para nao degradar o contexto de campanhas antigas.

## 2026-06-10 - Grafico de categorias abaixo da lista do catalogo

- A pagina `Catalogo de produtos` passou a renderizar um grafico de pizza logo abaixo da listagem de itens visiveis, dentro do mesmo painel de resultados.
- O grafico usa a distribuicao das categorias da lista filtrada atual, em vez do catalogo inteiro, para acompanhar busca textual e filtro lateral sem criar leituras divergentes.
- A area rolavel da lista foi mantida separada; o grafico fica fora de `catalog-list-shell`, preservando a barra propria dos itens e deixando o resumo visual fixo abaixo dela.
- Depois do primeiro ajuste, o grafico foi movido para fora do painel principal do catalogo e transformado em um painel proprio abaixo da grade, para devolver toda a altura util da lista de itens.
- O painel da direita deixou de usar altura fixa; a lista de itens passou a usar o mesmo limite visual da lista de categorias (`52vh`), evitando espaco vazio antes do grafico quando poucos itens ficam visiveis.
- O ajuste definitivo veio ao agrupar lista e grafico dentro da mesma coluna direita (`catalog-main-column`); assim o grafico comeca logo apos a listagem, sem esperar a coluna esquerda terminar.
- O alinhamento fino final foi aplicado no proprio painel `catalog-results-panel`, e nao mais na coluna inteira; um deslocamento vertical pequeno no bloco `Itens do catalogo` preserva o encaixe do grafico abaixo e resolve a diferenca visual em relacao ao bloco `Navegacao`.
- A calibragem fina desse painel foi ajustada mais uma vez de `-2px` para `-3px`, porque a diferenca residual ainda aparecia no topo do bloco em navegadores com renderizacao mais sensivel a subpixel.

## 2026-06-10 - Regras aceitam escrita amigavel e convertem para regex

- A tela de `Regras` deixou de exigir que o operador escreva regex manualmente; o formulario agora pede "como a regra deve acontecer" em texto amigavel.
- O backend passou a salvar tanto a descricao amigavel (`pattern_input`) quanto o regex final (`pattern`), preservando a edicao futura sem forcar o usuario a reaprender a expressao regular gerada.
- O conversor server-side aceita alternativas com `ou`, `;`, `|` ou quebra de linha e usa `*` como coringa; quando o texto ja parece regex, ele eh preservado como literal para manter compatibilidade com regras avancadas existentes.
- O detector de regex literal foi refinado para nao tratar `|` amigavel como regex por si so; isso evita que entradas como `Fardo | Caixa` escapem da conversao e deixem de acionar revisao quando deveriam casar com os itens.

## 2026-06-09 - Logo institucional movida para o rodapé fixo

- A marca `Clube Das Ofertas` deixou de ocupar o cabeçalho e passou a ser renderizada como elemento fixo no canto inferior direito de todas as páginas.
- O `main` ganhou folga adicional no rodapé para reduzir a chance de sobreposição com conteúdo próximo ao fim da tela.

## 2026-06-09 - Busca do catálogo com espaçamento próprio no painel

- O formulário de busca do painel de resultados do catálogo passou a usar espaçamento inferior próprio, separando melhor os botões `Buscar` e `Limpar` do início visual da lista de itens.
- O ajuste foi isolado em uma classe específica do catálogo para não alterar o espaçamento padrão dos demais formulários da aplicação.

## 2026-06-09 - Busca do catalogo movida para o painel de resultados

- A busca do catÃ¡logo saiu da coluna lateral e passou a ficar logo abaixo do titulo `Itens do catÃ¡logo`, no topo do painel de resultados.
- O formulario continua usando `GET /catalog` e preserva o filtro de categoria atual via campo oculto, evitando regressao no cruzamento entre busca textual e navegacao por categoria.

## 2026-06-09 - Exportacao sem bloqueio duro

- `ExportService` deixou de interromper a geracao do CSV quando `campaign_items.blocking_reasons` ainda possui valores.
- O controle de risco foi movido para duas camadas mais explicitas:
  - aviso na interface antes do `POST /campaigns/{id}/export`;
  - enriquecimento do CSV com colunas de status, riscos e pendencias.
- A leitura de XLSX/XLSM passou a propagar o valor da celula superior esquerda para todo o intervalo declarado em `mergeCells`, preservando `fonte` e `vigencia` quando a planilha pronta usa celulas mescladas.

## 2026-06-09 - Paleta do site alinhada ao EPS da marca

- O tema visual do site passou a usar as cores principais identificadas em `Clube DAS Ofertas versão preferencial.eps`.
- As duas cores-base extraidas do arquivo foram:
  - amarelo `0 0 1 0` em CMYK;
  - vermelho escuro `.2157 1 1 .2275` em CMYK, convertido para um tom aproximado de `#9b0000`.
- O layout foi reorganizado para trabalhar com fundo claro quente, paineis brancos, destaque amarelo e acoes principais em vermelho institucional, preservando contraste de leitura.

## 2026-06-09 - Logo do cabecalho trocada para a versao secundaria

- O cabeçalho passou a usar a arte derivada de `Clube DAS Ofertas versão secundária.eps`.
- A imagem foi exportada para `wwwroot/clube-das-ofertas-secundaria.png` e a referencia do `header-brandmark` foi atualizada para esse asset.

## 2026-06-09 - Layout de campanha e catalogo refinado

- A tabela de detalhes da campanha passou a manter os campos textuais em uma linha unica, deixando a leitura horizontal apoiada pelo `tablewrap`.
- A listagem do catálogo deixou de usar scroll interno na coluna da direita, para terminar junto da navegação lateral e reduzir áreas vazias.
- Os titulos duplicados do corpo das páginas administrativas foram removidos, mantendo a identificação principal somente no cabeçalho.
- A página antes rotulada como `Catálogo` passou a usar `Catálogo de produtos` no cabeçalho e no título do documento.

## 2026-06-09 - Detalhe da campanha sem importacao duplicada

- A tela de detalhe da campanha passou a focar apenas em revisao e exportacao; o formulario de importacao permanece somente no fluxo de criacao da campanha.
- A vigencia saiu da grade de itens e passou a aparecer logo abaixo do titulo da campanha, liberando espaco horizontal para os dados operacionais.
- O grid de itens continua com `nowrap` como padrao, mas as colunas de descricao agora abrem excecao com quebra de linha propria, enquanto preco final, quantidade e acoes foram agrupados no mesmo bloco visual.
- O bloco de acoes inline do item foi movido para baixo da descricao Solidus, em linha unica dentro da propria coluna textual, reduzindo espaco em branco e aproximando as acoes do contexto principal do item.
- A coluna de riscos ganhou excecao propria de quebra, permitindo que os badges ocupem mais de uma linha sem soltar as demais colunas da tabela.

## 2026-06-09 - Textos da UI normalizados

- A interface passou a centralizar parte da normalizacao textual no proprio renderizador, corrigindo labels, filtros, badges, papeis de usuario e historico de auditoria sem depender da limpeza imediata dos dados ja persistidos.
- Mensagens de apoio vindas da importacao, do preview de contas e dos avisos operacionais foram revistas para usar acentuacao e termos consistentes em portugues brasileiro.
- A revisao tambem padronizou pluralizacao simples (`campanha`/`campanhas`, `item`/`itens`, `pendencia`/`pendencias`) e nomes mais claros para elementos como `Catálogo`, `Histórico`, `Aplicativo` e `Administrador`.

## 2026-06-09 - Lista do catalogo com rolagem propria e altura ampliada

- A lista de itens do catálogo voltou a ter rolagem própria, em vez de compartilhar a barra da página inteira.
- O painel de resultados recebeu altura mínima própria e a área rolável foi ampliada para acompanhar melhor o painel lateral sem voltar ao grande espaço em branco anterior.
- A marcação do catálogo precisa manter a classe `catalog-results-panel` no painel da direita; sem isso, a altura própria do painel não entra em efeito e a lista volta a disputar a rolagem com a página.
- O painel de resultados usa uma altura limitada pela viewport e `overflow: hidden`, enquanto `catalog-list-shell` usa `overflow-y: scroll`, `scrollbar-gutter: stable` e `overscroll-behavior: contain`; isso mantém a barra da lista independente da barra da página.
