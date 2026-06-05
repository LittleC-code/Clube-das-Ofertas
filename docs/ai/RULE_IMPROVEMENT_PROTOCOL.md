# Rule Improvement Protocol

Agents may create supporting markdown files to improve repeatability, but persistent steering files are security-critical.

## Safe To Create Without Approval

- Task-local specs, plans, verification notes, decision logs, and checklists.
- New docs that describe current behavior without changing future agent authority.
- Proposed rule files in a draft location, for example `docs/ai/proposed-rules/*.md`.

## Approval Required

- Editing existing persistent instruction files:
  - `AGENTS.md`
  - `CLAUDE.md`
  - `.github/copilot-instructions.md`
  - `.github/instructions/*`
  - `.cursor/rules/*`
  - `.aider*`
  - `.windsurfrules`
  - MCP definitions
  - hooks
  - custom system prompts
- Adding rules that reduce review, testing, security, sandboxing, traceability, or approval gates.
- Giving an agent broader filesystem, network, credential, CI, deploy, or production access.

## Applying Approved Rule Changes

After human approval, update the source-of-truth rule files first. If the packaged kit should include the change, follow `docs/ai/PACKAGING_PROTOCOL.md` before treating the work as complete.

## Draft Rule Template

```markdown
# Proposed Rule: <name>

## Problem
<What repeated failure or workflow gap this addresses.>

## Proposed Instruction
<Exact text to add.>

## Scope
<Repo-wide, path-specific, tool-specific, or task-specific.>

## Risk
<How this could be misused or over-applied.>

## Verification
<How to tell whether the rule works.>
```
