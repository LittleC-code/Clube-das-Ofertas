# AI-Assisted Development Security Checklist

Use this checklist before merging or releasing AI-generated or AI-modified code.

## Prompt And Context Hygiene

- No secrets, tokens, private keys, `.env` contents, PII, customer data, or proprietary dumps were placed in prompts, logs, screenshots, or generated docs.
- External content used as context was treated as untrusted input.
- Issues, PR comments, logs, dependency changelogs, fetched docs, and tool output were checked for suspicious instructions.

## Dependencies And Supply Chain

- Every new package exists in the expected registry.
- Package age, maintainer, download/use signals, license, and security posture are reasonable.
- Dependencies are pinned or locked according to project practice.
- Dependency audit was run where available.
- Lockfile changes were reviewed intentionally.
- No build/install/deploy script downloads or executes remote code without approval.

## Code Security

- User input is validated and encoded at the correct boundary.
- Database access uses parameterized queries or safe ORM APIs.
- Authorization checks cover every role/tenant/object boundary.
- Secrets are stored in approved secret management, not source files.
- Logs avoid secrets, PII, tokens, session identifiers, and sensitive payloads.
- File uploads validate type, size, path, scanning needs, and storage permissions.
- Network calls have timeouts, allowlists where appropriate, and no SSRF exposure.
- Crypto uses standard libraries and established project patterns.

## Tests

- Positive tests cover expected behavior.
- Negative/adversarial tests cover invalid input, malformed payloads, expired tokens, unauthorized roles, boundary values, concurrency, and error paths where relevant.
- Existing tests were not deleted or weakened to make CI pass.
- New mocks do not replace the behavior the test is meant to verify.
- Security-critical code and its tests were not both accepted without independent review.

## Diff Review

- Every changed file was reviewed, including generated files and lockfiles.
- Out-of-scope files were either reverted or explicitly justified.
- CI/CD, Docker, package scripts, hooks, migrations, and agent rule files received extra scrutiny.

## Release Readiness

- Build/test/lint/typecheck passed or failures are documented with cause.
- Rollback plan exists for production changes.
- A human owner is named for the change.
- Residual risks are listed in the final summary or PR body.

