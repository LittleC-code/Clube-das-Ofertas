# Log de OperaÃ§Ãµes â€” Wiki Clube Das Ofertas

Registro cronolÃ³gico append-only de todas as operaÃ§Ãµes do agente sobre a wiki. Nunca editar entradas existentes â€” erros sÃ£o corrigidos com nova entrada.

Use `grep "^## \[" log.md | tail -10` para ver as Ãºltimas 10 entradas.

---

## [YYYY-MM-DD] wiki-criada | InicializaÃ§Ã£o da wiki de memÃ³ria
Estrutura criada: docs/wiki/ com index.md, log.md, overview.md e subdiretÃ³rios.
PÃ¡ginas stub criadas: campanha, item, matching, risco, exportacao, arquitetura, apprepository, autenticacao, importacao-xlsm, bugs-resolvidos, decisoes-revertidas.
PrÃ³ximo passo: popular as pÃ¡ginas de domÃ­nio e tÃ©cnico com o conhecimento existente no repositÃ³rio.
## [2026-06-08] sessÃ£o-iniciada | Investigar quebra de descriÃ§Ã£o
Tarefa: descobrir por que textos com acentuaÃ§Ã£o passaram a aparecer como `descriÃƒÂ§ÃƒÂ£o`.
PÃ¡ginas lidas: wiki-index.md, wiki-log.md, WIKI_GOVERNANCE.md

## [2026-06-08] consulta | Por que a descriÃ§Ã£o quebrou?
PÃ¡ginas consultadas: wiki-index.md, wiki-log.md, WIKI_GOVERNANCE.md, sintese/mojibake-codificacao.md
PÃ¡gina de sÃ­ntese criada: docs/wiki/sintese/mojibake-codificacao.md

## [2026-06-08] pÃ³s-tarefa | Corrigir mojibake da interface
PÃ¡ginas tÃ©cnicas atualizadas: nenhuma
PÃ¡ginas de domÃ­nio atualizadas: nenhuma
PÃ¡ginas de histÃ³rico atualizadas: historico/bugs-resolvidos.md
Aprendizados registrados: a corrupÃ§Ã£o estava concentrada em literais de `Program.cs`; a correÃ§Ã£o exigiu restaurar os textos em UTF-8 sem alterar a lÃ³gica das rotas.

## [2026-06-08] sessÃ£o-iniciada | Corrigir cards da campanha sem valores
Tarefa: descobrir por que os cards de estatÃ­sticas da campanha exibiam apenas os rÃ³tulos.
PÃ¡ginas lidas: wiki-index.md, wiki-log.md

## [2026-06-08] pÃ³s-tarefa | Corrigir contraste dos cards da campanha
PÃ¡ginas tÃ©cnicas atualizadas: nenhuma
PÃ¡ginas de domÃ­nio atualizadas: nenhuma
PÃ¡ginas de histÃ³rico atualizadas: historico/bugs-resolvidos.md
Aprendizados registrados: o problema era visual no tema `page-campaign`; os cards `.stat` nÃ£o recebiam o override de fundo escuro e escondiam nÃºmeros brancos sobre fundo claro.

## [2026-06-08] sessÃ£o-iniciada | Adicionar conta em itens de fardos e caixas
Tarefa: permitir contas simples nos campos de quantidade e preÃ§o durante a confirmaÃ§Ã£o manual de itens de fardos e caixas.
PÃ¡ginas lidas: wiki-index.md, wiki-log.md

## [2026-06-08] pÃ³s-tarefa | Permitir conta na ediÃ§Ã£o manual de itens
PÃ¡ginas tÃ©cnicas atualizadas: nenhuma
PÃ¡ginas de domÃ­nio atualizadas: dominio/item.md
PÃ¡ginas de histÃ³rico atualizadas: nenhuma
Aprendizados registrados: a conta Ã© resolvida no parsing antes da reavaliaÃ§Ã£o do item, o que preserva as regras de revisÃ£o e multiplicaÃ§Ã£o jÃ¡ aplicadas a fardos e caixas.

## [2026-06-08] sessÃ£o-iniciada | Ajustar conta na quantidade e preview
Tarefa: corrigir a conta no campo de quantidade e exibir preview do resultado antes de salvar.
PÃ¡ginas lidas: wiki-index.md, wiki-log.md

## [2026-06-08] pÃ³s-tarefa | Exibir preview e aceitar x na quantidade
PÃ¡ginas tÃ©cnicas atualizadas: nenhuma
PÃ¡ginas de domÃ­nio atualizadas: dominio/item.md
PÃ¡ginas de histÃ³rico atualizadas: nenhuma
Aprendizados registrados: operadores tendem a usar `x` na quantidade; o preview em tela reduz erro antes da confirmaÃ§Ã£o manual dos itens de fardos e caixas.

## [2026-06-08] sessÃ£o-iniciada | Corrigir preview invisÃ­vel e conta da quantidade
Tarefa: fazer o preview aparecer, aceitar divisÃ£o na quantidade com unidade misturada no texto e corrigir o contraste do formulÃ¡rio inline.
PÃ¡ginas lidas: wiki-index.md, wiki-log.md

## [2026-06-08] pÃ³s-tarefa | Ajustar preview e contraste da ediÃ§Ã£o inline
PÃ¡ginas tÃ©cnicas atualizadas: nenhuma
PÃ¡ginas de domÃ­nio atualizadas: dominio/item.md
PÃ¡ginas de histÃ³rico atualizadas: nenhuma
Aprendizados registrados: a ediÃ§Ã£o inline em tema escuro precisava de fundo prÃ³prio; a quantidade deve aceitar expressÃµes mesmo quando a unidade aparece no meio, como `20 Unidades / 6`.
## [2026-06-08] pós-tarefa | Permitir texto puro na quantidade
Páginas técnicas atualizadas: nenhuma
Páginas de domínio atualizadas: dominio/item.md
Páginas de histórico atualizadas: historico/bugs-resolvidos.md
Aprendizados registrados: a quantidade agora aceita texto puro de unidade, como `Caixas` e `Fardos`, usando base `1` quando não há número; o preview ajuda a validar a conversão antes de salvar.

