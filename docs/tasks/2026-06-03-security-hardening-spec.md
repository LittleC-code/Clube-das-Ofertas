# Security Hardening Spec

## Summary

This change hardens the Clube Das Ofertas web app in four areas:

- remove hardcoded bootstrap credentials from source and UI;
- require antiforgery validation for mutating routes;
- strengthen file upload validation for campaign and catalog imports;
- add targeted tests for critical business-rule behavior and import safety.

## Affected Files

- `src/ClubeDasOfertas.Web/Program.cs`
- `src/ClubeDasOfertas.Web/Data/AppDb.cs`
- `src/ClubeDasOfertas.Web/Data/SchemaInitializer.cs`
- `src/ClubeDasOfertas.Web/Services/SpreadsheetImporter.cs`
- `src/ClubeDasOfertas.Web/Services/CampaignImportService.cs`
- `src/ClubeDasOfertas.Web/Ui/HtmlView.cs`
- `src/ClubeDasOfertas.Web/appsettings*.json`
- `tests/ClubeDasOfertas.Tests/Program.cs`
- `README.md`
- `docs/FLUXOGRAMA.md`

## Implementation Notes

- Replace seeded default passwords with first-run bootstrap flow for the first admin user.
- Keep database connection string configurable from source, but require password override from environment when not embedded in the connection string.
- Convert export creation from `GET` to `POST`.
- Add a reusable antiforgery hidden input renderer for HTML forms and validate tokens in every mutating route.
- Reject empty, oversized, unsupported, or signature-mismatched uploads.
- Expose the pure item-evaluation logic for direct tests without requiring a live database.

## Out Of Scope

- Full role redesign.
- Production deployment automation.
- Database schema migration framework.
- Large UI redesign.
- Query-performance refactors beyond the touched security/verification flow.

## Verification

- `dotnet build ClubeDasOfertas.slnx`
- `dotnet run --project tests\ClubeDasOfertas.Tests\ClubeDasOfertas.Tests.csproj`
- Manual route review for login, campaign import, catalog import, review, and export form handling
