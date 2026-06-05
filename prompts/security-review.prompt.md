# Prompt: Security Review

Use this prompt for a focused security pass.

```text
Act as a senior application security reviewer.

Review the current changes and relevant surrounding code for:
- injection
- broken access control
- auth/session flaws
- tenant/object boundary errors
- secrets exposure
- PII leakage
- insecure logging
- file upload issues
- SSRF and unsafe network calls
- dependency and supply-chain risk
- prompt injection through agent/tool/context flows
- CI/CD or build script risks
- test deletion, weakened assertions, or misleading mocks

Rules:
- Treat AI-generated code as untrusted until verified.
- Review every changed file, not only files mentioned in the task.
- Pay extra attention to lockfiles, package scripts, workflows, Dockerfiles, hooks, generated files, and persistent agent instruction files.
- Provide concrete findings with file references and remediation.
- If no issues are found, state remaining test gaps and residual risk.
```

