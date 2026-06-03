# Fluxograma e mapa do projeto

Este documento explica como o sistema funciona de ponta a ponta, o que cada parte faz e onde olhar quando surgir erro.

## Visao geral

O sistema tem 4 blocos principais:

1. `Program.cs`: recebe as requisicoes HTTP, faz login, chama servicos e monta as telas.
2. `Services/`: aplica a regra de negocio.
3. `Data/`: fala com PostgreSQL, cria schema, le e grava dados.
4. `Ui/HtmlView.cs`: gera o HTML das paginas.

## Fluxo principal

```mermaid
flowchart TD
    A[Usuario abre o sistema] --> B[/login]
    B --> C{Credenciais validas?}
    C -- Nao --> B
    C -- Sim --> D[/campaigns]

    D --> E[Criar campanha]
    E --> F[Salvar campanha no banco]

    D --> G[Importar arquivo CSV/XLSX/XLSM]
    G --> H[SpreadsheetImporter le planilha]
    H --> I[CampaignImportService normaliza dados]
    I --> J[Busca correspondencias no catalogo]
    J --> K[Aplica regras de pesaveis e fardo/caixa]
    K --> L[Gera bloqueios e pendencias]
    L --> M[AppRepository grava campaign_items]

    M --> N[Usuario revisa itens pendentes]
    N --> O[ReviewService aprova ou rejeita]
    O --> P[Atualiza status do item]

    P --> Q{Ainda existe bloqueio?}
    Q -- Sim --> N
    Q -- Nao --> R[Exportar CSV]
    R --> S[ExportService monta arquivo]
    S --> T[Salvar exportacao e liberar download]
```

## Estrutura por responsabilidade

### Entrada web

- [Program.cs](</c:/Users/t.i/Projetos Git/Clube-das-Ofertas/src/ClubeDasOfertas.Web/Program.cs>)
  - Registra servicos.
  - Configura autenticacao por cookie.
  - Define rotas como `/login`, `/campaigns`, `/catalog`, `/rules`, `/history`.
  - Chama `CampaignImportService`, `ReviewService` e `ExportService`.

- [HtmlView.cs](</c:/Users/t.i/Projetos Git/Clube-das-Ofertas/src/ClubeDasOfertas.Web/Ui/HtmlView.cs>)
  - Monta o HTML das paginas.
  - Centraliza layout, badges e estilo visual.
  - Se um erro for “a tela ficou estranha” ou “nao aparece o badge certo”, olhe aqui.

### Regra de negocio

- [SpreadsheetImporter.cs](</c:/Users/t.i/Projetos Git/Clube-das-Ofertas/src/ClubeDasOfertas.Web/Services/SpreadsheetImporter.cs>)
  - Le `CSV`, `XLSX` e `XLSM`.
  - Procura as abas e os cabecalhos esperados.
  - Converte a planilha em linhas brutas para o sistema.
  - Se o erro for “arquivo nao importa”, “aba nao encontrada” ou “coluna obrigatoria ausente”, comece aqui.

- [Parsing.cs](</c:/Users/t.i/Projetos Git/Clube-das-Ofertas/src/ClubeDasOfertas.Web/Services/Parsing.cs>)
  - Converte preco, quantidade, unidade e tipo de codigo.
  - Se um valor monetario, `Kg`, `Fardos` ou `EAN` vier errado, olhe aqui.

- [TextNormalizer.cs](</c:/Users/t.i/Projetos Git/Clube-das-Ofertas/src/ClubeDasOfertas.Web/Services/TextNormalizer.cs>)
  - Remove acento, padroniza maiusculas e escapa CSV.
  - Se o cruzamento de descricao com o catalogo falhar por diferenca textual, este arquivo entra na investigacao.

- [CampaignImportService.cs](</c:/Users/t.i/Projetos Git/Clube-das-Ofertas/src/ClubeDasOfertas.Web/Services/CampaignImportService.cs>)
  - Orquestra a importacao da campanha.
  - Busca itens no catalogo.
  - Aplica regras de pesavel e fardo/caixa.
  - Gera bloqueios como `Produto sem catalogo/codigo`, `Conversao de pesavel pendente`, `Fardo/caixa pendente`.
  - Se o erro for “o item entrou bloqueado errado” ou “a regra de conversao nao bateu”, este e o primeiro arquivo.

