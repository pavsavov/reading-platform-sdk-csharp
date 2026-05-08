using MockPublishingPlatform.Api.Services;
using PublishingPlatform.SDK.Models;
using PublishingPlatform.SDK.Models.Common;

namespace MockPublishingPlatform.Api.Endpoints;

/// <summary>
/// Maps mock API endpoints for webhook registration lifecycle operations.
/// </summary>
internal static class WebhooksEndpointsModule
{
    internal static void Map(WebApplication app)
    {
        app.MapPost("/webhooks", (RegisterWebhookRequest request, HttpRequest httpRequest, MockApiState state) =>
        {
            var idempotencyKey = IdempotencyHelper.Resolve(httpRequest);
            if (IdempotencyHelper.TryReplay<Webhook>(state, HttpMethod.Post.Method, "/webhooks", idempotencyKey, out var replayed))
            {
                return Results.Ok(replayed);
            }

            if (string.IsNullOrWhiteSpace(request.EndpointUrl) || request.Events.Count == 0)
            {
                return MockApiResultFactory.CreateValidationError(httpRequest, state);
            }

            var webhook = new Webhook
            {
                Id = $"wh-{state.Webhooks.Count + 1:D4}",
                EndpointUrl = request.EndpointUrl.Trim(),
                Events = request.Events.Select(value => value.Trim()).Where(value => !string.IsNullOrWhiteSpace(value)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
                IsActive = request.IsActive,
                SigningKeyId = request.SigningKeyId,
                CreatedAt = MockApiConstants.WebhookCreatedTimestamp,
            };

            state.Webhooks.Add(webhook);
            IdempotencyHelper.StoreReplay(state, HttpMethod.Post.Method, "/webhooks", idempotencyKey, webhook);

            return Results.Ok(webhook);
        });

        app.MapGet("/webhooks", (HttpRequest httpRequest, MockApiState state) =>
        {
            var pageSize = QueryParser.ParsePositiveInt(httpRequest.Query["pageSize"], defaultValue: 50, min: 1, max: 500);
            var eventFilter = httpRequest.Query["event"].ToString();
            var isActiveFilter = QueryParser.ParseNullableBoolean(httpRequest.Query["isActive"]);
            var continuationToken = httpRequest.Query["continuationToken"].ToString();
            var start = QueryParser.ParseContinuationOffset(continuationToken);

            IEnumerable<Webhook> filtered = state.Webhooks;
            if (!string.IsNullOrWhiteSpace(eventFilter))
            {
                filtered = filtered.Where(webhook => webhook.Events.Contains(eventFilter, StringComparer.OrdinalIgnoreCase));
            }

            if (isActiveFilter.HasValue)
            {
                filtered = filtered.Where(webhook => webhook.IsActive == isActiveFilter.Value);
            }

            var total = filtered.Count();
            var items = filtered.Skip(start).Take(pageSize).ToArray();
            var next = start + items.Length < total ? $"offset:{start + items.Length}" : null;
            var result = new PagedResult<Webhook>
            {
                Items = items,
                TotalCount = total,
                ContinuationToken = next,
            };

            return Results.Ok(result);
        });

        app.MapPatch("/webhooks/{webhookId}", (string webhookId, UpdateWebhookRequest request, HttpRequest httpRequest, MockApiState state) =>
        {
            var existing = state.Webhooks.FirstOrDefault(webhook => string.Equals(webhook.Id, webhookId, StringComparison.OrdinalIgnoreCase));
            if (existing is null)
            {
                return MockApiResultFactory.CreateNotFound(httpRequest, state);
            }

            existing.EndpointUrl = request.EndpointUrl.Trim();
            existing.Events = request.Events.Select(value => value.Trim()).Where(value => !string.IsNullOrWhiteSpace(value)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            existing.IsActive = request.IsActive;
            existing.UpdatedAt = MockApiConstants.WebhookUpdatedTimestamp;
            return Results.Ok(existing);
        });

        app.MapDelete("/webhooks/{webhookId}", (string webhookId, HttpRequest httpRequest, MockApiState state) =>
        {
            var removed = state.Webhooks.RemoveAll(webhook => string.Equals(webhook.Id, webhookId, StringComparison.OrdinalIgnoreCase));
            if (removed == 0)
            {
                return MockApiResultFactory.CreateNotFound(httpRequest, state);
            }

            return Results.NoContent();
        });
    }
}
