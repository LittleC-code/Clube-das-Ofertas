# Project Profile

This repository is the Clube Das Ofertas internal web system.

The system imports campaign spreadsheets, matches items against a product catalog, applies risk rules, requires review for blocked items, and exports CSV files for CRM use.

## Current Structure

- `src/ClubeDasOfertas.Web/`
  ASP.NET Core web application, services, data access, domain models, and server-rendered HTML.
- `tests/ClubeDasOfertas.Tests/`
  Executable local tests for parsers, text normalization, and reading the real `.xlsm` reference file.
- `docs/FLUXOGRAMA.md`
  Main project map and troubleshooting guide.
- `CHECK LIST - Clube Das Ofertas.xlsm`
  Current spreadsheet reference used by import behavior and tests.
- `docker-compose.yml`
  Local PostgreSQL dependency for development.
- `docs/ai/`
  AI operating model, approval gates, security checklist, discovery notes, and verification guidance.
- `prompts/`
  Reusable AI prompts for development tasks.
- `templates/`
  Reusable markdown templates.
- `scripts/`
  Local utility scripts for agent rule installation/maintenance.

## Source Of Truth

For application behavior, the source of truth is `src/ClubeDasOfertas.Web/`, `tests/ClubeDasOfertas.Tests/`, `README.md`, and `docs/FLUXOGRAMA.md`.

For AI operating behavior, the source of truth is `AGENTS.md`, `CLAUDE.md`, `.github/copilot-instructions.md`, `.cursor/rules/*`, `docs/ai/`, `prompts/`, `templates/`, and `scripts/`.

## Expected Task Types

- Fix or improve import parsing for CSV, XLSX, and XLSM files.
- Adjust product catalog matching and text normalization.
- Change conversion rules for pesaveis, fardos, caixas, duplicate items, and review-required states.
- Improve review, audit, export, login, and admin flows.
- Update SQL schema or repository queries.
- Add tests before or alongside business-rule changes.
- Refine AI operating rules and prompts for this project.

## Current Verification Reality

Primary commands:

```powershell
dotnet build ClubeDasOfertas.slnx
dotnet run --project tests\ClubeDasOfertas.Tests\ClubeDasOfertas.Tests.csproj
docker compose up -d
dotnet run --project src\ClubeDasOfertas.Web\ClubeDasOfertas.Web.csproj
```

Use the tests for parsing, normalization, and spreadsheet-import behavior. Use local PostgreSQL plus the running app for end-to-end checks that touch persistence, auth, campaigns, review, catalog import, rules, history, or export.

## Sensitive Areas

Even without production code, these areas still deserve extra caution:

- Authentication and authorization in `Program.cs`, `SchemaInitializer.cs`, `PasswordHasher.cs`, and `AppRepository.cs`.
- Database schema and SQL in `Data/`.
- File upload and spreadsheet parsing in `SpreadsheetImporter.cs` and campaign/catalog import routes.
- Business-critical conversion and review logic in `CampaignImportService.cs`, `ReviewService.cs`, and `ExportService.cs`.
- CSV export formatting and encoding.
- Default users, local connection strings, and any future production credentials.
- Persistent AI instruction files and scripts.

## Packaging Rule

There is no release packaging pipeline yet. Treat this repository as a local-first internal application:

- application code lives in `src/ClubeDasOfertas.Web/`;
- verification lives in `tests/ClubeDasOfertas.Tests/`;
- local runtime dependency is PostgreSQL via `docker-compose.yml`;
- operational docs live in `README.md` and `docs/FLUXOGRAMA.md`;
- AI operating guidance lives in `AGENTS.md`, `CLAUDE.md`, `.github/copilot-instructions.md`, `.cursor/rules/*`, `docs/ai/`, `prompts/`, `templates/`, and `scripts/`.

Do not invent a packaging or deployment workflow without explicit approval. If packaging work is requested later, document the chosen build, config, secrets, database, and rollout path here.

## Local Notes For Agents

Prefer the troubleshooting paths in `docs/FLUXOGRAMA.md` when investigating bugs. Do not introduce a new web framework, ORM, frontend build system, or test framework unless explicitly requested and approved.

When changing import/export rules, update or add tests in `tests/ClubeDasOfertas.Tests/Program.cs` before relying on manual checks.
