namespace PublishingPlatform.SDK.Clients.BookPublishing.Requests;

/// <summary>
/// Produces header sets for book publishing requests.
/// </summary>
internal sealed class DefaultBookPublishingRequestHeadersFactory : IBookPublishingRequestHeadersFactory
{
    /// <inheritdoc />
    public IReadOnlyDictionary<string, string>? CreateIdempotencyHeaders(string? idempotencyKey)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            return null;
        }

        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Idempotency-Key"] = idempotencyKey,
        };
    }
}
