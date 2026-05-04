# Retry Policy

Use `RetryPolicyExample.csx` to configure retry behavior with explicit defaults:

- `RetryNonIdempotentMethods = false`:
  retries only idempotent requests (`GET`, `PUT`, `DELETE`, `HEAD`, `OPTIONS`, `TRACE`).
- `RetryNonIdempotentMethods = true`:
  allows retries for `POST` and `PATCH` using the SDK's safer transient set.

This keeps non-idempotent retries explicit and opt-in.
