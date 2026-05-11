using PublishingPlatform.SDK.Abstractions;
using PublishingPlatform.SDK.Infrastructure.Transport;
using PublishingPlatform.SDK.Models;
using PublishingPlatform.SDK.Options;

namespace PublishingPlatform.SDK.Extensions;

/// <summary>
/// Provides request-options overloads for idempotency-aware SDK operations without changing existing contracts.
/// </summary>
public static class PublishingPlatformRequestOptionsExtensions
{
    /// <summary>
    /// Publishes a book with optional request-scoped options.
    /// </summary>
    /// <param name="client">The publishing client.</param>
    /// <param name="bookId">The unique book identifier.</param>
    /// <param name="request">The publish payload.</param>
    /// <param name="requestOptions">The request-scoped options.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The resulting publishing status.</returns>
    public static Task<BookPublishingStatus> PublishAsync(
        this IBookPublishingClient client,
        string bookId,
        PublishBookRequest request,
        PublishingPlatformRequestOptions? requestOptions,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        using var scope = RequestScopedCorrelationContext.Push(requestOptions?.CorrelationId);
        return client.PublishAsync(bookId, request, requestOptions?.IdempotencyKey, ct);
    }

    /// <summary>
    /// Unpublishes a book with optional request-scoped options.
    /// </summary>
    /// <param name="client">The publishing client.</param>
    /// <param name="bookId">The unique book identifier.</param>
    /// <param name="requestOptions">The request-scoped options.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The resulting publishing status.</returns>
    public static Task<BookPublishingStatus> UnpublishAsync(
        this IBookPublishingClient client,
        string bookId,
        PublishingPlatformRequestOptions? requestOptions,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        using var scope = RequestScopedCorrelationContext.Push(requestOptions?.CorrelationId);
        return client.UnpublishAsync(bookId, requestOptions?.IdempotencyKey, ct);
    }

    /// <summary>
    /// Schedules publishing for a book with optional request-scoped options.
    /// </summary>
    /// <param name="client">The publishing client.</param>
    /// <param name="bookId">The unique book identifier.</param>
    /// <param name="request">The schedule payload.</param>
    /// <param name="requestOptions">The request-scoped options.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The resulting publishing status.</returns>
    public static Task<BookPublishingStatus> ScheduleAsync(
        this IBookPublishingClient client,
        string bookId,
        ScheduleBookPublishingRequest request,
        PublishingPlatformRequestOptions? requestOptions,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        using var scope = RequestScopedCorrelationContext.Push(requestOptions?.CorrelationId);
        return client.ScheduleAsync(bookId, request, requestOptions?.IdempotencyKey, ct);
    }

    /// <summary>
    /// Starts a distribution operation with optional request-scoped options.
    /// </summary>
    /// <param name="client">The distribution client.</param>
    /// <param name="bookId">The unique book identifier.</param>
    /// <param name="request">The distribution start payload.</param>
    /// <param name="requestOptions">The request-scoped options.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The created distribution operation snapshot.</returns>
    public static Task<BookDistributionOperation> StartAsync(
        this IBookDistributionClient client,
        string bookId,
        StartBookDistributionRequest request,
        PublishingPlatformRequestOptions? requestOptions,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        using var scope = RequestScopedCorrelationContext.Push(requestOptions?.CorrelationId);
        return client.StartAsync(bookId, request, requestOptions?.IdempotencyKey, ct);
    }

    /// <summary>
    /// Retries a distribution operation with optional request-scoped options.
    /// </summary>
    /// <param name="client">The distribution client.</param>
    /// <param name="bookId">The unique book identifier.</param>
    /// <param name="operationId">The unique distribution operation identifier.</param>
    /// <param name="requestOptions">The request-scoped options.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The distribution operation snapshot.</returns>
    public static Task<BookDistributionOperation> RetryAsync(
        this IBookDistributionClient client,
        string bookId,
        string operationId,
        PublishingPlatformRequestOptions? requestOptions,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        using var scope = RequestScopedCorrelationContext.Push(requestOptions?.CorrelationId);
        return client.RetryAsync(bookId, operationId, requestOptions?.IdempotencyKey, ct);
    }

    /// <summary>
    /// Registers a webhook with optional request-scoped options.
    /// </summary>
    /// <param name="client">The webhooks client.</param>
    /// <param name="request">The registration payload.</param>
    /// <param name="requestOptions">The request-scoped options.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The registered webhook snapshot.</returns>
    public static Task<Webhook> RegisterAsync(
        this IWebhooksClient client,
        RegisterWebhookRequest request,
        PublishingPlatformRequestOptions? requestOptions,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        using var scope = RequestScopedCorrelationContext.Push(requestOptions?.CorrelationId);
        return client.RegisterAsync(request, requestOptions?.IdempotencyKey, ct);
    }
}
