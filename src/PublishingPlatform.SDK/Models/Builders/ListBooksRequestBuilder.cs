namespace PublishingPlatform.SDK.Models.Builders;

/// <summary>
/// Fluent builder for <see cref="ListBooksRequest"/>.
/// </summary>
public sealed class ListBooksRequestBuilder
{
    private readonly List<string> _tags = [];
    private readonly ListBooksRequest _request = new();

    private ListBooksRequestBuilder()
    {
    }

    public static ListBooksRequestBuilder Create()
    {
        return new ListBooksRequestBuilder();
    }

    public ListBooksRequestBuilder WithTitle(string title)
    {
        _request.Title = title;
        return this;
    }

    public ListBooksRequestBuilder WithAuthor(string author)
    {
        _request.Author = author;
        return this;
    }

    public ListBooksRequestBuilder WithTag(string tag)
    {
        _tags.Add(tag);
        return this;
    }

    public ListBooksRequestBuilder SortedByTitle()
    {
        _request.SortBy = "title";
        return this;
    }

    public ListBooksRequestBuilder Descending()
    {
        _request.Descending = true;
        return this;
    }

    public ListBooksRequestBuilder WithPage(int page, int pageSize)
    {
        _request.Page = page;
        _request.PageSize = pageSize;
        return this;
    }

    public ListBooksRequest Build()
    {
        _request.Tags = _tags;
        return _request;
    }
}
