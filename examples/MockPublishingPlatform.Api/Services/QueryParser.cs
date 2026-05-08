namespace MockPublishingPlatform.Api.Services;

/// <summary>
/// Provides safe parsing helpers for query parameter values.
/// </summary>
public static class QueryParser
{
    /// <summary>
    /// Parses an integer constrained to an inclusive range.
    /// </summary>
    public static int ParsePositiveInt(string? rawValue, int defaultValue, int min, int max)
    {
        if (!int.TryParse(rawValue, out var parsed))
        {
            return defaultValue;
        }

        if (parsed < min)
        {
            return min;
        }

        if (parsed > max)
        {
            return max;
        }

        return parsed;
    }

    /// <summary>
    /// Parses a non-negative integer.
    /// </summary>
    public static int ParseNonNegativeInt(string? rawValue, int defaultValue)
    {
        if (!int.TryParse(rawValue, out var parsed))
        {
            return defaultValue;
        }

        return parsed < 0 ? defaultValue : parsed;
    }

    /// <summary>
    /// Parses a boolean with fallback default.
    /// </summary>
    public static bool ParseBoolean(string? rawValue, bool defaultValue)
    {
        return bool.TryParse(rawValue, out var parsed) ? parsed : defaultValue;
    }

    /// <summary>
    /// Parses a nullable boolean.
    /// </summary>
    public static bool? ParseNullableBoolean(string? rawValue)
    {
        return bool.TryParse(rawValue, out var parsed) ? parsed : null;
    }

    /// <summary>
    /// Parses continuation token in format offset:{number}.
    /// </summary>
    public static int ParseContinuationOffset(string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return 0;
        }

        var parts = rawValue.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 2 && parts[0].Equals("offset", StringComparison.OrdinalIgnoreCase)
            && int.TryParse(parts[1], out var parsed) && parsed >= 0)
        {
            return parsed;
        }

        return 0;
    }
}