## [2026-06-08] pós-tarefa | Permitir conta com unidade textual no meio
Páginas técnicas atualizadas: nenhuma
Páginas de domínio atualizadas: dominio/item.md
Páginas de histórico atualizadas: historico/bugs-resolvidos.md
Aprendizados registrados: palavras de unidade como `caixas` e `fardos` não podem deixar o `x` interno virar operador; a sanitização agora remove a palavra inteira e preserva só o `x` isolado quando ele é multiplicação de verdade.

## [2026-06-08] pós-tarefa | Manter preview ao reabrir edição
Páginas técnicas atualizadas: nenhuma
Páginas de domínio atualizadas: dominio/item.md
Páginas de histórico atualizadas: historico/bugs-resolvidos.md
Aprendizados registrados: o preview da conta precisa acompanhar o fluxo de fardos e caixas mesmo depois de salvar e reabrir a linha; a renderização agora é disparada de novo ao abrir a edição.

## [2026-06-08] pós-tarefa | Preservar conta ao reabrir a edição
Páginas técnicas atualizadas: nenhuma
Páginas de domínio atualizadas: dominio/item.md
Páginas de histórico atualizadas: historico/bugs-resolvidos.md
Aprendizados registrados: os preços da edição precisam guardar a expressão original da conta, não só o valor final calculado; assim o operador valida a conversão no preview antes de confirmar, enquanto a tabela continua mostrando apenas o resultado convertido.

## [2026-06-09] pós-tarefa | Liberar exportação com pendências e preservar fonte mesclada
Páginas técnicas atualizadas: tecnico/arquitetura.md
Páginas de domínio atualizadas: dominio/exportacao.md, dominio/item.md
Páginas de histórico atualizadas: historico/bugs-resolvidos.md
Aprendizados registrados: a exportação pode seguir mesmo com pendências quando o CSV carrega status, riscos e bloqueios de forma explícita; na importação XLSX/XLSM, `mergeCells` precisa ser expandido para não perder `fonte` e `vigencia` nas linhas subsequentes.

## [2026-06-09] pós-tarefa | Aplicar paleta do EPS da marca no site
Páginas técnicas atualizadas: tecnico/arquitetura.md
Páginas de domínio atualizadas: nenhuma
Páginas de histórico atualizadas: nenhuma
Aprendizados registrados: o EPS preferencial da marca usa amarelo puro em CMYK e um vermelho escuro institucional; a melhor combinação no site veio de fundo claro quente, paineis brancos, destaque amarelo e acoes principais em vermelho para manter contraste e identidade.

## [2026-06-09] pós-tarefa | Trocar a logo do topo pela versão secundária
Páginas técnicas atualizadas: tecnico/arquitetura.md
Páginas de domínio atualizadas: nenhuma
Páginas de histórico atualizadas: nenhuma
Aprendizados registrados: o EPS secundário já traz uma prévia embutida que pode ser exportada para PNG e reutilizada no cabeçalho sem alterar a estrutura do layout.

## [2026-06-09] pós-tarefa | Ajustar títulos e densidade visual da campanha e do catálogo
Páginas técnicas atualizadas: tecnico/arquitetura.md
Páginas de domínio atualizadas: nenhuma
Páginas de histórico atualizadas: nenhuma
Aprendizados registrados: a campanha fica visualmente melhor com células em linha única quando já existe rolagem horizontal; no catálogo, remover o scroll interno da lista principal evita o grande espaço em branco ao lado da navegação.

## [2026-06-09] pós-tarefa | Reequilibrar a rolagem da lista do catálogo
Páginas técnicas atualizadas: tecnico/arquitetura.md
Páginas de domínio atualizadas: nenhuma
Páginas de histórico atualizadas: nenhuma
Aprendizados registrados: no catálogo, a melhor navegação veio de manter a lista com barra própria, mas com altura maior e presa ao painel de resultados, em vez de delegar a rolagem à página inteira.

## [2026-06-09] pós-tarefa | Corrigir separação entre barra da página e barra da lista
Páginas técnicas atualizadas: tecnico/arquitetura.md
Páginas de domínio atualizadas: nenhuma
Páginas de histórico atualizadas: nenhuma
Aprendizados registrados: a rolagem própria da lista depende tanto do CSS quanto da classe do painel de resultados; se a classe some da marcação, a página volta a absorver a navegação vertical da lista.

## [2026-06-09] pós-tarefa | Tornar explícita a barra própria dos itens do catálogo
Páginas técnicas atualizadas: tecnico/arquitetura.md
Páginas de domínio atualizadas: nenhuma
Páginas de histórico atualizadas: nenhuma
Aprendizados registrados: para manter duas barras realmente distintas, o painel da direita precisa limitar sua altura e esconder o excesso, enquanto somente o contêiner da lista recebe `overflow-y: scroll` e contenção de rolagem.

## [2026-06-09] pós-tarefa | Ajustar o detalhe da pagina de campanhas
Páginas técnicas atualizadas: tecnico/arquitetura.md
Páginas de domínio atualizadas: nenhuma
Páginas de histórico atualizadas: historico/bugs-resolvidos.md
Aprendizados registrados: o detalhe da campanha fica mais claro quando a importacao permanece apenas na criacao; no grid de itens, manter `nowrap` como padrao e abrir excecao so nas descricoes preserva a leitura sem espalhar a tabela.

## [2026-06-09] pós-tarefa | Recuar os botoes de acao na campanha
Páginas técnicas atualizadas: tecnico/arquitetura.md
Páginas de domínio atualizadas: nenhuma
Páginas de histórico atualizadas: nenhuma
Aprendizados registrados: no bloco de preco final e quantidade, um pequeno recuo lateral e menos espacamento entre botoes deixa as acoes mais integradas ao conteudo sem alterar a hierarquia da linha.

