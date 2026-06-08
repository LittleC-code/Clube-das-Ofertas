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
