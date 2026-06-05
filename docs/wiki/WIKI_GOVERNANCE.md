# Governança da Wiki de Memória do Agente

Este documento define as regras que o agente segue para criar, manter e consultar a wiki de memória do projeto. A wiki é um artefato persistente e composto — ela acumula conhecimento entre sessões e nunca é reconstruída do zero.

---

## Princípios fundamentais

1. **A wiki é a memória do agente.** Tudo que foi descoberto, decidido ou aprendido sobre o projeto vive aqui. O agente nunca parte do zero em uma nova sessão — ele lê a wiki primeiro.
2. **O agente escreve, o humano lê e direciona.** O humano não edita a wiki manualmente (exceto para corrigir erros graves). O agente faz todo o trabalho de síntese, cruzamento e manutenção.
3. **Fontes brutas são imutáveis.** Arquivos em `docs/wiki/sources/` nunca são modificados pelo agente. São a fonte da verdade; a wiki é a interpretação.
4. **Conhecimento composto, não duplicado.** Quando algo novo contradiz algo antigo, o agente atualiza a página existente e registra a contradição — não cria uma nova página paralela.
5. **Tudo que importa é linkado.** Uma informação sem link de entrada é um órfão — não será encontrada. O agente sempre cria referências cruzadas ao escrever.

---

## Estrutura de diretórios

```
docs/wiki/
├── index.md                  ← catálogo de todas as páginas (atualizado a cada operação)
├── log.md                    ← registro cronológico append-only de todas as operações
├── overview.md               ← síntese geral do projeto, tese atual, estado do conhecimento
│
├── sources/                  ← fontes brutas (imutáveis)
│   ├── decisoes/             ← registros de decisões técnicas (ADRs, discussões)
│   ├── dominio/              ← documentos de regras de negócio, specs, planilhas de referência
│   └── incidentes/           ← relatórios de bugs, post-mortems, changelogs
│
├── dominio/                  ← páginas sobre o negócio e suas regras
│   ├── campanha.md
│   ├── item.md
│   ├── matching.md
│   ├── risco.md
│   ├── exportacao.md
│   └── ...
│
├── tecnico/                  ← páginas sobre arquitetura, padrões e decisões técnicas
│   ├── arquitetura.md
│   ├── apprepository.md
│   ├── autenticacao.md
│   ├── importacao-xlsm.md
│   └── ...
│
├── historico/                ← páginas sobre mudanças, bugs e incidentes
│   ├── bugs-resolvidos.md
│   ├── decisoes-revertidas.md
│   └── ...
│
└── sintese/                  ← análises, comparações e respostas a perguntas importantes
    └── ...
```

---

## Operações

### 1. Inicialização de sessão

No início de **toda** sessão, antes de qualquer outra leitura:

1. Leia `docs/wiki/index.md` para ter o mapa do conhecimento atual.
2. Leia as últimas 10 entradas de `docs/wiki/log.md` para entender o que foi feito recentemente.
3. Se a tarefa tocar em uma área específica (domínio, técnico, histórico), leia as páginas relevantes dessa área antes de ler qualquer arquivo de código-fonte.
4. Registre no log que a sessão foi iniciada.

Formato da entrada de log de início de sessão:
```
## [YYYY-MM-DD] sessão-iniciada | <título resumido da tarefa>
Tarefa: <descrição em uma linha>
Páginas lidas: index.md, log.md, <outras páginas lidas>
```

---

### 2. Ingestão de nova fonte

Quando um novo documento, decisão, bug ou regra de negócio for identificado:

1. Copie ou referencie o arquivo original em `docs/wiki/sources/<categoria>/`.
2. Discuta os pontos principais com o humano se necessário.
3. Crie ou atualize a página de resumo correspondente em `docs/wiki/`.
4. Identifique todas as páginas existentes que devem ser atualizadas (mínimo: páginas de entidades mencionadas, páginas de conceitos relacionados, `overview.md` se o impacto for significativo).
5. Atualize `docs/wiki/index.md` com a nova página (se criada).
6. Appende uma entrada ao `docs/wiki/log.md`.

Formato da entrada de log de ingestão:
```
## [YYYY-MM-DD] ingestão | <título da fonte>
Fonte: docs/wiki/sources/<caminho>
Páginas criadas: <lista>
Páginas atualizadas: <lista>
Contradições encontradas: <lista ou "nenhuma">
```

---

### 3. Consulta

Quando o humano fizer uma pergunta sobre o projeto:

1. Leia `docs/wiki/index.md` para identificar páginas relevantes.
2. Leia as páginas identificadas.
3. Sintetize a resposta com citações às páginas consultadas.
4. Se a resposta revelar uma conexão nova ou uma síntese valiosa, **arquive-a como nova página** em `docs/wiki/sintese/`.
5. Appende uma entrada ao log.

