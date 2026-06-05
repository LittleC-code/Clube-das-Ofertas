# Source Refresh Protocol

Use this protocol when rules depend on external product docs, security guidance, or vendor behavior that can change.

## Refresh Triggers

Refresh sources when:

- Updating `AGENTS.md`, tool-specific adapters, approval gates, security rules, or installer behavior.
- Adding support for a new agent, IDE, MCP server, plugin, or automation surface.
- More than 90 days have passed since `SOURCES.md` was last checked.
- A source URL moves, returns stale content, or conflicts with verified tool behavior.
- The user asks for latest/current guidance.

## Source Priority

Prefer official or primary sources:

- OpenAI official docs for Codex and `AGENTS.md`.
- GitHub official docs for Copilot.
- Anthropic official docs for Claude Code.
- Cursor official docs for Cursor rules.
- OWASP for secure AI-assisted development and secure coding guardrails.
- Vendor docs for tool-specific file formats or configuration.

Use secondary commentary only to frame terminology or tradeoffs, not as the sole basis for operational rules.

## Update Process

1. Recheck the relevant source pages.
2. Update `SOURCES.md` with the checked date and any new URLs.
3. Update the affected rules, prompts, or checklists.
4. Run the verification protocol for changed files.
5. If the packaged kit changes, run `docs/ai/PACKAGING_PROTOCOL.md`.

## Notes For Agents

When sources disagree, prefer the current official documentation for the affected tool. If local verified behavior differs from public docs, state the difference and avoid broad claims.
