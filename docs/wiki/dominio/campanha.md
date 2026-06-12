---
titulo: campanha
categoria: dominio
criado: 2026-06-12
atualizado: 2026-06-12
fontes: []
links: [item.md, exportacao.md, ../tecnico/arquitetura.md]
---

# Campanha

Uma campanha nasce da importacao da planilha e pode receber itens manuais depois de criada, sem exigir uma nova importacao completa.

- O ciclo basico continua sendo `Rascunho` -> importacao -> `Importado` -> exportacao -> `Exportado`.
- A tela de detalhe da campanha agora oferece um formulario proprio para inclusao manual de itens.
- A inclusao manual reaproveita matching, regras de risco e revisao do mesmo fluxo da importacao, mas marca a origem do item como `Manual`.
- O item incluido manualmente usa o periodo da campanha como referencia de `vigencia_original`, mantendo a trilha operacional no CSV e no historico.
