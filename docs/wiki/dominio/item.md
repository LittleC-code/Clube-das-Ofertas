---
titulo: item
categoria: dominio
criado: 2026-06-08
atualizado: 2026-06-08
fontes: []
links: []
---

# Item

Na ediÃ§Ã£o manual de itens da campanha, campos de quantidade e preÃ§o podem aceitar contas simples quando o operador precisa confirmar conversÃµes de fardos e caixas.

- Escopo principal: itens em revisÃ£o com flags de `FARDO_CAIXA`, `FARDO` ou `CAIXA`.
- Exemplos aceitos: `20*6 Unidades`, `20x6 Unidades`, `83,88/6`, `71,88/6`, `(12*3)/2`.
- A conta Ã© resolvida antes da reavaliaÃ§Ã£o das regras, entÃ£o o fluxo continua aplicando matching, bloqueios, revisÃ£o obrigatÃ³ria e multiplicadores de conversÃ£o jÃ¡ existentes.
- A dica visual dessa capacidade aparece no formulÃ¡rio inline dos itens pendentes de fardos e caixas para nÃ£o poluir os demais casos.
- Antes de salvar, a tela mostra um preview do resultado calculado para quantidade, preÃ§o venda e preÃ§o clube, ajudando a validar a conversÃ£o manual.
- No uso esperado para fardos e caixas, a quantidade costuma ser ajustada com divisÃ£o, como `20/6`, enquanto os preÃ§os costumam ser ajustados com multiplicaÃ§Ã£o, como `13,98*6`.
- A quantidade também aceita texto puro de unidade, como `Caixas` ou `Fardos`, e também permite misturar a unidade com a conta, como `120 caixas / 6`, usando quantidade base `1` quando não há número na entrada.
- O preview da conta permanece visível sempre que o item entra no fluxo de fardos e caixas, mesmo depois de salvar e reabrir a edição, para validar novas conversões antes da confirmação.
- Ao reabrir a edição, os campos de preço voltam com a expressão original da conta salva em `price_sale_raw` e `price_club_raw`; a listagem principal continua exibindo apenas os valores finais convertidos.
- A `fonte` do item diferencia `Tabloide` e `App`; na importacao de planilhas prontas, esse valor precisa ser herdado de celulas mescladas para nao desaparecer nas linhas seguintes do mesmo bloco.
