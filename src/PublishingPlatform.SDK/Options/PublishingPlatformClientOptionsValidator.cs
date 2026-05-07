using PublishingPlatform.SDK.Exceptions;

namespace PublishingPlatform.SDK.Options;

/// <summary>
/// Validates SDK client configuration before authenticated HTTP transport is created.
/// </summary>
internal static class PublishingPlatformClientOptionsValidator
{
    /// <summary>
    /// Validates the supplied SDK client options.
    /// </summary>
    /// <param name="options">The options to validate.</param>
    /// <returns><see langword="true"/> when validation succeeds.</returns>
    /// <exception cref="PublishingPlatformConfigurationException">Thrown when required client configuration is invalid.</exception>
    internal static bool Validate(PublishingPlatformClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var baseUri))
        {
            throw new PublishingPlatformConfigurationException("BaseUrl must be a valid absolute URL.");
        }

        if (baseUri.Scheme != Uri.UriSchemeHttps)
        {
            throw new PublishingPlatformConfigurationException("BaseUrl must use HTTPS.");
        }

        if (string.IsNullOrWhiteSpace(options.ApiKey))
        {
            throw new PublishingPlatformConfigurationException("ApiKey must not be empty.");
        }

        if (options.Timeout <= TimeSpan.Zero)
        {
            throw new PublishingPlatformConfigurationException("Timeout must be greater than zero.");
        }

        if (options.Diagnostics is not null
            && string.IsNullOrWhiteSpace(options.Diagnostics.CorrelationHeaderName))
        {
            throw new PublishingPlatformConfigurationException("Diagnostics correlation header name must not be empty.");
        }

        return true;
    }
}