- [ReviewService.cs](</c:/Users/t.i/Projetos Git/Clube-das-Ofertas/src/ClubeDasOfertas.Web/Services/ReviewService.cs>)
  - Aprova ou rejeita revisao manual.
  - Remove bloqueios pendentes quando o item e aprovado.
  - Se o item nao sai do estado pendente ou rejeitado, olhe aqui.

- [ExportService.cs](</c:/Users/t.i/Projetos Git/Clube-das-Ofertas/src/ClubeDasOfertas.Web/Services/ExportService.cs>)
  - Verifica se ainda existe bloqueio.
  - Gera o CSV final.
  - Salva o historico da exportacao.
  - Se o erro for “nao exporta” ou “o CSV saiu errado”, comece aqui.

### Persistencia e banco

- [AppDb.cs](</c:/Users/t.i/Projetos Git/Clube-das-Ofertas/src/ClubeDasOfertas.Web/Data/AppDb.cs>)
  - Abre conexao com PostgreSQL.
  - Se houver erro de string de conexao ou indisponibilidade do banco, este e o ponto base.

- [SchemaInitializer.cs](</c:/Users/t.i/Projetos Git/Clube-das-Ofertas/src/ClubeDasOfertas.Web/Data/SchemaInitializer.cs>)
  - Cria tabelas na inicializacao.
  - Cria usuarios padrao.
  - Cria regras padrao.
  - Se o sistema sobe mas “faltam tabelas” ou “nao existe login inicial”, olhe aqui.

- [AppRepository.cs](</c:/Users/t.i/Projetos Git/Clube-das-Ofertas/src/ClubeDasOfertas.Web/Data/AppRepository.cs>)
  - Faz todo o SQL de leitura e escrita.
  - Campaigns, catalogo, regras, itens, revisoes, exportacoes e auditoria passam por aqui.
  - Se o erro for “gravou errado no banco”, “consulta nao trouxe item” ou “nao salvou exportacao”, este e o arquivo principal.

- [PasswordHasher.cs](</c:/Users/t.i/Projetos Git/Clube-das-Ofertas/src/ClubeDasOfertas.Web/Data/PasswordHasher.cs>)
  - Gera e valida hash de senha.
  - Se o login falhar sem motivo aparente, confira este arquivo junto com `Program.cs`.

### Modelos

- [Models.cs](</c:/Users/t.i/Projetos Git/Clube-das-Ofertas/src/ClubeDasOfertas.Web/Domain/Models.cs>)
  - Define entidades como `Campaign`, `CampaignItem`, `ProductCatalogEntry`, `ConversionRule`, `ExportBatch`.
  - Se precisar adicionar campo novo no sistema, normalmente o primeiro passo e aqui.

## Fluxo por tela

```mermaid
flowchart LR
    A[Login] --> B[Campanhas]
    B --> C[Detalhe da campanha]
    C --> D[Importacao de itens]
    C --> E[Revisao de pendencias]
    C --> F[Exportacao CSV]
    B --> G[Catalogo]
    B --> H[Regras]
    B --> I[Historico]
```

## Fluxo de importacao

```mermaid
flowchart TD
    A[Arquivo enviado] --> B[SpreadsheetImporter]
    B --> C{Formato suportado?}
    C -- Nao --> X[Erro de importacao]
    C -- Sim --> D[Le linhas da aba]
    D --> E[Valida cabecalho]
    E --> F[CampaignImportService]
    F --> G[Normaliza descricao]
    G --> H[Busca no catalogo]
    H --> I{Encontrou codigo?}
    I -- Nao --> J[Marca SEM_CATALOGO]
    I -- Sim --> K[Cria 1 ou mais linhas por codigo]
    K --> L[Aplica regras]
    J --> L
    L --> M[Define bloqueios]
    M --> N[AppRepository salva campaign_items]
```

## Fluxo de revisao e exportacao

```mermaid
flowchart TD
    A[Usuario abre a campanha] --> B[Ve itens bloqueados ou pendentes]
    B --> C{Aprovar ou rejeitar?}
    C -- Aprovar --> D[ReviewService remove bloqueio pendente]
    C -- Rejeitar --> E[ReviewService mantem item bloqueado]
    D --> F[Tenta exportar]
    E --> F
    F --> G{Existe blocking_reasons?}
    G -- Sim --> H[ExportService bloqueia exportacao]
    G -- Nao --> I[Gera CSV]
    I --> J[Salva export_batch]
    J --> K[Download]
```

