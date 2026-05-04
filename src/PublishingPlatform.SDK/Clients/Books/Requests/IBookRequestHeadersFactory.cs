namespace PublishingPlatform.SDK.Clients.Books.Requests;

internal interface IBookRequestHeadersFactory
{
    IReadOnlyDictionary<string, string>? CreateIdempotencyHeaders(string? idempotencyKey);

    IReadOnlyDictionary<string, string>? CreateConcurrencyHeaders(string? concurrencyToken);
}
