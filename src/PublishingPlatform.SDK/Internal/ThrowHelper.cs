namespace PublishingPlatform.SDK.Internal;

internal static class ThrowHelper
{
    internal static void ThrowArgumentNull(string paramName)
    {
        throw new ArgumentNullException(paramName);
    }
}
