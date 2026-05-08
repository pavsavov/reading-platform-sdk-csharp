using MockPublishingPlatform.Api.Endpoints;
using MockPublishingPlatform.Api.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton(FixtureLoader.Load(Path.Combine(builder.Environment.ContentRootPath, "Fixtures")));

var app = builder.Build();

app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers.TryGetValue(MockApiConstants.CorrelationHeaderName, out var existing)
        && !string.IsNullOrWhiteSpace(existing)
        ? existing.ToString()
        : $"mock-{Guid.NewGuid():N}";

    context.Response.Headers[MockApiConstants.CorrelationHeaderName] = correlationId;
    await next().ConfigureAwait(false);
});

BooksEndpointsModule.Map(app);
BookContentEndpointsModule.Map(app);
BookPublishingEndpointsModule.Map(app);
BookDistributionEndpointsModule.Map(app);
WebhooksEndpointsModule.Map(app);

app.Run();
