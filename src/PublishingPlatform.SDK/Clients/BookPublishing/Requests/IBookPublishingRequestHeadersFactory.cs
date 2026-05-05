namespace PublishingPlatform.SDK.Clients.BookPublishing.Requests;

/// <summary>
/// Creates request headers for book publishing transport calls.
/// </summary>
internal interface IBookPublishingRequestHeadersFactory
{
    /// <summary>
    /// Creates idempotency headers when an idempotency key is provided.
    /// </summary>
    /// <param name="idempotencyKey">The optional idempotency key.</param>
    /// <returns>A headers dictionary, or <see langword="null"/> when no headers are needed.</returns>
    IReadOnlyDictionary<string, string>? CreateIdempotencyHeaders(string? idempotencyKey);
}
