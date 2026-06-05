# Sources

These files synthesize guidance from current public sources checked on 2026-06-03.

Refresh source checks according to `docs/ai/SOURCE_REFRESH_PROTOCOL.md`, especially before changing tool-specific instruction formats, security guardrails, or approval gates.

## Core Framing

- Simon Willison, "Not all AI-assisted programming is vibe coding": distinguishes casual vibe coding from responsible AI-assisted software development and emphasizes reviewing, testing, and understanding code before committing. https://simonwillison.net/2025/Mar/19/vibe-coding/
- Google Cloud, "What is vibe coding?": describes pure vibe coding versus responsible AI-assisted development and the describe/generate/execute/refine loop. https://cloud.google.com/discover/what-is-vibe-coding?hl=en

## Agent Instructions And Workflow

- OpenAI Codex, "Custom instructions with AGENTS.md": explains AGENTS.md discovery, precedence, fallback filenames, size limits, and setup verification. https://developers.openai.com/codex/guides/agents-md
- GitHub Docs, "Best practices for using GitHub Copilot to work on tasks": recommends clear scoped tasks, acceptance criteria, file guidance, planning before PRs, and repository custom instructions. https://docs.github.com/en/copilot/tutorials/cloud-agent/get-the-best-results
- GitHub Docs, "About customizing Copilot responses": documents repository custom instructions and prompt files. https://docs.github.com/en/copilot/concepts/prompting/response-customization
- Anthropic, "Best practices for Claude Code": recommends self-contained specs, end-to-end verification, tight feedback loops, context management, and review subagents. https://code.claude.com/docs/en/best-practices

## Security

- OWASP Cheat Sheet Series, "Secure Coding with AI": covers agent trust boundaries, hallucinated dependencies, indirect prompt injection, sandboxing, persistent rules files, out-of-scope edits, test fabrication, context leakage, CI/CD risks, and human accountability. https://cheatsheetseries.owasp.org/cheatsheets/Secure_Coding_with_AI_Cheat_Sheet.html
