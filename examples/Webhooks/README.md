# Webhooks Example

This example demonstrates webhook registration and management through `IWebhooksClient`:

- `RegisterAsync(...)`
- `UpdateAsync(...)`
- `ListAsync(...)`
- `DeleteAsync(...)`

It maps request values to:

- `POST /webhooks`
- `PATCH /webhooks/{webhookId}`
- `GET /webhooks?pageSize=...`
- `DELETE /webhooks/{webhookId}`

Notes:

- Callback endpoints must be absolute HTTPS URLs.
- `SigningKeyId` identifies a key managed outside the SDK; do not put signing secrets in code or examples.

Run:

- `examples/Webhooks/WebhooksExample.csx`
