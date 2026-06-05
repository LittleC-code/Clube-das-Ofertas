# Project Discovery Protocol

Use this when entering an existing repository or starting a new session.

## First Pass

1. Identify the repo root and active working directory.
2. Read durable instructions:
   - `AGENTS.md`
   - `CLAUDE.md`
   - `.github/copilot-instructions.md`
   - `.cursor/rules/*`
   - README and contributing docs
   - `docs/ai/PROJECT_PROFILE.md`
3. Inspect manifests and tooling:
   - package manager files
   - language/toolchain version files
   - CI workflows
   - test config
   - lint/typecheck/format config
4. Locate app entry points, routing, data access, auth, and shared utilities.
5. Identify generated files and folders that should not be manually edited.

## Current Repository Notes

For this repository today, use the following default assumptions unless new source code is added later:

- The product runtime is `src/ClubeDasOfertas.Web/Program.cs`.
- The main solution file is `ClubeDasOfertas.slnx`.
- The test entry point is `tests/ClubeDasOfertas.Tests/Program.cs`.
- The project uses .NET 10, ASP.NET Core, PostgreSQL, Npgsql, cookie authentication, and server-rendered HTML.
- Business flow documentation lives in `docs/FLUXOGRAMA.md`.
- The local spreadsheet reference is `CHECK LIST - Clube Das Ofertas.xlsm`.
- Verification usually starts with `dotnet build ClubeDasOfertas.slnx` and `dotnet run --project tests\ClubeDasOfertas.Tests\ClubeDasOfertas.Tests.csproj`.

## Discovery Output

Keep this concise in your working notes:

```text
Stack:
Entrypoints:
Important commands:
Test strategy:
Security-sensitive areas:
Generated/avoid-edit areas:
Relevant docs:
Assumptions:
```

## Existing Project Rule

Do not introduce a new architecture, dependency, state management pattern, styling system, or test framework when a local pattern already exists and is adequate.

For this repository specifically, prefer local patterns in `Program.cs`, `Services/`, `Data/`, `Domain/`, and `Ui/HtmlView.cs`. Do not add a new ORM, frontend framework, or test framework unless the user explicitly asks for that direction.
