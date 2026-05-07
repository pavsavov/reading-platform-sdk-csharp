using System.Runtime.CompilerServices;
using PublishingPlatform.SDK.Models.Common;

namespace PublishingPlatform.SDK.Clients.Common.Pagination;

/// <summary>
/// Iterates paged API responses through continuation-token traversal.
/// </summary>
internal static class PagedAsyncIterator
{
    /// <summary>
    /// Streams all items from a paged endpoint by repeatedly loading pages until no continuation token is returned.
    /// </summary>
    /// <typeparam name="TRequest">The request model used to load each page.</typeparam>
    /// <typeparam name="TItem">The item type returned by the paged endpoint.</typeparam>
    /// <param name="request">The initial list request.</param>
    /// <param name="cloneRequest">Clones the request so caller-owned instances are not mutated.</param>
    /// <param name="setContinuationToken">Updates continuation token before the next page request.</param>
    /// <param name="loadPage">Loads a page using the provided request and cancellation token.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An async stream of all items across pages.</returns>
    public static async IAsyncEnumerable<TItem> IterateAsync<TRequest, TItem>(
        TRequest request,
        Func<TRequest, TRequest> cloneRequest,
        Action<TRequest, string> setContinuationToken,
        Func<TRequest, CancellationToken, Task<PagedResult<TItem>>> loadPage,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var iterationRequest = cloneRequest(request);
        while (true)
        {
            var page = await loadPage(iterationRequest, cancellationToken).ConfigureAwait(false);
            foreach (var item in page.Items)
            {
                yield return item;
            }

            if (string.IsNullOrWhiteSpace(page.ContinuationToken))
            {
                yield break;
            }

            setContinuationToken(iterationRequest, page.ContinuationToken);
        }
    }
}