## Banco de dados

Tabelas principais:

- `users`: login e perfil.
- `product_catalog_entries`: base de descricao, Solidus e codigo.
- `conversion_rules`: regras editaveis.
- `campaigns`: cabecalho da campanha.
- `import_batches`: registro de cada importacao.
- `campaign_items`: itens finais processados.
- `review_decisions`: aprovacoes e rejeicoes.
- `export_batches`: arquivos exportados.
- `audit_logs`: historico operacional.

Relacao simplificada:

```mermaid
erDiagram
    USERS ||--o{ CAMPAIGNS : creates
    USERS ||--o{ IMPORT_BATCHES : imports
    USERS ||--o{ REVIEW_DECISIONS : reviews
    USERS ||--o{ EXPORT_BATCHES : exports
    CAMPAIGNS ||--o{ IMPORT_BATCHES : has
    CAMPAIGNS ||--o{ CAMPAIGN_ITEMS : has
    CAMPAIGNS ||--o{ REVIEW_DECISIONS : has
    CAMPAIGNS ||--o{ EXPORT_BATCHES : has
    IMPORT_BATCHES ||--o{ CAMPAIGN_ITEMS : creates
    CAMPAIGN_ITEMS ||--o{ REVIEW_DECISIONS : receives
```

## Onde mexer quando algo der erro

### 1. Erro de login

Olhar nesta ordem:

- [Program.cs](</c:/Users/t.i/Projetos Git/Clube-das-Ofertas/src/ClubeDasOfertas.Web/Program.cs>): rota `/login`.
- [AppRepository.cs](</c:/Users/t.i/Projetos Git/Clube-das-Ofertas/src/ClubeDasOfertas.Web/Data/AppRepository.cs>): `GetUserByEmailAsync`.
- [PasswordHasher.cs](</c:/Users/t.i/Projetos Git/Clube-das-Ofertas/src/ClubeDasOfertas.Web/Data/PasswordHasher.cs>)
- [SchemaInitializer.cs](</c:/Users/t.i/Projetos Git/Clube-das-Ofertas/src/ClubeDasOfertas.Web/Data/SchemaInitializer.cs>): seed dos usuarios.

### 2. Banco nao conecta ou sistema nao sobe

Olhar nesta ordem:

- [appsettings.json](</c:/Users/t.i/Projetos Git/Clube-das-Ofertas/src/ClubeDasOfertas.Web/appsettings.json>)
- [AppDb.cs](</c:/Users/t.i/Projetos Git/Clube-das-Ofertas/src/ClubeDasOfertas.Web/Data/AppDb.cs>)
- [SchemaInitializer.cs](</c:/Users/t.i/Projetos Git/Clube-das-Ofertas/src/ClubeDasOfertas.Web/Data/SchemaInitializer.cs>)
- `app-run.out.log` e `app-run.err.log` na raiz do projeto

### 3. Arquivo nao importa

Olhar nesta ordem:

- [SpreadsheetImporter.cs](</c:/Users/t.i/Projetos Git/Clube-das-Ofertas/src/ClubeDasOfertas.Web/Services/SpreadsheetImporter.cs>)
- [Parsing.cs](</c:/Users/t.i/Projetos Git/Clube-das-Ofertas/src/ClubeDasOfertas.Web/Services/Parsing.cs>)
- [Program.cs](</c:/Users/t.i/Projetos Git/Clube-das-Ofertas/src/ClubeDasOfertas.Web/Program.cs>): rota `/campaigns/{id}/import` ou `/catalog/import`

Sinais comuns:

- “Aba nao encontrada”: nome esperado no importador nao bate com a planilha.
- “Coluna obrigatoria ausente”: cabecalho veio diferente.
- “Preco invalido”: parser nao conseguiu entender o valor.

### 4. Item nao cruzou com o catalogo

Olhar nesta ordem:

- [TextNormalizer.cs](</c:/Users/t.i/Projetos Git/Clube-das-Ofertas/src/ClubeDasOfertas.Web/Services/TextNormalizer.cs>)
- [CampaignImportService.cs](</c:/Users/t.i/Projetos Git/Clube-das-Ofertas/src/ClubeDasOfertas.Web/Services/CampaignImportService.cs>)
- [AppRepository.cs](</c:/Users/t.i/Projetos Git/Clube-das-Ofertas/src/ClubeDasOfertas.Web/Data/AppRepository.cs>): `FindCatalogMatchesAsync`
- Tabela `product_catalog_entries`

