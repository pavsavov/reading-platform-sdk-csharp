# Retry Policy

Use `RetryPolicyExample.csx` to configure retry behavior with explicit defaults:

- `RetryNonIdempotentMethods = false`:
  retries only idempotent requests (`GET`, `PUT`, `DELETE`, `HEAD`, `OPTIONS`, `TRACE`).
- `RetryNonIdempotentMethods = true`:
  allows retries for `POST` and `PATCH` using the SDK's safer transient set when idempotency key and replay-safe content requirements are met.
- `UseJitter = true`:
  keeps exponential backoff randomized in production; set to `false` for deterministic tests when needed.

This keeps non-idempotent retries explicit and opt-in.
