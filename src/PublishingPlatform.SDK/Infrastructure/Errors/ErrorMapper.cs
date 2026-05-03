namespace PublishingPlatform.SDK.Infrastructure.Errors;

public static class ErrorMapper
{
    public static ApiException Map(int statusCode, string message)
    {
        return new ApiException(statusCode, message);
    }
}