## [2026-06-09] pós-tarefa | Baixar os botoes dentro da celula final da campanha
Páginas técnicas atualizadas: tecnico/arquitetura.md
Páginas de domínio atualizadas: nenhuma
Páginas de histórico atualizadas: nenhuma
Aprendizados registrados: na coluna final da campanha, transformar o resumo em pilha flex e deixar as acoes com `margin-top: auto` ajuda a empurrar os botoes para o fundo da celula quando a linha tem espaco vertical sobrando.

## [2026-06-09] pós-tarefa | Puxar o bloco de acoes mais para a esquerda
Páginas técnicas atualizadas: tecnico/arquitetura.md
Páginas de domínio atualizadas: nenhuma
Páginas de histórico atualizadas: nenhuma
Aprendizados registrados: para esse layout, ampliar o `margin-left` negativo do grupo de acoes reposiciona o bloco sem alterar a ordem visual nem o espaçamento vertical da celula.

## [2026-06-09] pós-tarefa | Mover os botoes para baixo da descricao Solidus
Páginas técnicas atualizadas: tecnico/arquitetura.md
Páginas de domínio atualizadas: nenhuma
Páginas de histórico atualizadas: nenhuma
Aprendizados registrados: no detalhe da campanha, deixar as acoes logo abaixo da descricao Solidus melhora o aproveitamento da linha e tira o peso visual da coluna de preco final.

## [2026-06-09] pós-tarefa | Liberar quebra na coluna de riscos da campanha
Páginas técnicas atualizadas: tecnico/arquitetura.md
Páginas de domínio atualizadas: nenhuma
Páginas de histórico atualizadas: nenhuma
Aprendizados registrados: na tabela de campanha, manter `nowrap` global e abrir excecao so na celula de riscos permite quebrar os badges sem desalinhar as colunas de pendencias, preco e codigo.

## [2026-06-09] pós-tarefa | Mover a logo do cabeçalho para o rodapé
Páginas técnicas atualizadas: tecnico/arquitetura.md
Páginas de domínio atualizadas: nenhuma
Páginas de histórico atualizadas: nenhuma
Aprendizados registrados: ao transformar a marca institucional em elemento fixo de rodapé, o cabeçalho fica mais limpo e o layout precisa reservar folga extra no fim do conteúdo para evitar sobreposição visual.

## [2026-06-09] pós-tarefa | Dar respiro ao bloco de busca do catálogo
Páginas técnicas atualizadas: tecnico/arquitetura.md
Páginas de domínio atualizadas: nenhuma
Páginas de histórico atualizadas: nenhuma
Aprendizados registrados: no topo do catálogo, um espaçamento inferior dedicado ao formulário de busca evita que os botões pareçam colados ao início da lista sem interferir no restante dos formulários da aplicação.

## [2026-06-09] pós-tarefa | Mover a busca para o topo do catálogo
Páginas técnicas atualizadas: tecnico/arquitetura.md
Páginas de domínio atualizadas: nenhuma
Páginas de histórico atualizadas: nenhuma
Aprendizados registrados: no catálogo, deixar a busca junto do título dos resultados reduz a competição visual com a navegação lateral sem mudar a combinação entre texto buscado e categoria selecionada.

## [2026-06-09] pós-tarefa | Revisar gramática e rótulos da UI
Páginas técnicas atualizadas: tecnico/arquitetura.md
Páginas de domínio atualizadas: nenhuma
Páginas de histórico atualizadas: historico/bugs-resolvidos.md
Aprendizados registrados: a interface ficou mais estável quando a camada de renderização assumiu a normalização de badges, filtros, auditoria e papéis visíveis, evitando depender da correção imediata dos dados legados.

## [2026-06-10] pós-tarefa | Revisar o pacote e consolidar commit local
Páginas técnicas atualizadas: nenhuma
Páginas de domínio atualizadas: nenhuma
Páginas de histórico atualizadas: nenhuma
Aprendizados registrados: a consolidação final deste pacote ficou mais segura ao separar arquivos de operação local e insumos brutos de marca dos artefatos realmente necessários para o produto, documentação e testes.

## [2026-06-10] pós-tarefa | Adicionar gráfico de pizza ao catálogo
Páginas técnicas atualizadas: tecnico/arquitetura.md
Páginas de domínio atualizadas: nenhuma
Páginas de histórico atualizadas: nenhuma
Aprendizados registrados: no catálogo, o gráfico de categorias precisa usar apenas os itens visíveis da lista atual e ficar fora do contêiner rolável da listagem, para não quebrar a separação entre a barra da página e a barra própria dos itens.

## [2026-06-10] pós-tarefa | Rebaixar o gráfico e restaurar a altura da lista do catálogo
Páginas técnicas atualizadas: tecnico/arquitetura.md
Páginas de domínio atualizadas: nenhuma
Páginas de histórico atualizadas: nenhuma
Aprendizados registrados: no catálogo, mesmo um resumo visual pequeno pode comprimir demais a listagem quando compartilha o mesmo painel flex; a solução mais estável foi mover o gráfico para um painel independente logo abaixo da grade principal.

## [2026-06-10] pós-tarefa | Alinhar a altura da lista de itens com a lista de categorias
Páginas técnicas atualizadas: tecnico/arquitetura.md
Páginas de domínio atualizadas: nenhuma
Páginas de histórico atualizadas: nenhuma
Aprendizados registrados: no catálogo, a altura fixa do painel de resultados criava um vazio visual quando poucos itens eram exibidos; usar na lista de itens o mesmo limite de altura da lista de categorias remove o espaço morto e mantém a rolagem independente.

## [2026-06-10] pós-tarefa | Unir lista e gráfico na mesma coluna do catálogo
Páginas técnicas atualizadas: tecnico/arquitetura.md
Páginas de domínio atualizadas: nenhuma
Páginas de histórico atualizadas: nenhuma
Aprendizados registrados: no catálogo, enquanto lista e gráfico ficaram fora da mesma coluna de grid, a coluna direita continuou sofrendo com vazio herdado da altura da coluna esquerda; encapsular ambos em `catalog-main-column` fez o gráfico começar imediatamente após a listagem.

