namespace PublishingPlatform.SDK.Clients.Webhooks.Requests;

/// <summary>
/// Produces header sets for webhook management requests.
/// </summary>
internal interface IWebhookRequestHeadersFactory
{
    /// <summary>
    /// Creates idempotency headers for retry-safe webhook registration.
    /// </summary>
    /// <param name="idempotencyKey">The optional idempotency key.</param>
    /// <returns>A header dictionary when a key is provided; otherwise <see langword="null"/>.</returns>
    IReadOnlyDictionary<string, string>? CreateIdempotencyHeaders(string? idempotencyKey);
}
