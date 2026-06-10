---
titulo: exportacao
categoria: dominio
criado: 2026-06-09
atualizado: 2026-06-09
fontes: []
links: [item.md, ../historico/bugs-resolvidos.md, ../tecnico/arquitetura.md]
---

# Exportacao

A exportacao de campanha continua gerando um arquivo `CSV`, mas agora nao depende mais de todos os itens terem sido validados antes do download.

- Quando ainda existem pendencias, a interface avisa o operador antes da exportacao.
- O arquivo exportado passa a carregar contexto operacional adicional por item: `status_item`, `status_revisao`, `revisao_obrigatoria`, `riscos`, `pendencias`, `linha_origem`, `vigencia_original`, `preco_original_venda` e `preco_original_clube`.
- A coluna `fonte` segue presente no CSV para diferenciar itens de `Tabloide` e `App`.
- Como o formato final permanece `CSV`, nao existe suporte nativo para cor de fonte; o destaque visual de riscos e pendencias acontece na tabela da aplicacao, enquanto o arquivo usa colunas textuais explicitas.
