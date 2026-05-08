namespace MockPublishingPlatform.Api.Services;

/// <summary>
/// Provides deterministic idempotency helpers for mock mutating endpoints.
/// </summary>
public static class IdempotencyHelper
{
    private const string IdempotencyHeaderName = "Idempotency-Key";

    /// <summary>
    /// Resolves idempotency key from request headers.
    /// </summary>
    /// <param name="request">The incoming HTTP request.</param>
    /// <returns>Header value when present; otherwise null.</returns>
    public static string? Resolve(HttpRequest request)
    {
        if (request.Headers.TryGetValue(IdempotencyHeaderName, out var values))
        {
            var value = values.ToString().Trim();
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        return null;
    }

    /// <summary>
    /// Attempts to replay a previous mutating result.
    /// </summary>
    /// <typeparam name="T">Expected replay payload type.</typeparam>
    /// <param name="state">Runtime state.</param>
    /// <param name="operationKey">Stable operation + key tuple.</param>
    /// <param name="replayed">The replayed value when found.</param>
    /// <returns>True when replay is available; otherwise false.</returns>
    public static bool TryReplay<T>(MockApiState state, string operationKey, out T replayed)
    {
        if (state.IdempotencyResponses.TryGetValue(operationKey, out var existing)
            && existing is T typed)
        {
            replayed = typed;
            return true;
        }

        replayed = default!;
        return false;
    }

    /// <summary>
    /// Attempts to replay a previous mutating result using a method/path/idempotency tuple.
    /// </summary>
    /// <typeparam name="T">Expected replay payload type.</typeparam>
    /// <param name="state">Runtime state.</param>
    /// <param name="method">HTTP method name.</param>
    /// <param name="path">Request path key.</param>
    /// <param name="idempotencyKey">Resolved idempotency key.</param>
    /// <param name="replayed">The replayed value when found.</param>
    /// <returns>True when replay is available; otherwise false.</returns>
    public static bool TryReplay<T>(
        MockApiState state,
        string method,
        string path,
        string? idempotencyKey,
        out T replayed)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            replayed = default!;
            return false;
        }

        return TryReplay(state, BuildOperationKey(method, path, idempotencyKey), out replayed);
    }

    /// <summary>
    /// Stores a replay payload for a method/path/idempotency tuple when an idempotency key is present.
    /// </summary>
    /// <param name="state">Runtime state.</param>
    /// <param name="method">HTTP method name.</param>
    /// <param name="path">Request path key.</param>
    /// <param name="idempotencyKey">Resolved idempotency key.</param>
    /// <param name="payload">Payload to replay.</param>
    public static void StoreReplay(
        MockApiState state,
        string method,
        string path,
        string? idempotencyKey,
        object payload)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            return;
        }

        state.IdempotencyResponses[BuildOperationKey(method, path, idempotencyKey)] = payload;
    }

    /// <summary>
    /// Builds a stable operation key from HTTP method, request path key, and idempotency key.
    /// </summary>
    /// <param name="method">HTTP method name.</param>
    /// <param name="path">Request path key.</param>
    /// <param name="idempotencyKey">Idempotency key value.</param>
    /// <returns>The stable replay key.</returns>
    public static string BuildOperationKey(string method, string path, string idempotencyKey)
    {
        return $"{method}:{path}:{idempotencyKey}";
    }
}
