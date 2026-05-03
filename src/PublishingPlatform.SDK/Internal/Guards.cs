namespace PublishingPlatform.SDK.Internal;

internal static class Guards
{
    internal static void NotNull<T>(T value, string paramName)
        where T : class
    {
        if (value is null)
        {
            ThrowHelper.ThrowArgumentNull(paramName);
        }
    }
}