### 5. Conversao de preco saiu errada

Olhar nesta ordem:

- [CampaignImportService.cs](</c:/Users/t.i/Projetos Git/Clube-das-Ofertas/src/ClubeDasOfertas.Web/Services/CampaignImportService.cs>)
- [Parsing.cs](</c:/Users/t.i/Projetos Git/Clube-das-Ofertas/src/ClubeDasOfertas.Web/Services/Parsing.cs>)
- [SchemaInitializer.cs](</c:/Users/t.i/Projetos Git/Clube-das-Ofertas/src/ClubeDasOfertas.Web/Data/SchemaInitializer.cs>): regras padrao
- Tela `/rules`

Casos tipicos:

- pesavel multiplicou quando nao devia;
- item `Kg` nao deveria virar `x10`;
- fardo/caixa deveria ir para revisao e nao foi.

### 6. Item fica pendente para sempre

Olhar nesta ordem:

- [ReviewService.cs](</c:/Users/t.i/Projetos Git/Clube-das-Ofertas/src/ClubeDasOfertas.Web/Services/ReviewService.cs>)
- [AppRepository.cs](</c:/Users/t.i/Projetos Git/Clube-das-Ofertas/src/ClubeDasOfertas.Web/Data/AppRepository.cs>): `UpdateItemReviewAsync`
- Tabela `campaign_items`, campo `blocking_reasons`

### 7. Exportacao nao libera

Olhar nesta ordem:

- [ExportService.cs](</c:/Users/t.i/Projetos Git/Clube-das-Ofertas/src/ClubeDasOfertas.Web/Services/ExportService.cs>)
- [AppRepository.cs](</c:/Users/t.i/Projetos Git/Clube-das-Ofertas/src/ClubeDasOfertas.Web/Data/AppRepository.cs>): leitura dos itens
- Tabela `campaign_items`: conferir `blocking_reasons`

### 8. CSV final saiu com formato errado

Olhar nesta ordem:

- [ExportService.cs](</c:/Users/t.i/Projetos Git/Clube-das-Ofertas/src/ClubeDasOfertas.Web/Services/ExportService.cs>)
- [TextNormalizer.cs](</c:/Users/t.i/Projetos Git/Clube-das-Ofertas/src/ClubeDasOfertas.Web/Services/TextNormalizer.cs>): escape do CSV
- [Parsing.cs](</c:/Users/t.i/Projetos Git/Clube-das-Ofertas/src/ClubeDasOfertas.Web/Services/Parsing.cs>): formato monetario e datas

## Ordem segura para mexer

Quando precisar alterar o sistema, a ordem mais segura costuma ser:

1. Ajustar ou adicionar campos em [Models.cs](</c:/Users/t.i/Projetos Git/Clube-das-Ofertas/src/ClubeDasOfertas.Web/Domain/Models.cs>)
2. Ajustar schema ou SQL em [SchemaInitializer.cs](</c:/Users/t.i/Projetos Git/Clube-das-Ofertas/src/ClubeDasOfertas.Web/Data/SchemaInitializer.cs>) e [AppRepository.cs](</c:/Users/t.i/Projetos Git/Clube-das-Ofertas/src/ClubeDasOfertas.Web/Data/AppRepository.cs>)
3. Ajustar regra de negocio em `Services/`
4. Ajustar rota ou fluxo em [Program.cs](</c:/Users/t.i/Projetos Git/Clube-das-Ofertas/src/ClubeDasOfertas.Web/Program.cs>)
5. Ajustar tela em [HtmlView.cs](</c:/Users/t.i/Projetos Git/Clube-das-Ofertas/src/ClubeDasOfertas.Web/Ui/HtmlView.cs>)
6. Validar com [tests/Program.cs](</c:/Users/t.i/Projetos Git/Clube-das-Ofertas/tests/ClubeDasOfertas.Tests/Program.cs>)

## Testes atuais

- [tests/Program.cs](</c:/Users/t.i/Projetos Git/Clube-das-Ofertas/tests/ClubeDasOfertas.Tests/Program.cs>)
  - valida parser monetario;
  - valida parser de quantidade;
  - valida normalizacao de texto;
  - valida leitura real da planilha `.xlsm`.

Se surgir bug novo de regra de negocio, o melhor caminho e acrescentar teste aqui antes de corrigir a implementacao.
