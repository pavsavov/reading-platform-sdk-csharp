namespace PublishingPlatform.SDK.Infrastructure.Transport;

internal sealed class TransportErrorResponse
{
    public string? Message { get; set; }

    public string? ErrorCode { get; set; }

    public string? CorrelationId { get; set; }

    public string? RequestId { get; set; }
}
