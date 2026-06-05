# Verification Protocol

Verification should scale with risk.

## Low-Risk Docs Or Styling

- Read affected files.
- Check formatting when available.
- Run focused tests only if behavior changed.

## Normal Feature Or Bug Fix

- Run focused unit/integration tests for changed behavior.
- Run lint/typecheck if available.
- Start or exercise the changed workflow when feasible.
- Add tests for regression-prone behavior.

## Shared, Cross-Module, Or Public Behavior

- Run focused tests plus broader suite.
- Run typecheck/lint/build.
- Review downstream call sites.
- Update docs and examples.

## Security-Sensitive Change

- Run normal verification.
- Run dependency audit and secret scan where available.
- Add negative/adversarial tests.
- Review logs, permissions, input validation, authz, and error handling.
- Require human approval before deploy or merge if risk remains.

## Verification Report Template

```text
Checks run:
- <command>: <pass/fail/not run>

Manual checks:
- <workflow or file reviewed>

Not run:
- <check> because <reason>

Residual risk:
- <risk or none>
```

## Current Repository Defaults

For Clube Das Ofertas, default to these checks:

- Run `dotnet build ClubeDasOfertas.slnx` for code changes.
- Run `dotnet run --project tests\ClubeDasOfertas.Tests\ClubeDasOfertas.Tests.csproj` for parser, import, normalization, and business-rule changes.
- Use `docker compose up -d` when database-backed behavior needs local verification.
- Run `dotnet run --project src\ClubeDasOfertas.Web\ClubeDasOfertas.Web.csproj` when route, auth, HTML, import, review, or export behavior should be exercised manually.
- Review `docs/FLUXOGRAMA.md` when changing flow or troubleshooting guidance.
- For AI rule changes, read the changed markdown directly and check file placement with `rg --files --hidden`.

If a check cannot run because .NET 10, Docker, or PostgreSQL is unavailable, report that clearly with the command that should be run.
