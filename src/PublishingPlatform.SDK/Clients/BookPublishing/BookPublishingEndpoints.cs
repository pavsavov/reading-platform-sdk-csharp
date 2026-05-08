namespace PublishingPlatform.SDK.Clients.BookPublishing;

/// <summary>
/// Defines book publishing route templates and route builders.
/// </summary>
internal static class BookPublishingEndpoints
{
    internal static string Publish(string bookId)
    {
        return $"/books/{Uri.EscapeDataString(bookId)}/publishing/publish";
    }

    internal static string Unpublish(string bookId)
    {
        return $"/books/{Uri.EscapeDataString(bookId)}/publishing/unpublish";
    }

    internal static string Schedule(string bookId)
    {
        return $"/books/{Uri.EscapeDataString(bookId)}/publishing/schedule";
    }

    internal static string Status(string bookId)
    {
        return $"/books/{Uri.EscapeDataString(bookId)}/publishing/status";
    }
}