## [2026-06-10] pós-tarefa | Ajustar o alinhamento fino entre os blocos do catálogo
Páginas técnicas atualizadas: tecnico/arquitetura.md
Páginas de domínio atualizadas: nenhuma
Páginas de histórico atualizadas: nenhuma
Aprendizados registrados: depois do ajuste estrutural da coluna direita, ainda restou uma diferença visual mínima no topo entre `Navegacao` e `Itens do catalogo`; um recuo vertical de `1px` na coluna direita resolveu o desalinhamento sem afetar o mobile.

## [2026-06-10] pós-tarefa | Corrigir o alvo do ajuste fino no painel do catálogo
Páginas técnicas atualizadas: tecnico/arquitetura.md
Páginas de domínio atualizadas: nenhuma
Páginas de histórico atualizadas: nenhuma
Aprendizados registrados: o desalinhamento residual não estava na coluna direita inteira, mas no painel `Itens do catalogo`; mover somente `catalog-results-panel` foi mais preciso e evitou deslocar o bloco do grafico logo abaixo.

## [2026-06-10] pós-tarefa | Recalibrar o deslocamento fino do painel de itens
Páginas técnicas atualizadas: tecnico/arquitetura.md
Páginas de domínio atualizadas: nenhuma
Páginas de histórico atualizadas: nenhuma
Aprendizados registrados: em telas onde a borda ainda parecia ligeiramente mais baixa, o deslocamento de `-2px` ainda não bastava; aumentar para `-3px` resolveu melhor a leitura do topo entre `Navegacao` e `Itens do catalogo`.

## [2026-06-10] pós-tarefa | Adicionar conversão amigável de regras para regex
Páginas técnicas atualizadas: tecnico/arquitetura.md
Páginas de domínio atualizadas: nenhuma
Páginas de histórico atualizadas: nenhuma
Aprendizados registrados: no fluxo de regras, a melhor ergonomia veio de salvar separadamente a escrita amigável e o regex final; assim o operador edita exemplos naturais como `Fardo ou FD`, enquanto o matching continua usando uma expressão regular segura e compatível com as regras antigas.

## [2026-06-10] pós-tarefa | Corrigir regra amigável que não acionava revisão
Páginas técnicas atualizadas: tecnico/arquitetura.md
Páginas de domínio atualizadas: nenhuma
Páginas de histórico atualizadas: historico/bugs-resolvidos.md
Aprendizados registrados: no conversor de regras, `|` precisava continuar sendo aceito como separador amigável sem derrubar o suporte a regex literal completo; separar melhor esses dois caminhos evitou que critérios válidos fossem ignorados na revisão.

## [2026-06-10] pós-tarefa | Sincronizar campanhas antigas após corrigir regra semântica
Páginas técnicas atualizadas: tecnico/arquitetura.md
Páginas de domínio atualizadas: nenhuma
Páginas de histórico atualizadas: historico/bugs-resolvidos.md
Aprendizados registrados: corrigir apenas `conversion_rules` não basta quando a campanha já foi importada; para regras semânticas como `descricao solidus contem CERV`, o startup precisa reavaliar itens pendentes com a mesma lógica da importação, preservando decisões humanas já aprovadas ou rejeitadas.

## [2026-06-10] pós-tarefa | Remover fundo branco da logo do rodape
Páginas técnicas atualizadas: tecnico/arquitetura.md
Páginas de domínio atualizadas: nenhuma
Páginas de histórico atualizadas: nenhuma
Aprendizados registrados: para esse ajuste visual bastou regravar o PNG publico com transparência real, mantendo o mesmo caminho em `wwwroot` e evitando mudanças no HTML ou no CSS do rodape.

## [2026-06-10] pós-tarefa | Criar novo manual visual a partir da marca redonda
Páginas técnicas atualizadas: tecnico/arquitetura.md
Páginas de domínio atualizadas: nenhuma
Páginas de histórico atualizadas: nenhuma
Aprendizados registrados: mesmo sem extrator dedicado de PDF no ambiente, foi possivel reconstruir um manual de identidade util ao produto reaproveitando a organizacao do manual original e definindo a paleta oficial diretamente da arte `Clube Das Ofertas fundo redondo.png`, com preto, amarelo, vermelho e branco como base do site.

## [2026-06-10] pós-tarefa | Documentar encerramento completo do servidor local
Páginas técnicas atualizadas: tecnico/arquitetura.md
Páginas de domínio atualizadas: nenhuma
Páginas de histórico atualizadas: nenhuma
Aprendizados registrados: em Windows, `dotnet run` pode deixar `ClubeDasOfertas.Web.exe` ativo mesmo após a sessao aparentar ter encerrado; o README passou a orientar a parada do host, a limpeza do executavel residual e a checagem final dos processos.

## [2026-06-10] pós-tarefa | Aplicar a nova identidade visual no tema do site
Páginas técnicas atualizadas: tecnico/arquitetura.md
Páginas de domínio atualizadas: nenhuma
Páginas de histórico atualizadas: nenhuma
Aprendizados registrados: a troca de identidade ficou mais segura ao centralizar a reestilizacao em `HtmlView.cs`, atualizando tokens globais e ajustando depois apenas os overrides da pagina de campanhas; isso evitou que o cabecalho, os botoes e os badges seguissem paletas divergentes.

## [2026-06-10] pós-tarefa | Melhorar busca da campanha e a tabela de regras
Páginas técnicas atualizadas: tecnico/arquitetura.md
Páginas de domínio atualizadas: nenhuma
Páginas de histórico atualizadas: nenhuma
Aprendizados registrados: a busca textual da campanha precisou entrar no mesmo fluxo de filtros e mutacoes dos itens para nao perder contexto durante aprovacoes e edicoes; na tela de regras, a solucao mais limpa foi dar um layout proprio para a tabela, com quebra de linha no criterio e acao explicita de exclusao.

