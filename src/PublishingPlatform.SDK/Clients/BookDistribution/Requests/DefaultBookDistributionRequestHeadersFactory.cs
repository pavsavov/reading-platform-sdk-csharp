namespace PublishingPlatform.SDK.Clients.BookDistribution.Requests;

/// <summary>
/// Produces header sets for book distribution requests.
/// </summary>
internal sealed class DefaultBookDistributionRequestHeadersFactory : IBookDistributionRequestHeadersFactory
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
