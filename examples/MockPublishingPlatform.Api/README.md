# MockPublishingPlatform.Api

Minimal mock backend used for SDK testing and interview showcase flows from ticket `RPS-104`.

## What it includes

- Fixture-backed in-memory data (no database).
- Deterministic responses for:
  - books core flow
  - content upload metadata flow
  - publishing status flow
  - distribution start/status flow
  - webhook lifecycle flow
- `X-Correlation-Id` response header echo/generation.
- Deterministic idempotency replay for mutating endpoints when `Idempotency-Key` is present.

## Run locally

1. Start the mock API:

```powershell
dotnet run --project examples/MockPublishingPlatform.Api/MockPublishingPlatform.Api.csproj --launch-profile https
```

2. Run SDK showcase script:

```powershell
dotnet script examples/MockApiCoreFlowExample.csx
```

## Notes

- The SDK requires HTTPS base URLs, so use the `https` launch profile.
- This mock is intentionally minimal and does not implement the full OpenAPI contract.
