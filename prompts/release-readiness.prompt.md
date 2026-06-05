# Prompt: Release Readiness Review

Use this before merging, tagging, or deploying.

```text
Act as a release readiness reviewer.

Assess whether the current branch is ready to merge or release.

Check:
- acceptance criteria satisfied
- tests/lint/typecheck/build status
- security checklist
- dependency and lockfile changes
- migrations and rollback
- docs and changelog
- package output sync when this is a reusable kit
- production config changes
- feature flags
- observability/logging
- error handling
- accessibility/performance where relevant
- human approval gates

Output:
1. Ready / Not Ready.
2. Blocking issues.
3. Non-blocking risks.
4. Checks run and checks not run.
5. Approval gates that still require a human.
```
