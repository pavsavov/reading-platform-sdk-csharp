using System.Diagnostics;
using System.Net.Http.Json;
using PublishingPlatform.SDK.Abstractions;
using PublishingPlatform.SDK.Clients.Common.Pagination;
using PublishingPlatform.SDK.Clients.Webhooks.Requests;
using PublishingPlatform.SDK.Clients.Webhooks.Serialization;
using PublishingPlatform.SDK.Clients.Webhooks.Validation;
using PublishingPlatform.SDK.Infrastructure.Diagnostics;
using PublishingPlatform.SDK.Infrastructure.Transport;
using PublishingPlatform.SDK.Internal;
using PublishingPlatform.SDK.Models;
using PublishingPlatform.SDK.Models.Common;

namespace PublishingPlatform.SDK.Clients;

/// <summary>
/// Orchestrates webhook management operations through the shared SDK transport.
/// </summary>
public sealed class WebhooksClient : IWebhooksClient
{
    private const string RegisterOperationName = "Webhooks.Register";
    private const string UpdateOperationName = "Webhooks.Update";
    private const string DeleteOperationName = "Webhooks.Delete";
    private const string ListOperationName = "Webhooks.List";

    private readonly ISharedHttpTransport _transport;
    private readonly IWebhookRequestValidator _validator;
    private readonly IWebhookQueryStringBuilder _queryStringBuilder;
    private readonly IWebhookRequestHeadersFactory _requestHeadersFactory;
    private readonly IWebhookResponseReader _responseReader;

    internal WebhooksClient(ISharedHttpTransport transport)
        : this(
            transport,
            new DefaultWebhookRequestValidator(),
            new DefaultWebhookQueryStringBuilder(),
            new DefaultWebhookRequestHeadersFactory(),
            new DefaultWebhookResponseReader())
    {
    }

    internal WebhooksClient(
        ISharedHttpTransport transport,
        IWebhookRequestValidator validator,
        IWebhookQueryStringBuilder queryStringBuilder,
        IWebhookRequestHeadersFactory requestHeadersFactory,
        IWebhookResponseReader responseReader)
    {
        _transport = transport;
        _validator = validator;
        _queryStringBuilder = queryStringBuilder;
        _requestHeadersFactory = requestHeadersFactory;
        _responseReader = responseReader;
    }

    /// <inheritdoc />
    public async Task<Webhook> RegisterAsync(
        RegisterWebhookRequest request,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default)
    {
        Guards.NotNull(request, nameof(request));
        _validator.ValidateRegister(request);

        using var activity = StartActivity(RegisterOperationName);
        var headers = _requestHeadersFactory.CreateIdempotencyHeaders(idempotencyKey);
        var content = JsonContent.Create(request);
        using var response = await _transport.SendAsync(
            HttpMethod.Post,
            "/webhooks",
            content,
            headers,
            RegisterOperationName,
            cancellationToken).ConfigureAwait(false);

        return await _responseReader.ReadWebhookAsync(response, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Webhook> UpdateAsync(
        UpdateWebhookRequest request,
        CancellationToken cancellationToken = default)
    {
        Guards.NotNull(request, nameof(request));
        _validator.ValidateUpdate(request);

        using var activity = StartActivity(UpdateOperationName);
        var content = JsonContent.Create(request);
        using var response = await _transport.SendAsync(
            HttpMethod.Patch,
            $"/webhooks/{Uri.EscapeDataString(request.WebhookId)}",
            content,
            null,
            UpdateOperationName,
            cancellationToken).ConfigureAwait(false);

        return await _responseReader.ReadWebhookAsync(response, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(
        string webhookId,
        CancellationToken cancellationToken = default)
    {
        _validator.ValidateWebhookId(webhookId);

        using var activity = StartActivity(DeleteOperationName);
        using var response = await _transport.SendAsync(
            HttpMethod.Delete,
            $"/webhooks/{Uri.EscapeDataString(webhookId)}",
            null,
            null,
            DeleteOperationName,
            cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<PagedResult<Webhook>> ListAsync(
        ListWebhooksRequest request,
        CancellationToken cancellationToken = default)
    {
        Guards.NotNull(request, nameof(request));
        _validator.ValidateList(request);

        using var activity = StartActivity(ListOperationName);
        var relativePath = _queryStringBuilder.BuildListPath(request);
        using var response = await _transport.SendAsync(
            HttpMethod.Get,
            relativePath,
            null,
            null,
            ListOperationName,
            cancellationToken).ConfigureAwait(false);

        return await _responseReader.ReadPagedWebhooksAsync(response, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<Webhook> ListAllAsync(
        ListWebhooksRequest request,
        CancellationToken cancellationToken = default)
    {
        Guards.NotNull(request, nameof(request));
        _validator.ValidateList(request);

        return PagedAsyncIterator.IterateAsync(
            request,
            CloneListRequest,
            static (iterationRequest, continuationToken) => iterationRequest.ContinuationToken = continuationToken,
            ListAsync,
            cancellationToken);
    }

    private static Activity? StartActivity(string operationName)
    {
        var activity = ActivitySourceProvider.ActivitySource.StartActivity(operationName, ActivityKind.Client);
        activity?.SetTag("sdk.operation", operationName);
        return activity;
    }

    private static ListWebhooksRequest CloneListRequest(ListWebhooksRequest request)
    {
        return new ListWebhooksRequest
        {
            PageSize = request.PageSize,
            ContinuationToken = request.ContinuationToken,
            Event = request.Event,
            IsActive = request.IsActive,
        };
    }
}
