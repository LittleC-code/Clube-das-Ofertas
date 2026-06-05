# Claude Code Project Instructions

Follow the same operating model as `AGENTS.md`. If both files exist and conflict, prefer the more specific instruction closest to the current working directory, then the user's latest direct instruction.

Read `docs/ai/PROJECT_PROFILE.md` before project-level work. This repository is an internal .NET 10 / ASP.NET Core app for importing Clube Das Ofertas campaign spreadsheets, reviewing blocked items, and exporting CSV files for CRM use.

## Default Behavior

Operate autonomously through the full engineering loop: discover, plan, implement, verify, self-review, document, and summarize. Ask the user only for approval gates listed in `docs/ai/APPROVAL_GATES.md`.

For larger work, create or update a self-contained spec before implementation. A useful spec names affected files and interfaces, states out-of-scope items, and ends with an end-to-end verification step.

Use focused subagents or separate review passes when useful:

- Security review for auth, data, file upload, payment, secrets, or CI/CD changes.
- Test review for deleted tests, weakened assertions, excessive mocks, and missing negative cases.
- Architecture review for broad refactors or cross-module changes.

If the conversation becomes noisy after repeated corrections, summarize the current state into a task-local markdown file and continue from the cleanest available context.

## Approval Gates

Read `docs/ai/APPROVAL_GATES.md` before taking risky action. Do not modify persistent instruction files, CI/CD, production config, secrets, dependencies, or destructive operations without explicit approval.

Use `dotnet build ClubeDasOfertas.slnx` and `dotnet run --project tests\ClubeDasOfertas.Tests\ClubeDasOfertas.Tests.csproj` as the default verification path for code changes.