## [2026-06-10] pós-tarefa | Refinar compactacao da lista de campanhas e previa das regras
Páginas técnicas atualizadas: tecnico/arquitetura.md
Páginas de domínio atualizadas: nenhuma
Páginas de histórico atualizadas: nenhuma
Aprendizados registrados: para eliminar a barra horizontal residual da lista de campanhas foi necessario reduzir larguras minimas e compactar as acoes; para as regras, a melhor leitura veio de uma previa limitada com expansao no hover, em vez de deixar a quebra de linha aberta o tempo todo.
## [2026-06-11] sessão-iniciada | Ajustar coluna de pendências na página da campanha
Tarefa: compactar a lista resumida de campanhas para manter a coluna de pendências visível sem rolagem horizontal desnecessária.
Páginas lidas: wiki-index.md, wiki-log.md, tecnico/arquitetura.md, historico/bugs-resolvidos.md

## [2026-06-11] pós-tarefa | Compactar a lista de campanhas
Páginas técnicas atualizadas: tecnico/arquitetura.md
Páginas de domínio atualizadas: nenhuma
Páginas de histórico atualizadas: historico/bugs-resolvidos.md
Aprendizados registrados: na lista resumida de campanhas, a coluna de pendências só volta a caber de forma estável quando a compactação considera em conjunto grid, badges, textos auxiliares e botões, inclusive sob os overrides tipográficos da page-campaign.

## [2026-06-11] sessão-iniciada | Enxugar a lista principal de regras
Tarefa: deixar na tela principal de regras apenas status, nome, como acontece, multiplicador e ações, movendo o restante para a edição inline.
Páginas lidas: wiki-index.md, wiki-log.md, tecnico/arquitetura.md

## [2026-06-11] pós-tarefa | Simplificar colunas da página de regras
Páginas técnicas atualizadas: tecnico/arquitetura.md
Páginas de domínio atualizadas: nenhuma
Páginas de histórico atualizadas: nenhuma
Aprendizados registrados: na tela de regras, a leitura rápida fica mais útil quando a grade principal mostra só status, identificação, gatilho e multiplicador; campos de configuração detalhada funcionam melhor dentro do formulário de edição inline.

## [2026-06-11] sessão-iniciada | Subir a informação exibida na página de regras
Tarefa: aproximar visualmente a informação expandida de "Como acontece" da própria regra na listagem.
Páginas lidas: wiki-index.md, wiki-log.md, tecnico/arquitetura.md

## [2026-06-11] pós-tarefa | Ajustar posição da informação expandida em regras
Páginas técnicas atualizadas: tecnico/arquitetura.md
Páginas de domínio atualizadas: nenhuma
Páginas de histórico atualizadas: nenhuma
Aprendizados registrados: na listagem de regras, a leitura da previa expandida fica mais natural quando o balao abre acima do criterio, em vez de descer para longe da linha correspondente.

## [2026-06-11] sessão-iniciada | Recalibrar a prévia expandida das regras
Tarefa: trazer a informação de "Como acontece" para perto da regra sem cobrir a primeira linha da tabela.
Páginas lidas: wiki-index.md, wiki-log.md, tecnico/arquitetura.md

## [2026-06-11] pós-tarefa | Recalibrar a posição da prévia em regras
Páginas técnicas atualizadas: tecnico/arquitetura.md
Páginas de domínio atualizadas: nenhuma
Páginas de histórico atualizadas: nenhuma
Aprendizados registrados: para preservar a leitura da primeira regra, a prévia precisa ficar perto do critério, mas sem sair da faixa visual da própria linha.

## [2026-06-11] pós-tarefa | Transformar tooltip das regras em overlay flutuante
Páginas técnicas atualizadas: tecnico/arquitetura.md
Páginas de domínio atualizadas: nenhuma
Páginas de histórico atualizadas: nenhuma
Aprendizados registrados: para que a informação deixe de parecer parte da tabela, o balão precisa sair do fluxo do `tablewrap` e ser posicionado como overlay fixo no viewport; assim a última regra não cria barra de rolagem extra.

## [2026-06-11] sessão-iniciada | Centralizar a tooltip sobre a própria regra
Tarefa: manter a informação da regra flutuante, mas posicionada no centro da própria célula, sem subir ou descer.
Páginas lidas: wiki-index.md, wiki-log.md, tecnico/arquitetura.md

## [2026-06-11] pós-tarefa | Centralizar a tooltip sobre a regra
Páginas técnicas atualizadas: tecnico/arquitetura.md
Páginas de domínio atualizadas: nenhuma
Páginas de histórico atualizadas: nenhuma
Aprendizados registrados: quando o usuário quer uma informação sobreposta e não separada da tabela, o melhor ponto de ancoragem é o centro da própria célula, usando o overlay apenas para desenhar o contexto sem afetar o fluxo da lista.

## [2026-06-11] sessao-iniciada | Evitar tooltip vazia nas regras
Tarefa: fazer a informacao flutuante da regra receber o texto sem depender de atributos crus do HTML.
Páginas lidas: wiki-index.md, wiki-log.md, tecnico/arquitetura.md

## [2026-06-11] pós-tarefa | Corrigir tooltip vazia nas regras
Páginas técnicas atualizadas: tecnico/arquitetura.md
Páginas de domínio atualizadas: nenhuma
Páginas de histórico atualizadas: nenhuma
Aprendizados registrados: Base64 reduz o risco de perder o conteudo da tooltip quando a regra usa caracteres que podem ser mal interpretados no atributo HTML; o fallback para a preview da celula evita um balao vazio.

## [2026-06-11] sessao-iniciada | Voltar tooltip para HTML e CSS
Tarefa: remover a dependencia de JavaScript da tooltip das regras para eliminar a barra vazia e manter o balão sobre a própria célula.
Páginas lidas: wiki-index.md, wiki-log.md, tecnico/arquitetura.md

## [2026-06-11] pós-tarefa | Reverter tooltip para HTML e CSS
Páginas técnicas atualizadas: tecnico/arquitetura.md
Páginas de domínio atualizadas: nenhuma
Páginas de histórico atualizadas: nenhuma
Aprendizados registrados: quando a tooltip precisa ficar sobre a própria regra, o caminho mais estável é renderizar o balão no HTML e controlar a visibilidade só com CSS; assim o texto não depende de script e não fica vazia.

