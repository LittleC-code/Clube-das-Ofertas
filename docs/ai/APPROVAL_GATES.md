# Human Approval Gates

The agent should ask for human approval only when one of these gates is triggered.

## Destructive Or Irreversible

- Delete or overwrite user data.
- Drop, truncate, or irreversibly migrate a database.
- Rewrite git history, force push, or delete branches/tags.
- Bulk-delete files or generated assets outside a clearly temporary folder.
- Rotate or revoke credentials.

## Production Or Cost

- Deploy to production.
- Change DNS, domains, certificates, production env vars, production secrets, queues, cron jobs, or feature flags.
- Create paid cloud resources, external services, paid APIs, or resources with recurring cost.
- Run load tests or jobs that may create cost spikes.

## Security, Privacy, Compliance

- Change authentication, authorization, session handling, password reset, MFA, permissions, tenancy, audit logs, encryption, payments, or PII processing.
- Handle incidents or suspected compromise.
- Change legal/compliance-relevant behavior such as retention, deletion, consent, exports, or auditability.

## Supply Chain

- Add a production dependency, SDK, package registry, binary, third-party GitHub Action, MCP server, plugin, browser extension, or external tool.
- Add install/build/deploy steps that fetch or execute remote code.
- Relax dependency scanning, pinning, lockfile controls, or package integrity checks.

## CI/CD And Execution Paths

- Modify CI workflows, release scripts, Dockerfiles, package scripts, Makefiles, hooks, deployment manifests, or code executed during install/build/test/deploy.
- Grant write permissions, secrets access, deploy keys, or broad tokens to automation.

## Persistent Agent Steering

- Modify existing `AGENTS.md`, `CLAUDE.md`, `.github/copilot-instructions.md`, `.github/instructions/*`, `.cursor/rules/*`, `.aider*`, `.windsurfrules`, MCP definitions, hooks, or custom system prompts.
- Add rules that weaken existing review, testing, security, or approval gates.
- Allow the agent to modify its own operating rules without review.

## Broad Or Ambiguous Scope

- Large architectural rewrites.
- Major framework or database migrations.
- Tasks with unclear requirements where the wrong assumption can cause data loss, security exposure, user harm, major cost, or difficult rollback.

## Approval Request Format

When approval is required, ask one concise question:

```text
Approval needed: <action>.
Reason: <specific gate and risk>.
Impact if approved: <what will change>.
Rollback: <how to undo or why rollback is hard>.
Proceed?
```

