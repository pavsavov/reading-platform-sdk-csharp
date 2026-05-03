using PublishingPlatform.SDK.Infrastructure.Auth;

namespace PublishingPlatform.SDK.Infrastructure.Http.Handlers;

public sealed class AuthHandler : DelegatingHandler
{
    private readonly ITokenProvider _tokenProvider;

    public AuthHandler(ITokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _tokenProvider.GetTokenAsync(cancellationToken).ConfigureAwait(false);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