## [2026-06-11] pós-tarefa | Fixar tooltip das regras fora da tabela
Páginas técnicas atualizadas: tecnico/arquitetura.md
Páginas de domínio atualizadas: nenhuma
Páginas de histórico atualizadas: historico/bugs-resolvidos.md
Aprendizados registrados: o balão das regras ficou mais estável quando o texto completo saiu do fluxo do `tablewrap` e passou a ser desenhado como overlay fixo com conteúdo guardado em `<template>`, mantendo a leitura sobre a própria célula sem criar rolagem extra.

## [2026-06-12] sessão-iniciada | Ajustar fonte e paleta da lista de campanhas e do gráfico
Tarefa: aumentar discretamente a fonte da área de campanhas criadas, trocar a paleta do gráfico do catálogo e normalizar `Sobremesa` para `Sobremesas` na UI.
Páginas lidas: wiki-index.md, wiki-log.md, tecnico/arquitetura.md, historico/bugs-resolvidos.md

## [2026-06-12] pós-tarefa | Recalibrar tipografia e categorias do catálogo
Páginas técnicas atualizadas: tecnico/arquitetura.md
Páginas de domínio atualizadas: nenhuma
Páginas de histórico atualizadas: historico/bugs-resolvidos.md
Aprendizados registrados: a paleta do gráfico ficou mais estável quando passou a depender de um mapa fixo por categoria, e a correção visual de `Sobremesa` funcionou melhor como normalização de apresentação reaproveitada também no filtro lateral e na lista de itens.

## [2026-06-12] pós-tarefa | Tornar as mudanças do catálogo visíveis fora do gráfico
Páginas técnicas atualizadas: tecnico/arquitetura.md
Páginas de domínio atualizadas: nenhuma
Páginas de histórico atualizadas: nenhuma
Aprendizados registrados: quando a mudança de cor vive só no gráfico, ela pode passar despercebida; aplicar a mesma paleta na navegação lateral e nos cards dos itens torna a alteração de categoria imediatamente perceptível no catálogo.

## [2026-06-12] pós-tarefa | Canonizar categorias do catálogo antes de colorir e filtrar
Páginas técnicas atualizadas: tecnico/arquitetura.md
Páginas de domínio atualizadas: nenhuma
Páginas de histórico atualizadas: historico/bugs-resolvidos.md
Aprendizados registrados: a paleta só fica confiável quando filtro, agrupamento e exibição compartilham a mesma chave canônica de categoria; corrigir só o texto renderizado não basta quando os valores reais do banco variam entre singular, plural ou acentuação.

## [2026-06-12] pós-tarefa | Separar Frios de Hortifruti e restaurar Todos os itens
Páginas técnicas atualizadas: tecnico/arquitetura.md
Páginas de domínio atualizadas: nenhuma
Páginas de histórico atualizadas: historico/bugs-resolvidos.md
Aprendizados registrados: no catálogo, unir categorias próximas visualmente pode esconder filtros importantes; manter `Todos os itens` como entrada própria e preservar `Frios` separado de `Hortifruti` devolve legibilidade e deixa a paleta mais fiel ao domínio.

## [2026-06-12] pós-tarefa | Fazer Todos os itens voltar a agir como ausência de filtro
Páginas técnicas atualizadas: tecnico/arquitetura.md
Páginas de domínio atualizadas: nenhuma
Páginas de histórico atualizadas: historico/bugs-resolvidos.md
Aprendizados registrados: quando um rótulo de interface também circula como valor de formulário, ele precisa ser normalizado de volta para o estado sem filtro; caso contrário, a própria label vira uma pseudo-categoria e esvazia a consulta.

## [2026-06-12] pós-tarefa | Recriar o gráfico em SVG para hover por fatia
Páginas técnicas atualizadas: tecnico/arquitetura.md
Páginas de domínio atualizadas: nenhuma
Páginas de histórico atualizadas: historico/bugs-resolvidos.md
Aprendizados registrados: para donut com tooltip por segmento, `conic-gradient` é bom para resumo estático, mas SVG com strokes separados entrega bordas mais limpas e interação confiável em cada categoria.

## [2026-06-12] pós-tarefa | Remover recorte branco entre as fatias do gráfico
Páginas técnicas atualizadas: tecnico/arquitetura.md
Páginas de domínio atualizadas: nenhuma
Páginas de histórico atualizadas: historico/bugs-resolvidos.md
Aprendizados registrados: quando o donut usa gaps artificiais para separar fatias, o fundo claro pode vazar e parecer defeito; para este layout, foi melhor manter as fatias encostadas e deixar a leitura apoiada pelo hover e pela legenda.

## [2026-06-12] pós-tarefa | Incluir itens manualmente em campanhas ja criadas

Paginas tecnicas atualizadas: tecnico/arquitetura.md
Paginas de dominio atualizadas: dominio/campanha.md, dominio/item.md
Paginas de historico atualizadas: nenhuma
Aprendizados registrados: a inclusao manual precisa seguir o mesmo caminho de avaliacao da importacao para manter regras, auditoria e exportacao coerentes, mas deve marcar a origem explicitamente como `Manual`.

## [2026-06-12] pós-tarefa | Regras de fardo e caixa dividem a quantidade

Paginas tecnicas atualizadas: tecnico/arquitetura.md
Paginas de dominio atualizadas: dominio/item.md
Paginas de historico atualizadas: nenhuma
Aprendizados registrados: se a regra ja multiplica o preco pelo fator da embalagem, a quantidade limitada precisa usar o mesmo fator no sentido inverso para que a lista e o CSV reflitam a unidade comercial correta.

## [2026-06-12] sessao-iniciada | Diagnosticar falha de startup no PostgreSQL local
Tarefa: investigar a excecao de conexao do Npgsql no startup e corrigir o diagnostico operacional da inicializacao.
Paginas lidas: wiki-index.md, wiki-log.md, tecnico/arquitetura.md, historico/bugs-resolvidos.md

