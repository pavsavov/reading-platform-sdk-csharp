using PublishingPlatform.SDK.Abstractions;

namespace PublishingPlatform.SDK.Infrastructure.Transport.Errors;

internal sealed class DefaultTransportErrorContextFactory : ITransportErrorContextFactory
{
    public PublishingPlatformErrorContext Create(
        HttpMethod method,
        string relativePath,
        int statusCode,
        string message,
        string correlationId,
        string? operationName)
    {
        return new PublishingPlatformErrorContext
        {
            Method = method,
            RelativePath = relativePath,
            StatusCode = statusCode,
            Message = message,
            CorrelationId = correlationId,
            OperationName = operationName,
        };
    }
}
