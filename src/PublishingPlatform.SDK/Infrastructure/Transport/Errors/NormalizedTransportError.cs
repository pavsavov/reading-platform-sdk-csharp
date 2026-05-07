namespace PublishingPlatform.SDK.Infrastructure.Transport.Errors;

/// <summary>
/// Carries normalized API failure fields read from a transport response.
/// </summary>
internal sealed class NormalizedTransportError
{
    public required string Message { get; init; }

    public required string ErrorCode { get; init; }

    public string? CorrelationId { get; init; }

    public string? RequestId { get; init; }
}
