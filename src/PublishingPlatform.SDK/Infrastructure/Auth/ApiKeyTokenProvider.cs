namespace PublishingPlatform.SDK.Infrastructure.Auth;

public sealed class ApiKeyTokenProvider : ITokenProvider
{
    private readonly string _apiKey;

    public ApiKeyTokenProvider(string apiKey)
    {
        _apiKey = apiKey;
    }

    public Task<string> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_apiKey);
    }
}
