namespace PublishingPlatform.SDK.Clients.BookContent;

/// <summary>
/// Defines book content route templates and route builders.
/// </summary>
internal static class BookContentEndpoints
{
    internal static string ByBookId(string bookId)
    {
        return $"/books/{Uri.EscapeDataString(bookId)}/content";
    }
}
