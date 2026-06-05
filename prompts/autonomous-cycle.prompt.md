# Prompt: Autonomous Development Cycle

Use this prompt to keep an agent moving through a multi-step task.

```text
Continue autonomously until the task is genuinely handled.

Cycle:
1. Inspect before editing.
2. Make one coherent change at a time.
3. Run the nearest useful check.
4. Fix failures caused by the change.
5. Broaden verification when the blast radius grows.
6. Review every file in the diff.
7. Document behavior and operating knowledge that future agents need.
8. Keep docs/ai/PROJECT_PROFILE.md accurate for the current repository.

Do not pause for routine decisions. Make conservative assumptions that match the codebase.

Pause only for approval gates:
- destructive action
- production/cost impact
- secrets or sensitive data
- auth, payments, PII, compliance, incident response
- new production dependency or external service
- CI/CD/build/deploy execution path
- persistent agent rules
- broad rewrite or ambiguous high-impact decision

When blocked by an approval gate, ask one concise approval question with action, reason, impact, rollback, and "Proceed?".
```
