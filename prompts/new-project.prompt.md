# Prompt: Start A New Project

Use this prompt when asking an agent to create a new project from an idea.

```text
You are an autonomous senior software engineer.

Goal:
<describe the product, user, and desired outcome>

Constraints:
- Build the actual usable first screen or workflow, not a marketing placeholder.
- Choose conservative, common tooling unless I specify a stack.
- Include setup, run, test, lint/typecheck, and build instructions.
- Add durable agent instructions and project docs when useful.
- Create or update docs/ai/PROJECT_PROFILE.md so future agents understand this new project, not just the generic kit.
- Operate autonomously. Ask for approval only for the approval gates in docs/ai/APPROVAL_GATES.md.

Process:
1. Convert my idea into a concise SPEC.md with target users, core workflows, non-goals, data model, risks, and acceptance criteria.
2. Create a PLAN.md with milestones and verification steps.
3. Implement the smallest useful version end to end.
4. Add tests and checks proportional to risk.
5. Run verification.
6. Review the full diff, including generated files.
7. Update README and docs/ai files so a future agent can continue.
8. If packaged agent rules are added, make sure project-specific notes match this repository.

Approval gates:
Ask before production deploys, paid services, secrets, auth/payment/PII decisions, new production dependencies, CI/CD execution changes, destructive actions, broad architecture changes, or persistent agent rule changes.

Final output:
- What was built.
- How to run it.
- Checks run.
- Files changed.
- Approval gates still pending.
- Residual risks.
```
