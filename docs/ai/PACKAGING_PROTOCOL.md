# Agent Rules Packaging Protocol

Use this protocol only when maintaining or exporting the reusable agent-rule kit from this repository.

For normal Clube Das Ofertas application work, prefer `docs/ai/VERIFICATION_PROTOCOL.md` instead.

## Source Of Truth

The source of truth is the repository root and these folders:

- `AGENTS.md`
- `CLAUDE.md`
- `.github/copilot-instructions.md`
- `.cursor/rules/`
- `docs/ai/`
- `prompts/`
- `templates/`
- `tooling/`
- `scripts/`
- `README.md`
- `SOURCES.md`

This application repository does not require an `outputs/` package for normal development. Create or refresh `outputs/vibe-agent-kit/` only when the task explicitly asks to export the reusable rule kit.

## Required Packaging Steps

1. Update source files in the root source-of-truth areas.
2. Run the root installer in dry-run mode against a throwaway target under `work/`.
3. Install into a throwaway target under `work/` when installer behavior changed.
4. Refresh `outputs/vibe-agent-kit/` from the root source-of-truth files.
5. Regenerate `outputs/vibe-agent-kit-YYYY-MM-DD.zip`.
6. Confirm the package includes hidden adapter paths such as `.github/` and `.cursor/`.
7. Confirm new docs, prompts, templates, scripts, and tooling files are listed by the installer.

## Verification Commands

```powershell
.\scripts\install-agent-rules.ps1 -TargetPath ".\work\package-dry-run" -Profile all -DryRun
.\scripts\install-agent-rules.ps1 -TargetPath ".\work\package-install-test" -Profile all
rg --files --hidden outputs/vibe-agent-kit
```

For zip validation, list the archive entries and confirm the same major paths are present.

## Release Notes

When changing the packaged kit, summarize:

- Source files changed.
- Package files refreshed.
- Installer profiles affected.
- Verification run.
- Any files intentionally excluded from the package.
