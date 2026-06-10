---
titulo: arquitetura
categoria: tecnico
criado: 2026-06-09
atualizado: 2026-06-09
fontes: []
links: [../dominio/exportacao.md, ../historico/bugs-resolvidos.md]
---

# Arquitetura

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
