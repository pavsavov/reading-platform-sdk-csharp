namespace PublishingPlatform.SDK.Clients.Webhooks.Requests;

/// <summary>
/// Produces header sets for webhook management requests.
/// </summary>
internal sealed class DefaultWebhookRequestHeadersFactory : IWebhookRequestHeadersFactory
{
    private const string IdempotencyHeaderName = "Idempotency-Key";

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string>? CreateIdempotencyHeaders(string? idempotencyKey)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            return null;
        }

        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [IdempotencyHeaderName] = idempotencyKey,
        };
    }
}
