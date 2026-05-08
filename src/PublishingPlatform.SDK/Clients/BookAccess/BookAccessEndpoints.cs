namespace PublishingPlatform.SDK.Clients.BookAccess;

/// <summary>
/// Defines book access route templates and route builders.
/// </summary>
internal static class BookAccessEndpoints
{
    internal const string Collection = "/book-access";

    internal static string ForBook(string bookId)
    {
        return $"/books/{Uri.EscapeDataString(bookId)}/access";
    }

    internal static string Revoke(string bookId)
    {
        return $"/books/{Uri.EscapeDataString(bookId)}/access/revoke";
    }

    internal static string Check(string bookId)
    {
        return $"/books/{Uri.EscapeDataString(bookId)}/access/check";
    }
}