Formato da entrada de log de consulta:
```
## [YYYY-MM-DD] consulta | <pergunta resumida>
Páginas consultadas: <lista>
Página de síntese criada: <caminho ou "nenhuma">
```

---

### 4. Atualização pós-tarefa

Ao concluir qualquer tarefa de desenvolvimento (bugfix, feature, refatoração):

1. Identifique o que foi aprendido ou decidido durante a tarefa.
2. Atualize as páginas técnicas afetadas (`docs/wiki/tecnico/`).
3. Se um bug foi resolvido, atualize `docs/wiki/historico/bugs-resolvidos.md`.
4. Se uma decisão de arquitetura foi tomada, atualize `docs/wiki/tecnico/arquitetura.md` e crie uma entrada em `docs/wiki/historico/decisoes-revertidas.md` se algo anterior foi abandonado.
5. Se uma regra de negócio foi clarificada, atualize a página de domínio correspondente.
6. Appende uma entrada ao log.

Formato da entrada de log de atualização pós-tarefa:
```
## [YYYY-MM-DD] pós-tarefa | <título da tarefa>
Páginas técnicas atualizadas: <lista>
Páginas de domínio atualizadas: <lista>
Páginas de histórico atualizadas: <lista>
Aprendizados registrados: <resumo em 1-2 linhas>
```

---

### 5. Manutenção periódica (lint)

Execute a cada 10 ingestões ou quando solicitado pelo humano:

1. Identifique páginas órfãs (sem links de entrada).
2. Identifique contradições entre páginas.
3. Identifique claims desatualizados por fontes mais recentes.
4. Identifique conceitos mencionados em várias páginas mas sem página própria.
5. Sugira ao humano novas fontes ou perguntas que preencheriam lacunas detectadas.
6. Registre o resultado no log.

Formato da entrada de log de manutenção:
```
## [YYYY-MM-DD] lint | manutenção periódica
Órfãos encontrados: <lista ou "nenhum">
Contradições encontradas: <lista ou "nenhuma">
Claims desatualizados: <lista ou "nenhum">
Conceitos sem página: <lista ou "nenhum">
Fontes sugeridas: <lista ou "nenhuma">
```

---

## Formato das páginas

Toda página da wiki segue este cabeçalho:

```markdown
---
titulo: <nome da página>
categoria: dominio | tecnico | historico | sintese
criado: YYYY-MM-DD
atualizado: YYYY-MM-DD
fontes: [<lista de arquivos em sources/ que embasam esta página>]
links: [<lista de outras páginas da wiki que esta página referencia>]
---

# <Título>

<conteúdo>
```

Regras de conteúdo:

- Máximo de 500 palavras por página. Se precisar de mais, divida em subpáginas linkadas.
- Todo claim factual deve ter uma referência: `[fonte](../sources/...)` ou `[ver página](../tecnico/...)`.
- Contradições com outras páginas são sinalizadas em bloco destacado:
  ```
  > ⚠️ **Contradição:** esta página afirma X, mas [[outra-pagina]] afirma Y. Última atualização: YYYY-MM-DD.
  ```
- Decisões revertidas ou deprecadas são marcadas com:
  ```
  > ~~Deprecado em YYYY-MM-DD.~~ Ver [[nova-pagina]] para a abordagem atual.
  ```

---

## Regras de linkagem

- Toda menção a uma entidade de domínio (campanha, item, oferta, matching, risco, exportação) é um link para a página correspondente em `docs/wiki/dominio/`.
- Toda menção a um componente técnico (AppRepository, serviço de importação, autenticação) é um link para a página correspondente em `docs/wiki/tecnico/`.
- `index.md` e `log.md` nunca são linkados dentro do conteúdo das páginas — são arquivos de navegação, não de conteúdo.

---

## O que nunca fazer

- Deletar páginas. Marque como deprecada com link para a substituta.
- Reescrever o log. É append-only — erros no log são corrigidos com uma nova entrada de correção.
- Criar duas páginas sobre o mesmo conceito. Mescle e linke.
- Deixar uma sessão sem entrada no log.
- Escrever na wiki sem atualizar o `index.md`.

---

## Integração com PROJECT_MAP.md

A wiki e o `PROJECT_MAP.md` são complementares:

- `PROJECT_MAP.md` responde: **quais arquivos de código ler para esta tarefa**.
- A wiki responde: **o que já se sabe sobre este domínio, decisão ou componente**.

O agente lê os dois antes de começar qualquer tarefa. A ordem é: `index.md` da wiki → páginas relevantes da wiki → `PROJECT_MAP.md` → arquivos de código listados no mapa.