## [2026-06-12] pós-tarefa | Tornar a falha de conexao do PostgreSQL mais acionavel
Paginas tecnicas atualizadas: tecnico/arquitetura.md
Paginas de dominio atualizadas: nenhuma
Paginas de historico atualizadas: historico/bugs-resolvidos.md
Aprendizados registrados: quando o banco local nao esta disponivel, a aplicacao precisa falhar com uma mensagem que aponte o pre-requisito faltante; no nosso ambiente, `localhost` tambem merece ser normalizado para IPv4 para evitar diagnosticos enganadores em `::1`.

## [2026-06-12] sessao-iniciada | Inspecionar e validar alteracoes antes do commit
Tarefa: revisar o worktree atual, validar build e testes e registrar um commit com o estado aprovado.
Paginas lidas: wiki-index.md, wiki-log.md

## [2026-06-12] pós-tarefa | Validar alteracoes e preparar commit
Paginas tecnicas atualizadas: nenhuma
Paginas de dominio atualizadas: nenhuma
Paginas de historico atualizadas: nenhuma
Aprendizados registrados: o conjunto atual de alteracoes compilou com sucesso; a suite local passou via `dotnet run --no-build` porque havia um binario de web app em uso por outro processo durante a tentativa com rebuild.

## [2026-06-12] sessao-iniciada | Diagnosticar aplicacao fora do ar apos publicacao
Tarefa: verificar por que a aplicacao deixou de iniciar no ambiente local e corrigir a causa quando possivel.
Paginas lidas: wiki-index.md, wiki-log.md, tecnico/arquitetura.md, historico/bugs-resolvidos.md

## [2026-06-12] pós-tarefa | Identificar banco local parado como causa da indisponibilidade
Paginas tecnicas atualizadas: nenhuma
Paginas de dominio atualizadas: nenhuma
Paginas de historico atualizadas: nenhuma
Aprendizados registrados: a aplicacao estava saudavel do lado do codigo; o bloqueio atual veio do servico `postgresql-x64-16`, que existe na maquina mas estava parado e exige privilegio administrativo para ser reiniciado.

## [2026-06-25] sessao-iniciada | Corrigir borda branca no rodape
Tarefa: ajustar as dimensoes da pagina para evitar sobra branca no rodape em diferentes resolucoes de tela.
Paginas lidas: wiki-index.md, wiki-log.md, WIKI_GOVERNANCE.md, tecnico/arquitetura.md, historico/bugs-resolvidos.md

## [2026-06-25] pós-tarefa | Corrigir borda branca no rodape
Paginas tecnicas atualizadas: tecnico/arquitetura.md
Paginas de dominio atualizadas: nenhuma
Paginas de historico atualizadas: historico/bugs-resolvidos.md
Aprendizados registrados: quando um elemento decorativo fixo muda de tamanho conforme a viewport, a folga do layout nao pode ficar hardcoded; atrelar o espaco ao tamanho real da marca elimina a sobra branca sem perder o respiro do rodape.


## [2026-06-25] sessao-iniciada | Tornar menu lateral expansivel por hover
Tarefa: trocar o menu de clique por um painel lateral dinamico que expande no hover, mantendo so o icone do menu quando recolhido.
Paginas lidas: wiki-index.md, wiki-log.md, tecnico/arquitetura.md

## [2026-06-25] pós-tarefa | Tornar menu lateral expansivel por hover
Paginas tecnicas atualizadas: tecnico/arquitetura.md
Paginas de dominio atualizadas: nenhuma
Paginas de historico atualizadas: nenhuma
Aprendizados registrados: para um menu recolhido que precisa funcionar tanto em desktop quanto em toque, o hover pode cuidar da expansao visual principal, mas um estado `data-open` com `aria-expanded` evita regressao de acessibilidade e nao exige um dropdown separado.


## [2026-06-25] sessao-iniciada | Recolocar menu no cabecalho com painel no hover
Tarefa: manter o menu no cabecalho, preservar o titulo da pagina ao centro e abrir o painel de navegacao ao passar o cursor sobre o botao do menu.
Paginas lidas: wiki-index.md, wiki-log.md, tecnico/arquitetura.md

## [2026-06-25] pós-tarefa | Recolocar menu no cabecalho com painel no hover
Paginas tecnicas atualizadas: tecnico/arquitetura.md
Paginas de dominio atualizadas: nenhuma
Paginas de historico atualizadas: nenhuma
Aprendizados registrados: para manter o titulo realmente no cabecalho e ainda oferecer hover navigation, o caminho mais estavel foi usar um trigger compacto no grid do header e deixar o painel como dropdown absoluto ancorado no proprio botao, em vez de um trilho lateral.


## [2026-06-25] sessao-iniciada | Corrigir fechamento prematuro do menu no hover
Tarefa: impedir que o painel do menu feche quando o cursor sai do botao e vai em direcao aos links do painel.
Paginas lidas: wiki-index.md, wiki-log.md

## [2026-06-25] pós-tarefa | Corrigir fechamento prematuro do menu no hover
Paginas tecnicas atualizadas: nenhuma
Paginas de dominio atualizadas: nenhuma
Paginas de historico atualizadas: historico/bugs-resolvidos.md
Aprendizados registrados: em menus abertos por hover, um espacamento visual entre gatilho e painel precisa ser coberto por uma area de transicao; sem essa ponte, o dropdown fecha no caminho mesmo quando a logica do hover esta correta.

## [2026-06-25] consulta | Onde encaixar cadastro manual de item no catalogo
Paginas consultadas: wiki-index.md, wiki-log.md, tecnico/arquitetura.md, dominio/item.md, FLUXOGRAMA.md
Pagina de sintese criada: nenhuma

## [2026-06-25] sessao-iniciada | Adicionar cadastro manual no catalogo
Tarefa: permitir cadastrar item manualmente na propria tela do catalogo, preservando busca e categoria atuais apos salvar.
Paginas lidas: wiki-index.md, wiki-log.md, tecnico/arquitetura.md, dominio/item.md, FLUXOGRAMA.md

