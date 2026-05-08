using MockPublishingPlatform.Api.Models;
using MockPublishingPlatform.Api.Services;

namespace MockPublishingPlatform.Api.Endpoints;

/// <summary>
/// Creates common endpoint results backed by fixture-defined error payloads.
/// </summary>
internal static class MockApiResultFactory
{
    internal static IResult CreateValidationError(HttpRequest request, MockApiState state)
    {
        return Results.BadRequest(ApiErrorResponse.FromFixture(state.ApiErrors[MockApiConstants.ValidationErrorKey], request.HttpContext.TraceIdentifier));
    }

    internal static IResult CreateNotFound(HttpRequest request, MockApiState state)
    {
        return Results.NotFound(ApiErrorResponse.FromFixture(state.ApiErrors[MockApiConstants.NotFoundErrorKey], request.HttpContext.TraceIdentifier));
    }
}
