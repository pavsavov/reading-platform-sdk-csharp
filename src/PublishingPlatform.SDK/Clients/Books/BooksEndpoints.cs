namespace PublishingPlatform.SDK.Clients.Books;

/// <summary>
/// Defines book route templates and route builders.
/// </summary>
internal static class BooksEndpoints
{
    internal const string Collection = "/books";

    internal static string ById(string bookId)
    {
        return $"/books/{Uri.EscapeDataString(bookId)}";
    }
}
