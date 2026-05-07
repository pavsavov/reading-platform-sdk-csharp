using System.Text;

namespace PublishingPlatform.SDK.Clients.Common.Serialization;

/// <summary>
/// Provides shared URL-encoded query-string append helpers for SDK client builders.
/// </summary>
internal static class QueryStringBuilderHelper
{
    /// <summary>
    /// Appends a required query parameter.
    /// </summary>
    /// <param name="builder">The destination query-string builder.</param>
    /// <param name="key">The query parameter name.</param>
    /// <param name="value">The query parameter value.</param>
    /// <param name="isFirstParameter">Whether this is the first query parameter in the string.</param>
    public static void AppendRequired(StringBuilder builder, string key, string value, bool isFirstParameter)
    {
        AppendSeparator(builder, isFirstParameter);
        AppendEncoded(builder, key, value);
    }

    /// <summary>
    /// Appends an optional query parameter when a non-empty value is provided.
    /// </summary>
    /// <param name="builder">The destination query-string builder.</param>
    /// <param name="key">The query parameter name.</param>
    /// <param name="value">The query parameter value.</param>
    /// <param name="isFirstParameter">Whether this is the first query parameter in the string.</param>
    /// <returns><see langword="true"/> when the value was appended; otherwise <see langword="false"/>.</returns>
    public static bool AppendOptional(StringBuilder builder, string key, string? value, bool isFirstParameter)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        AppendSeparator(builder, isFirstParameter);
        AppendEncoded(builder, key, value);
        return true;
    }

    private static void AppendSeparator(StringBuilder builder, bool isFirstParameter)
    {
        builder.Append(isFirstParameter ? '?' : '&');
    }

    private static void AppendEncoded(StringBuilder builder, string key, string value)
    {
        builder.Append(Uri.EscapeDataString(key));
        builder.Append('=');
        builder.Append(Uri.EscapeDataString(value));
    }
}