## [2026-06-25] pós-tarefa | Adicionar cadastro manual no catalogo
Paginas tecnicas atualizadas: tecnico/arquitetura.md
Paginas de dominio atualizadas: nenhuma
Paginas de historico atualizadas: nenhuma
Aprendizados registrados: como o catalogo ja centraliza importacao, busca e navegacao por categoria, o fluxo manual ficou mais natural como formulario inline na mesma sidebar; reaproveitar o mesmo upsert da importacao reduziu o risco de divergencia entre cadastro manual e carga em massa.

## [2026-06-25] sessao-iniciada | Refinar seletor de categoria do cadastro manual
Tarefa: trocar o campo nativo de categoria por um painel visual com as categorias disponiveis e a mesma dinamica cromatica do catalogo.
Paginas lidas: wiki-index.md, wiki-log.md

## [2026-06-25] pós-tarefa | Refinar seletor de categoria do cadastro manual
Paginas tecnicas atualizadas: tecnico/arquitetura.md
Paginas de dominio atualizadas: nenhuma
Paginas de historico atualizadas: nenhuma
Aprendizados registrados: para categorias conhecidas do catalogo, um seletor visual com painel proprio comunica melhor a paleta e evita a aparencia crua do dropdown nativo; manter um fallback simples quando a lista estiver vazia protege o primeiro cadastro do ambiente.


## [2026-06-25] consulta | Escolher formato da exportacao para tabloide e envio
Paginas consultadas: wiki-index.md, wiki-log.md, dominio/exportacao.md
Pagina de sintese criada: nenhuma

## [2026-06-25] pós-tarefa | Permitir selecao de colunas na exportacao CSV
Paginas tecnicas atualizadas: tecnico/arquitetura.md
Paginas de dominio atualizadas: dominio/exportacao.md
Paginas de historico atualizadas: nenhuma
Aprendizados registrados: a mesma campanha pode exigir saídas diferentes para CRM e para circulação operacional; manter um catálogo fixo de colunas exportáveis e deixar a UI apenas filtrar esse conjunto preserva compatibilidade do CSV sem engessar o operador.

## [2026-06-25] consulta | Analisar formula da aba do tabloide para cadastro no CRM
Paginas consultadas: wiki-index.md, wiki-log.md, dominio/exportacao.md, CHECK LIST - Clube Das Ofertas.xlsm
Pagina de sintese criada: nenhuma

## [2026-06-25] pós-tarefa | Adicionar presets de exportacao para lojas e interno
Paginas tecnicas atualizadas: tecnico/arquitetura.md
Paginas de dominio atualizadas: dominio/exportacao.md
Paginas de historico atualizadas: nenhuma
Aprendizados registrados: quando o mesmo CSV atende dois usos operacionais diferentes, presets leves na propria UI resolvem o dia a dia sem duplicar rotas; deixar a definicao dos presets no servico evita que a regra de negocio fique presa ao JavaScript da tela.

## [2026-06-25] pós-tarefa | Tornar a escolha de exportacao mais orientada por destino
Paginas tecnicas atualizadas: tecnico/arquitetura.md
Paginas de dominio atualizadas: dominio/exportacao.md
Paginas de historico atualizadas: nenhuma
Aprendizados registrados: em fluxo operacional, rotulos orientados por destino final ajudam mais do que nomes tecnicos; quando a regra ja esta estavel, ajustar a microcopia melhora a usabilidade sem alterar a logica.

## [2026-06-25] pós-tarefa | Adicionar botoes rapidos de exportacao por destino
Paginas tecnicas atualizadas: tecnico/arquitetura.md
Paginas de dominio atualizadas: dominio/exportacao.md
Paginas de historico atualizadas: nenhuma
Aprendizados registrados: quando ha um preset dominante por uso, um botao de envio direto reduz cliques e erro operacional; manter a exportacao personalizada ao lado preserva flexibilidade sem esconder o caminho padrao.

## [2026-06-25] pós-tarefa | Criar exportacao XLSX para lojas
Paginas tecnicas atualizadas: tecnico/arquitetura.md
Paginas de dominio atualizadas: dominio/exportacao.md
Paginas de historico atualizadas: nenhuma
Aprendizados registrados: quando o operador precisa proximidade visual com uma planilha modelo, o limite deixa de ser a selecao de colunas e passa a ser o proprio formato; manter `CSV` para CRM e adicionar `XLSX` para lojas separa bem os dois objetivos sem confundir o fluxo.

## [2026-06-25] pós-tarefa | Corrigir reparo do Excel no XLSX de lojas
Paginas tecnicas atualizadas: tecnico/arquitetura.md
Paginas de dominio atualizadas: dominio/exportacao.md
Paginas de historico atualizadas: historico/bugs-resolvidos.md
Aprendizados registrados: em exportacao OpenXML manual, pequenos desvios de ordem estrutural no `worksheet` ja bastam para o Excel abrir em modo de reparo; validar a ordem dos nos e higienizar texto para XML reduz esse risco sem depender de biblioteca externa.

## [2026-06-25] pós-tarefa | Trocar exportacao interna rapida de CSV para XLSX
Paginas tecnicas atualizadas: tecnico/arquitetura.md
Paginas de dominio atualizadas: dominio/exportacao.md
Paginas de historico atualizadas: nenhuma
Aprendizados registrados: quando o arquivo interno precisa espelhar a aba operacional do tabloide, vale mais gerar um `XLSX` aderente ao layout real do que insistir em um `CSV` completo; separar o caminho rapido em `XLSX` e manter o `CSV` apenas como exportacao personalizada preserva flexibilidade sem confundir o uso principal.

## [2026-06-25] pós-tarefa | Corrigir sobreposicao de texto no painel de exportacao
Paginas tecnicas atualizadas: nenhuma
Paginas de dominio atualizadas: nenhuma
Paginas de historico atualizadas: historico/bugs-resolvidos.md
Aprendizados registrados: componentes visuais baseados em `button` precisam neutralizar herancas como `white-space: nowrap` quando passam a carregar textos descritivos; sem isso, larguras intermediarias quebram a leitura mesmo quando o grid responsivo esta correto.

