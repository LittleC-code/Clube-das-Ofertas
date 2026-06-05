# Adição ao AGENTS.md — Integração da Wiki de Memória

Adicione a seção abaixo ao `AGENTS.md` do projeto, após a seção **Protocolo de Inicialização**.

---

## Wiki de Memória

O projeto mantém uma wiki persistente em `docs/wiki/`. Ela é a memória do agente entre sessões — acumula decisões técnicas, regras de negócio, histórico de mudanças e sínteses. O agente nunca redescobre o que já está na wiki.

As regras completas de operação estão em `docs/wiki/WIKI_GOVERNANCE.md`. Esta seção é o resumo executivo.

### Leitura obrigatória no início de cada sessão

Antes de ler qualquer arquivo de código ou `PROJECT_MAP.md`:

1. Leia `docs/wiki/index.md` — mapa de todo o conhecimento acumulado.
2. Leia as últimas 10 entradas de `docs/wiki/log.md` — o que foi feito recentemente.
3. Leia as páginas da wiki relevantes para a tarefa atual.
4. Somente então consulte `docs/wiki/PROJECT_MAP.md` e leia os arquivos de código listados.

### Escrita obrigatória ao fim de cada sessão

Ao concluir qualquer tarefa, antes de reportar o output final:

1. Atualize as páginas de domínio, técnico ou histórico afetadas pela tarefa.
2. Atualize `docs/wiki/index.md` se novas páginas foram criadas.
3. Appende uma entrada ao `docs/wiki/log.md` com o formato definido em `WIKI_GOVERNANCE.md`.

### Três tipos de conteúdo que sempre vão para a wiki

- **Decisão técnica tomada** → `docs/wiki/tecnico/arquitetura.md` + entrada no log.
- **Regra de negócio clarificada ou descoberta** → página correspondente em `docs/wiki/dominio/`.
- **Bug resolvido** → `docs/wiki/historico/bugs-resolvidos.md` com causa raiz e solução.

### O que nunca fazer

- Não deletar páginas da wiki — marque como deprecada com link para a substituta.
- Não reescrever o log — é append-only.
- Não terminar uma sessão sem entrada no log.
- Não criar duas páginas sobre o mesmo conceito — mescle e linke.
