# Prompt: Create Or Improve Agent Rules

Use this prompt when the agent should create supporting markdown or propose durable rules.

```text
You are maintaining agent operating rules for this project.

Goal:
<describe the repeated workflow, failure mode, or convention to encode>

Rules:
- Keep durable instruction files short and specific.
- Prefer links to deeper docs over large instruction blocks.
- Do not weaken security, review, testing, sandboxing, traceability, or approval gates.
- Treat persistent rule files as security-critical configuration.
- You may create draft rule docs automatically.
- Ask for approval before modifying existing persistent steering files.
- If the change affects packaged outputs, follow docs/ai/PACKAGING_PROTOCOL.md after approval.

Process:
1. Inspect existing instruction files and docs.
2. Identify whether this should be a repo-wide rule, path-specific rule, prompt file, checklist, or ordinary documentation.
3. Draft the smallest useful instruction.
4. Include scope, risk, and verification.
5. If the change modifies persistent steering, request approval before applying it.
6. If the approved change should ship with the reusable kit, refresh package outputs and verify them.
```
