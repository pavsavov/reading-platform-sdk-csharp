# Architecture Tradeoffs Example

This example shows the consumer-facing choices behind the book-centric SDK architecture:

- one root client with book lifecycle modules
- shared transport behavior for correlation, diagnostics, resilience, and errors
- conservative retry defaults for side-effecting requests
- explicit idempotency keys for replay-safe publishing and distribution operations
- privacy-preserving diagnostics that stay opt-in

Run `ArchitectureTradeoffsExample.csx` after setting a valid API base URL and API key.
