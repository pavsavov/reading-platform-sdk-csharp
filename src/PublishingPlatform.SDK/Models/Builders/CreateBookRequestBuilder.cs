namespace PublishingPlatform.SDK.Models.Builders;

/// <summary>
/// Fluent builder for <see cref="CreateBookRequest"/>.
/// </summary>
public sealed class CreateBookRequestBuilder
{
    private readonly List<string> _tags = [];
    private string _title = string.Empty;
    private string _author = string.Empty;
    private string? _idempotencyKey;

    private CreateBookRequestBuilder()
    {
    }

    public static CreateBookRequestBuilder Create()
    {
        return new CreateBookRequestBuilder();
    }

    public CreateBookRequestBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public CreateBookRequestBuilder WithAuthor(string author)
    {
        _author = author;
        return this;
    }

    public CreateBookRequestBuilder WithTag(string tag)
    {
        _tags.Add(tag);
        return this;
    }

    public CreateBookRequestBuilder WithIdempotencyKey(string idempotencyKey)
    {
        _idempotencyKey = idempotencyKey;
        return this;
    }

    public CreateBookRequest Build()
    {
        return new CreateBookRequest
        {
            Title = _title,
            Author = _author,
            Tags = _tags,
            IdempotencyKey = _idempotencyKey,
        };
    }
}
