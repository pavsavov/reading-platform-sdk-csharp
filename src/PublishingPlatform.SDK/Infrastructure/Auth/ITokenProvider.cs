namespace PublishingPlatform.SDK.Infrastructure.Auth;

public interface ITokenProvider
{
    Task<string> GetTokenAsync(CancellationToken cancellationToken = default);
}
