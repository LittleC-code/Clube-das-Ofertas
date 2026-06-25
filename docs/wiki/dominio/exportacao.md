---
titulo: exportacao
categoria: dominio
criado: 2026-06-09
atualizado: 2026-06-25
fontes: []
links: [item.md, ../historico/bugs-resolvidos.md, ../tecnico/arquitetura.md]
---

# Exportacao

A exportacao de campanha continua gerando um arquivo `CSV`, mas agora nao depende mais de todos os itens terem sido validados antes do download.

- Quando ainda existem pendencias, a interface avisa o operador antes da exportacao.
- O arquivo exportado passa a carregar contexto operacional adicional por item: `status_item`, `status_revisao`, `revisao_obrigatoria`, `riscos`, `pendencias`, `linha_origem`, `vigencia_original`, `preco_original_venda` e `preco_original_clube`.
- A coluna `fonte` segue presente no CSV para diferenciar itens de `Tabloide` e `App`.
- A tela de campanha agora permite escolher quais colunas entram no CSV antes da exportacao; por padrao, todas continuam marcadas para preservar o comportamento historico.
- A tela de campanha agora tambem oferece dois presets operacionais:
  - `Exportar para lojas`: reduz a exportacao ao bloco equivalente ao envio externo (`fonte`, `vigencia_original`, `descricao_tabloide`, `quantidade`, `preco_venda`, `preco_clube`, `descricao_solidus`, `codigo_barras`);
  - `Exportar para uso interno e CRM`: volta ao conjunto completo de colunas disponiveis para revisao e apoio ao cadastro.
- Alem da selecao manual de colunas, a tela agora expoe botoes rapidos em `XLSX` para os dois destinos operacionais; o `CSV` continua disponivel apenas no modo personalizado, quando o operador quiser montar o proprio recorte de colunas.
- Como o formato final permanece `CSV`, nao existe suporte nativo para cor de fonte; o destaque visual de riscos e pendencias acontece na tabela da aplicacao, enquanto o arquivo usa colunas textuais explicitas.

## XLSX para lojas

Agora existem duas exportacoes especificas em `XLSX`: uma para lojas e outra para uso interno/CRM.

- O `XLSX` de lojas usa o mesmo bloco operacional equivalente a `A-H`.
- O `XLSX` interno replica o desenho operacional de `A` ate `T` da aba do tabloide, incluindo `tipo de codigo`, `contagem na base`, `vigencia interna`, `indicador de item no tabloide`, `concatenado`, `status`, `quantidade` e `unidade`.
- O arquivo de lojas sai com uma aba propria de tabloide, titulo visual da campanha, cabecalho estilizado e mesclagens basicas de `Fonte` e `Vigencia` quando os valores se repetem em sequencia.
- O arquivo interno sai com uma aba propria para CRM e apoio operacional, preservando a estrutura da planilha base para reduzir retrabalho manual antes do cadastro.
- A serializacao do arquivo precisa respeitar a ordem estrutural esperada pelo Excel no XML da aba; no formato atual, o `autoFilter` vem antes de `mergeCells`, e o conteudo textual das celulas e saneado para remover caracteres invalidos de XML antes da gravacao.

## Referencia da planilha de envio para lojas

A aba `Tabloide - 02.06.2026 F` da planilha `CHECK LIST - Clube Das Ofertas.xlsm` traz uma orientacao explicita no topo:

- para envio às lojas, usar principalmente as colunas equivalentes de `A` ate `H`;
- de `I` ate `T`, a propria planilha marca o conteudo como informacao interna da Rede.

Na leitura atual da aba, o bloco principal de envio fica assim:

- `A`: `Fonte`
- `B`: `Vigencia`
- `C`: `Descricao no Tabloide`
- `D`: `Quantidade limitada`
- `E`: `Venda`
- `F`: `Venda Clube`
- `G`: `Descricao Solidus`
- `H`: `Cod Barras`

As colunas seguintes aparecem como apoio operacional interno:

- `J`: tipo de codigo (`EAN` nos exemplos lidos)
- `K`: quantidade de codigos de barra encontrados na base
- `M`: identificacao da origem/vigencia interna (`Tabloide 02.06.2026`)
- `N`: indicador se o item tambem esta nas ofertas do tabloide
- `O`: campo concatenado no formato `codigo;venda;clube;quantidade`
- `P`: status
- `Q`: quantidade tratada
- `R`: unidade
- `T/U`: numeracao e orientacoes operacionais para cadastro

## Formulas observadas na aba do tabloide

Na planilha atual, o campo usado pelo CRM para cadastro de ofertas e o `Concatenado` da coluna `O`. A formula-base observada na aba `Tabloide - 02.06.2026 F` e:

- `O3 = CONCATENATE(H3,$L$1,E3,$L$1,F3,$L$1,Q3)`

Com isso, o `Concatenado` final segue o padrao:

- `codigo_barras;preco_venda;preco_clube;quantidade`

As dependencias diretas desse campo sao:

- `H`: codigo de barras
- `E`: preco de venda
- `F`: preco clube
- `Q`: quantidade tratada para cadastro
- `L1`: separador `;`

Outras formulas relevantes para o fluxo interno:

- `J = IF(ISTEXT(Hx), "", IF(LEN(TEXT(Hx,"0"))<6, "Codigo Unificado", "EAN"))`
  Isso deriva o tipo de codigo a partir do valor em `H`.
- `K = COUNTIF('Base - Cod Barras'!A:A,Cx)`
  Isso mede quantos codigos de barra a descricao do tabloide possui na base auxiliar.

Leitura operacional atual:

- para envio as lojas, o bloco de `A` ate `H` continua sendo o suficiente;
- para uso interno e cadastro no CRM, o operador depende ao menos do bloco que permite montar e revisar `O`, com destaque para `H`, `E`, `F`, `Q`, alem dos campos auxiliares `J`, `K`, `N` e `P`.
