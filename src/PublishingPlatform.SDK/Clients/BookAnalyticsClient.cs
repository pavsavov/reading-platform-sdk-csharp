using PublishingPlatform.SDK.Abstractions;
using PublishingPlatform.SDK.Clients.BookAnalytics.Serialization;
using PublishingPlatform.SDK.Clients.BookAnalytics.Validation;
using PublishingPlatform.SDK.Infrastructure.Transport;
using PublishingPlatform.SDK.Internal;
using PublishingPlatform.SDK.Models;

namespace PublishingPlatform.SDK.Clients;

/// <summary>
/// Orchestrates book analytics operations through the shared transport.
/// </summary>
public sealed class BookAnalyticsClient : IBookAnalyticsClient
{
    private const string GetSummaryOperationName = "BookAnalytics.GetSummary";

    private readonly ISharedHttpTransport _transport;
    private readonly IBookAnalyticsRequestValidator _validator;
    private readonly IBookAnalyticsQueryStringBuilder _queryStringBuilder;
    private readonly IBookAnalyticsResponseReader _responseReader;

    internal BookAnalyticsClient(ISharedHttpTransport transport)
        : this(
            transport,
            new DefaultBookAnalyticsRequestValidator(),
            new DefaultBookAnalyticsQueryStringBuilder(),
            new DefaultBookAnalyticsResponseReader())
    {
    }

    internal BookAnalyticsClient(
        ISharedHttpTransport transport,
        IBookAnalyticsRequestValidator validator,
        IBookAnalyticsQueryStringBuilder queryStringBuilder,
        IBookAnalyticsResponseReader responseReader)
    {
        _transport = transport;
        _validator = validator;
        _queryStringBuilder = queryStringBuilder;
        _responseReader = responseReader;
    }

    /// <inheritdoc />
    public async Task<AnalyticsSummary> GetSummaryAsync(
        GetBookAnalyticsRequest request,
        CancellationToken cancellationToken = default)
    {
        Guards.NotNull(request, nameof(request));
        _validator.ValidateGetSummary(request);

        var relativePath = _queryStringBuilder.BuildSummaryPath(request);
        using var response = await _transport.SendAsync(
            HttpMethod.Get,
            relativePath,
            null,
            null,
            GetSummaryOperationName,
            cancellationToken).ConfigureAwait(false);

        return await _responseReader.ReadSummaryAsync(response, cancellationToken).ConfigureAwait(false);
    }
}
