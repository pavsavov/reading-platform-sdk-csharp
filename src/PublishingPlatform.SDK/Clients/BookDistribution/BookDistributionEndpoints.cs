namespace PublishingPlatform.SDK.Clients.BookDistribution;

/// <summary>
/// Defines book distribution route templates and route builders.
/// </summary>
internal static class BookDistributionEndpoints
{
    internal static string Start(string bookId)
    {
        return $"/books/{Uri.EscapeDataString(bookId)}/distribution/start";
    }

    internal static string Status(string bookId, string operationId)
    {
        return $"/books/{Uri.EscapeDataString(bookId)}/distribution/{Uri.EscapeDataString(operationId)}/status";
    }

    internal static string Retry(string bookId, string operationId)
    {
        return $"/books/{Uri.EscapeDataString(bookId)}/distribution/{Uri.EscapeDataString(operationId)}/retry";
    }

    internal static string List(string bookId)
    {
        return $"/books/{Uri.EscapeDataString(bookId)}/distribution";
    }
}
