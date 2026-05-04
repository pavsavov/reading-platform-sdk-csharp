namespace PublishingPlatform.SDK.Models.Builders;

/// <summary>
/// Fluent builder for <see cref="UpdateBookPatchRequest"/>.
/// </summary>
public sealed class UpdateBookPatchRequestBuilder
{
    private readonly List<string> _tags = [];
    private string? _title;
    private string? _author;
    private string? _concurrencyToken;

    private UpdateBookPatchRequestBuilder()
    {
    }

    public static UpdateBookPatchRequestBuilder Create()
    {
        return new UpdateBookPatchRequestBuilder();
    }

    public UpdateBookPatchRequestBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public UpdateBookPatchRequestBuilder WithAuthor(string author)
    {
        _author = author;
        return this;
    }

    public UpdateBookPatchRequestBuilder WithTag(string tag)
    {
        _tags.Add(tag);
        return this;
    }

    public UpdateBookPatchRequestBuilder WithConcurrencyToken(string concurrencyToken)
    {
        _concurrencyToken = concurrencyToken;
        return this;
    }

    public UpdateBookPatchRequest Build()
    {
        return new UpdateBookPatchRequest
        {
            Title = _title,
            Author = _author,
            Tags = _tags.Count == 0 ? null : _tags,
            ConcurrencyToken = _concurrencyToken,
        };
    }
}
