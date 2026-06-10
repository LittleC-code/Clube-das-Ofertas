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
