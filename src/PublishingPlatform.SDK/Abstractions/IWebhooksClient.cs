using PublishingPlatform.SDK.Models;
using PublishingPlatform.SDK.Models.Common;

namespace PublishingPlatform.SDK.Abstractions;

/// <summary>
/// Provides webhook registration and management operations for platform integrations.
/// </summary>
public interface IWebhooksClient
{
    /// <summary>
    /// Registers a webhook callback endpoint for asynchronous platform events.
    /// </summary>
    /// <param name="request">The webhook registration request.</param>
    /// <param name="idempotencyKey">An optional idempotency key for retry-safe registration.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The registered webhook snapshot.</returns>
    Task<Webhook> RegisterAsync(
        RegisterWebhookRequest request,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing webhook callback endpoint and subscription settings.
    /// </summary>
    /// <param name="request">The webhook update request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated webhook snapshot.</returns>
    Task<Webhook> UpdateAsync(
        UpdateWebhookRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a webhook registration by identifier.
    /// </summary>
    /// <param name="webhookId">The unique webhook identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the webhook has been deleted.</returns>
    Task DeleteAsync(
        string webhookId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists webhook registrations with optional filtering.
    /// </summary>
    /// <param name="request">The webhook list request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paged result of webhook registrations.</returns>
    Task<PagedResult<Webhook>> ListAsync(
        ListWebhooksRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams webhook registrations across all pages for the provided list request.
    /// </summary>
    /// <param name="request">The webhook list request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An async sequence of webhook registrations.</returns>
    IAsyncEnumerable<Webhook> ListAllAsync(
        ListWebhooksRequest request,
        CancellationToken cancellationToken = default);
}
