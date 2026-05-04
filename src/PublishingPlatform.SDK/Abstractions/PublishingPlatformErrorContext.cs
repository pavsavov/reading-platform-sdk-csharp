namespace PublishingPlatform.SDK.Abstractions;

public sealed class PublishingPlatformErrorContext
{
    public required HttpMethod Method { get; init; }

    public required string RelativePath { get; init; }

    public required int StatusCode { get; init; }

    public required string Message { get; init; }

    public string? CorrelationId { get; init; }

    public string? OperationName { get; init; }
}
