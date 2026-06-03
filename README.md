# Clube Das Ofertas

Sistema web interno para importar itens de campanha, cruzar com catalogo de codigos, aplicar regras de risco, exigir revisao e exportar CSV pronto para o CRM.

Documentacao de fluxo e manutencao:

- [docs/FLUXOGRAMA.md](</c:/Users/t.i/Projetos Git/Clube-das-Ofertas/docs/FLUXOGRAMA.md>)

## Stack

- .NET 10 / ASP.NET Core
- PostgreSQL
- Npgsql
- HTML server-side com autenticacao por cookie

## Rodar localmente

1. Instale o SDK .NET 10.
2. Suba um PostgreSQL local. Se tiver Docker:

```powershell
docker compose up -d
```

3. Restaure e compile:

```powershell
dotnet build ClubeDasOfertas.slnx
```

4. Rode a aplicacao:

```powershell
dotnet run --project src\ClubeDasOfertas.Web\ClubeDasOfertas.Web.csproj
```

5. Acesse `http://localhost:5088`.

Usuarios iniciais:

- `admin@clube.local` / `Admin@123`
- `operador@clube.local` / `Operador@123`

## Fluxo operacional

1. Entrar como admin e importar o catalogo usando a planilha atual `CHECK LIST - Clube Das Ofertas.xlsm` ou outro arquivo com as colunas `Descricao Tabloide`, `Categoria`, `Descricao Solidus`, `Cod Barras`.
2. Criar uma campanha informando nome e vigencia.
3. Importar o arquivo de itens com layout fixo: `Fonte`, `Vigencia`, `Descricao no Tabloide`, `Quantidade limitada`, `Venda`, `Venda Clube`.
4. Revisar itens bloqueados:
   - sem catalogo/codigo;
   - pesaveis convertidos para kg;
   - fardos/caixas detectados;
   - preco ou quantidade invalida.
5. Exportar CSV quando nao houver pendencias criticas.

## Testes

Os testes locais validam parsers e leitura da planilha `.xlsm` real sem depender do banco.

```powershell
dotnet run --project tests\ClubeDasOfertas.Tests\ClubeDasOfertas.Tests.csproj
```

## Configuracao

A connection string padrao esta em `src/ClubeDasOfertas.Web/appsettings.json`:

```json
"Host=localhost;Port=5432;Database=clube_das_ofertas;Username=clube;Password=clube123"
```

Altere esse valor para apontar para o servidor PostgreSQL da rede quando for implantar em producao interna.
