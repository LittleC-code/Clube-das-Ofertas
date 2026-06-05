# Autonomous Agent Operating Model

## Goal

Build, improve, and maintain software with AI acceleration while preserving engineering accountability. The agent should act independently for ordinary engineering work and escalate only when the action crosses a clear approval gate.

## Modes

### New Project

Use when no mature codebase exists.

1. Convert the user's idea into a concise product brief.
2. Identify target users, core workflow, non-goals, data model, integrations, and risk areas.
3. Choose a conservative stack unless the user specifies one.
4. Create a minimal but real app, not a placeholder.
5. Add setup, tests, linting, and a verification command from day one.
6. Document how to run, test, and extend the project.

### Existing Project

Use when code already exists.

1. Read the project before proposing changes.
2. Identify conventions and reuse local helpers.
3. Preserve public behavior unless the task explicitly changes it.
4. Keep the diff small and explain any file touched outside the expected area.
5. Expand tests where behavior risk justifies it.

### Repair Or Refactor

Use when improving quality without changing behavior.

1. Capture current behavior with tests or existing checks first.
2. Refactor in small steps.
3. Avoid unrelated modernization.
4. Verify behavior remains unchanged.

### Security-Sensitive Work

Use when touching auth, authorization, data access, file upload, payments, secrets, cryptography, CI/CD, infra, or external tool execution.

1. Create or update threat notes in `docs/ai/SECURITY_CHECKLIST.md` or a task-local file.
2. Add negative/adversarial tests.
3. Run dependency, secret, static, or dynamic checks where available.
4. Request human approval before merge/deploy or persistent rule/config changes.

## Default Autonomy

Allowed without asking:

- Read code, docs, tests, local config, and CI.
- Edit files in scope.
- Add task-local docs, plans, specs, and verification notes.
- Run local tests, linters, formatters, build checks, and safe app startup.
- Create focused tests for changed behavior.
- Fix failures caused by the agent's own changes.

Not allowed without approval:

- Any approval gate in `APPROVAL_GATES.md`.

