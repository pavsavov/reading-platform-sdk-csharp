using MockPublishingPlatform.Api.Models;
using PublishingPlatform.SDK.Models;

namespace MockPublishingPlatform.Api.Services;

/// <summary>
/// Holds mutable in-memory mock API state initialized from deterministic fixtures.
/// </summary>
public sealed class MockApiState
{
    /// <summary>
    /// Gets the books collection.
    /// </summary>
    public List<Book> Books { get; } = [];

    /// <summary>
    /// Gets the current book content snapshots keyed by book id.
    /// </summary>
    public Dictionary<string, BookContent> BookContents { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets publishing statuses keyed by book id.
    /// </summary>
    public Dictionary<string, BookPublishingStatus> PublishingStatuses { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets distribution operations keyed by operation id.
    /// </summary>
    public Dictionary<string, BookDistributionOperation> DistributionOperations { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets webhook registrations.
    /// </summary>
    public List<Webhook> Webhooks { get; } = [];

    /// <summary>
    /// Gets API error fixtures keyed by fixture id.
    /// </summary>
    public Dictionary<string, ApiErrorFixture> ApiErrors { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets idempotency replay cache for mutating operations.
    /// </summary>
    public Dictionary<string, object> IdempotencyResponses { get; } = new(StringComparer.Ordinal);
}
