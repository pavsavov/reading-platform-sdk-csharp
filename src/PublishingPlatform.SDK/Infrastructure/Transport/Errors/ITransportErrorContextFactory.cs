using PublishingPlatform.SDK.Abstractions;

namespace PublishingPlatform.SDK.Infrastructure.Transport.Errors;

internal interface ITransportErrorContextFactory
{
    PublishingPlatformErrorContext Create(
        HttpMethod method,
        string relativePath,
        int statusCode,
        NormalizedTransportError error,
        string? correlationId,
        string? operationName);
}
