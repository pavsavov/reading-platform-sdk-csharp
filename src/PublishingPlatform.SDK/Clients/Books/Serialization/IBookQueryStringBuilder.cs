using PublishingPlatform.SDK.Models;

namespace PublishingPlatform.SDK.Clients.Books.Serialization;

internal interface IBookQueryStringBuilder
{
    string BuildListPath(ListBooksRequest request);
}
