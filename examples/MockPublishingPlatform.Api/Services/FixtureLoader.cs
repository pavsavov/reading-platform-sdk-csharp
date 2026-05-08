using System.Text.Json;
using MockPublishingPlatform.Api.Models;
using PublishingPlatform.SDK.Models;

namespace MockPublishingPlatform.Api.Services;

/// <summary>
/// Loads deterministic fixture files and initializes mock API runtime state.
/// </summary>
public static class FixtureLoader
{
    /// <summary>
    /// Loads all fixture sets from disk.
    /// </summary>
    /// <param name="fixturesDirectory">Absolute directory path that contains fixture json files.</param>
    /// <returns>Initialized mutable mock state.</returns>
    public static MockApiState Load(string fixturesDirectory)
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var state = new MockApiState();

        var books = DeserializeFile<List<Book>>(fixturesDirectory, "books.json", options);
        var contents = DeserializeFile<List<BookContent>>(fixturesDirectory, "bookContent.json", options);
        var publishing = DeserializeFile<List<BookPublishingStatus>>(fixturesDirectory, "publishingStatuses.json", options);
        var distribution = DeserializeFile<List<BookDistributionOperation>>(fixturesDirectory, "distributionOperations.json", options);
        var webhooks = DeserializeFile<List<Webhook>>(fixturesDirectory, "webhooks.json", options);
        var errors = DeserializeFile<Dictionary<string, ApiErrorFixture>>(fixturesDirectory, "apiErrors.json", options);

        state.Books.AddRange(books);
        foreach (var item in contents)
        {
            state.BookContents[item.BookId] = item;
        }

        foreach (var item in publishing)
        {
            state.PublishingStatuses[item.BookId] = item;
        }

        foreach (var item in distribution)
        {
            state.DistributionOperations[item.OperationId] = item;
        }

        state.Webhooks.AddRange(webhooks);
        foreach (var item in errors)
        {
            state.ApiErrors[item.Key] = item.Value;
        }

        return state;
    }

    private static T DeserializeFile<T>(string fixturesDirectory, string fileName, JsonSerializerOptions options)
    {
        var fullPath = Path.Combine(fixturesDirectory, fileName);
        var json = File.ReadAllText(fullPath);
        return JsonSerializer.Deserialize<T>(json, options)
            ?? throw new InvalidOperationException($"Fixture '{fileName}' returned null.");
    }
}
