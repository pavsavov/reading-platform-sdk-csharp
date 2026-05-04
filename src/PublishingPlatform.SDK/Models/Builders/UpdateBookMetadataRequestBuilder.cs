namespace PublishingPlatform.SDK.Models.Builders;

/// <summary>
/// Fluent builder for <see cref="UpdateBookMetadataRequest"/>.
/// </summary>
public sealed class UpdateBookMetadataRequestBuilder
{
    private readonly List<string> _tags = [];
    private string _title = string.Empty;
    private string _author = string.Empty;
    private string? _concurrencyToken;

    private UpdateBookMetadataRequestBuilder()
    {
    }

    public static UpdateBookMetadataRequestBuilder Create()
    {
        return new UpdateBookMetadataRequestBuilder();
    }

    public UpdateBookMetadataRequestBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public UpdateBookMetadataRequestBuilder WithAuthor(string author)
    {
        _author = author;
        return this;
    }

    public UpdateBookMetadataRequestBuilder WithTag(string tag)
    {
        _tags.Add(tag);
        return this;
    }

    public UpdateBookMetadataRequestBuilder WithConcurrencyToken(string concurrencyToken)
    {
        _concurrencyToken = concurrencyToken;
        return this;
    }

    public UpdateBookMetadataRequest Build()
    {
        return new UpdateBookMetadataRequest
        {
            Title = _title,
            Author = _author,
            Tags = _tags,
            ConcurrencyToken = _concurrencyToken,
        };
    }
}
