namespace PublishingPlatform.SDK.Clients.BookContent.Requests;

internal interface IBookContentRequestHeadersFactory
{
    IReadOnlyDictionary<string, string>? CreateIdempotencyHeaders(string? idempotencyKey);
}
