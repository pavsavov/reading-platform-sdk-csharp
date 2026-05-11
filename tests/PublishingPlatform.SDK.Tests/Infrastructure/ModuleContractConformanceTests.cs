using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using PublishingPlatform.SDK.Abstractions;
using PublishingPlatform.SDK.Clients;
using PublishingPlatform.SDK.Infrastructure.Transport;
using PublishingPlatform.SDK.Models;
using PublishingPlatform.SDK.Models.Common;

namespace PublishingPlatform.SDK.Tests.Infrastructure;

[Trait("Category", "ContractConformance")]
public sealed class ModuleContractConformanceTests
{
    private const string ExpectedContractShapeHash = "67cde6a14c0d25451f81f613d8baa13e2d42511506731c4303f0c9fd1298b2fb";
    private static readonly string[] BookPublishedEvents = ["book.published"];
    private static readonly string[] KindleChannels = ["kindle"];
    private static readonly Book[] SingleBookItems = [new Book { Id = "book-1", Title = "Title", Author = "Author" }];
    private static readonly BookDistributionOperation[] SingleDistributionOperations =
        [new BookDistributionOperation { OperationId = "op-1", BookId = "book-1", Status = "running" }];
    private static readonly AuditLog[] SingleAuditLogItems =
        [new AuditLog { Id = "audit-1", Action = "book.updated", Timestamp = DateTimeOffset.UtcNow }];
    private static readonly Webhook[] SingleWebhookItems =
    [
        new Webhook
        {
            Id = "wh-1",
            EndpointUrl = "https://hooks.example.test/books",
            Events = BookPublishedEvents,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
        },
    ];

    private static readonly (string Name, Type InterfaceType)[] ExpectedPublishingPlatformClientProperties =
    {
        (nameof(IPublishingPlatformClient.BookAccess), typeof(IBookAccessClient)),
        (nameof(IPublishingPlatformClient.BookAnalytics), typeof(IBookAnalyticsClient)),
        (nameof(IPublishingPlatformClient.BookAuditLogs), typeof(IBookAuditLogsClient)),
        (nameof(IPublishingPlatformClient.BookContent), typeof(IBookContentClient)),
        (nameof(IPublishingPlatformClient.BookDistribution), typeof(IBookDistributionClient)),
        (nameof(IPublishingPlatformClient.BookPublishing), typeof(IBookPublishingClient)),
        (nameof(IPublishingPlatformClient.Books), typeof(IBooksClient)),
        (nameof(IPublishingPlatformClient.Webhooks), typeof(IWebhooksClient)),
    };

    private static readonly MethodInfo[] ExpectedIdempotencyMethods =
    {
        GetRequiredMethod(typeof(IBookDistributionClient), nameof(IBookDistributionClient.RetryAsync), typeof(string), typeof(string), typeof(string), typeof(CancellationToken)),
        GetRequiredMethod(typeof(IBookDistributionClient), nameof(IBookDistributionClient.StartAsync), typeof(string), typeof(StartBookDistributionRequest), typeof(string), typeof(CancellationToken)),
        GetRequiredMethod(typeof(IBookPublishingClient), nameof(IBookPublishingClient.PublishAsync), typeof(string), typeof(PublishBookRequest), typeof(string), typeof(CancellationToken)),
        GetRequiredMethod(typeof(IBookPublishingClient), nameof(IBookPublishingClient.ScheduleAsync), typeof(string), typeof(ScheduleBookPublishingRequest), typeof(string), typeof(CancellationToken)),
        GetRequiredMethod(typeof(IBookPublishingClient), nameof(IBookPublishingClient.UnpublishAsync), typeof(string), typeof(string), typeof(CancellationToken)),
        GetRequiredMethod(typeof(IWebhooksClient), nameof(IWebhooksClient.RegisterAsync), typeof(RegisterWebhookRequest), typeof(string), typeof(CancellationToken)),
    };

    [Fact]
    public void PublishingPlatformClient_ModuleProperties_AreStable()
    {
        var actual = typeof(IPublishingPlatformClient)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(property => (property.Name, property.PropertyType))
            .OrderBy(x => x.Name, StringComparer.Ordinal)
            .ToArray();

        actual.Should().Equal(ExpectedPublishingPlatformClientProperties);
        actual.Should().HaveCount(8);
    }

    [Fact]
    public void PublishingPlatformClient_ContractShape_IsStable()
    {
        var properties = DiscoverClientProperties();
        var methods = DiscoverSubClientMethods();
        var fingerprint = BuildContractFingerprint(properties, methods);
        var actualHash = ComputeSha256Hex(fingerprint);

        actualHash.Should().Be(
            ExpectedContractShapeHash,
            $"""
            the reflected sub-client contract shape changed.
            fingerprint:
            {fingerprint}
            update ExpectedContractShapeHash to: {actualHash}
            """);

        methods.Should().HaveCount(34);
    }

    [Fact]
    public void ModuleInterfaces_Methods_EndWithOptionalCancellationToken()
    {
        foreach (var method in GetModuleInterfaceMethods())
        {
            var parameters = method.GetParameters();
            parameters.Should().NotBeEmpty();

            var cancellationToken = parameters[^1];
            cancellationToken.ParameterType.Should().Be<CancellationToken>(
                $"{method.DeclaringType!.Name}.{method.Name} should take a trailing cancellation token");
            cancellationToken.IsOptional.Should().BeTrue();
            cancellationToken.HasDefaultValue.Should().BeTrue();
        }
    }

    [Fact]
    public void ModuleInterfaces_IdempotencyKeyParameters_AreOptionalNullableAndDefaultNull()
    {
        var nullabilityInfoContext = new NullabilityInfoContext();
        var methodsWithIdempotency = DiscoverSubClientMethods()
            .Where(HasIdempotencyKeyParameterShape)
            .OrderBy(ToMethodSignatureKey, StringComparer.Ordinal)
            .ToArray();

        methodsWithIdempotency.Should().Equal(
            ExpectedIdempotencyMethods.OrderBy(ToMethodSignatureKey, StringComparer.Ordinal).ToArray(),
            "only methods with optional nullable string before CancellationToken should be idempotency-aware");

        foreach (var method in methodsWithIdempotency)
        {
            var idempotencyKeyParameter = method
                .GetParameters()[^2];

            idempotencyKeyParameter.ParameterType.Should().Be<string>();
            idempotencyKeyParameter.IsOptional.Should().BeTrue();
            idempotencyKeyParameter.HasDefaultValue.Should().BeTrue();
            idempotencyKeyParameter.DefaultValue.Should().BeNull();

            var nullabilityInfo = nullabilityInfoContext.Create(idempotencyKeyParameter);
            nullabilityInfo.WriteState.Should().Be(NullabilityState.Nullable);
        }
    }

    [Fact]
    public async Task ModuleMethods_PassExpectedDiagnosticOperationNames()
    {
        var cases = BuildDiagnosticInvocationCases();

        foreach (var diagnosticCase in cases)
        {
            var transport = CreateSuccessfulTransport();

            await diagnosticCase.InvokeAsync(transport);

            await transport.Received(1).SendAsync(
                Arg.Any<HttpMethod>(),
                Arg.Any<string>(),
                Arg.Any<HttpContent?>(),
                Arg.Any<IReadOnlyDictionary<string, string>?>(),
                diagnosticCase.ExpectedOperationName,
                Arg.Any<CancellationToken>());
        }
    }

    private static MethodInfo[] GetModuleInterfaceMethods()
    {
        return DiscoverSubClientMethods();
    }

    private static (string Name, Type InterfaceType)[] DiscoverClientProperties()
    {
        return typeof(IPublishingPlatformClient)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.PropertyType.IsInterface
                && property.PropertyType.Name.EndsWith("Client", StringComparison.Ordinal))
            .Select(property => (property.Name, property.PropertyType))
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .ToArray();
    }

    private static MethodInfo[] DiscoverSubClientMethods()
    {
        return DiscoverClientProperties()
            .SelectMany(property => property.InterfaceType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            .OrderBy(method => ToMethodSignatureKey(method), StringComparer.Ordinal)
            .ToArray();
    }

    private static string ToMethodSignatureKey(MethodInfo method)
    {
        var parameters = string.Join(", ", method.GetParameters().Select(parameter => NormalizeTypeName(parameter.ParameterType)));
        return $"{method.DeclaringType!.Name}.{method.Name}({parameters}) -> {NormalizeTypeName(method.ReturnType)}";
    }

    private static string NormalizeTypeName(Type type)
    {
        if (!type.IsGenericType)
        {
            return type.Name;
        }

        var genericName = type.Name[..type.Name.IndexOf('`')];
        var arguments = string.Join(", ", type.GetGenericArguments().Select(NormalizeTypeName));
        return $"{genericName}<{arguments}>";
    }

    private static string ComputeSha256Hex(string text)
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        var hash = System.Security.Cryptography.SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string BuildContractFingerprint(
        (string Name, Type InterfaceType)[] properties,
        MethodInfo[] methods)
    {
        var builder = new StringBuilder();

        builder.AppendLine("properties:");
        foreach (var property in properties)
        {
            builder.AppendLine($"  {property.Name}:{property.InterfaceType.FullName}");
        }

        builder.AppendLine("methods:");
        foreach (var method in methods.OrderBy(ToMethodSignatureKey, StringComparer.Ordinal))
        {
            builder.AppendLine($"  {ToMethodSignatureKey(method)}");
        }

        return builder.ToString();
    }

    private static bool HasIdempotencyKeyParameterShape(MethodInfo method)
    {
        var parameters = method.GetParameters();
        if (parameters.Length < 2)
        {
            return false;
        }

        var candidate = parameters[^2];
        var last = parameters[^1];
        return last.ParameterType == typeof(CancellationToken)
            && candidate.ParameterType == typeof(string)
            && candidate.IsOptional
            && candidate.HasDefaultValue
            && candidate.DefaultValue is null;
    }

    private static MethodInfo GetRequiredMethod(Type interfaceType, string methodName, params Type[] parameterTypes)
    {
        var method = interfaceType.GetMethod(methodName, parameterTypes);
        return method ?? throw new InvalidOperationException($"Missing method {interfaceType.Name}.{methodName}.");
    }

    private static ISharedHttpTransport CreateSuccessfulTransport()
    {
        var transport = Substitute.For<ISharedHttpTransport>();
        transport.SendAsync(
                Arg.Any<HttpMethod>(),
                Arg.Any<string>(),
                Arg.Any<HttpContent?>(),
                Arg.Any<IReadOnlyDictionary<string, string>?>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var operationName = callInfo.ArgAt<string?>(4);
                return Task.FromResult(CreateSuccessResponse(operationName));
            });

        return transport;
    }

    private static HttpResponseMessage CreateSuccessResponse(string? operationName)
    {
        return operationName switch
        {
            "Books.Create" or "Books.GetById" or "Books.UpdateMetadata" or "Books.PatchMetadata"
                => JsonResponse(new Book { Id = "book-1", Title = "Title", Author = "Author" }),
            "Books.List"
                => JsonResponse(new PagedResult<Book> { Items = SingleBookItems }),
            "Books.Delete"
                => new HttpResponseMessage(HttpStatusCode.NoContent),
            "BookContent.Get" or "BookContent.UploadOrReplace"
                => JsonResponse(new PublishingPlatform.SDK.Models.BookContent { BookId = "book-1", Format = "pdf" }),
            "BookPublishing.Publish" or "BookPublishing.Unpublish" or "BookPublishing.Schedule" or "BookPublishing.GetStatus"
                => JsonResponse(new BookPublishingStatus { BookId = "book-1", Status = "queued" }),
            "BookDistribution.Start" or "BookDistribution.GetStatus" or "BookDistribution.Retry"
                => JsonResponse(new BookDistributionOperation { OperationId = "op-1", BookId = "book-1", Status = "running" }),
            "BookDistribution.List"
                => JsonResponse(new BookDistributionListResult
                {
                    Operations = SingleDistributionOperations,
                }),
            "BookAccess.Grant"
                => JsonResponse(new BookAccessGrantResult
                {
                    GrantId = "grant-1",
                    BookId = "book-1",
                    PrincipalId = "user-1",
                    PrincipalType = "user",
                    AccessLevel = "read",
                    Created = true,
                }),
            "BookAccess.Revoke"
                => JsonResponse(new BookAccessRevokeResult
                {
                    BookId = "book-1",
                    PrincipalId = "user-1",
                    PrincipalType = "user",
                    Revoked = true,
                    RevokedAt = DateTimeOffset.UtcNow,
                }),
            "BookAccess.Check"
                => JsonResponse(new BookAccessStatus
                {
                    BookId = "book-1",
                    PrincipalId = "user-1",
                    PrincipalType = "user",
                    HasAccess = true,
                    AccessLevel = "read",
                }),
            "BookAccess.List"
                => JsonResponse(new PagedResult<BookAccessGrant>
                {
                    Items = new[]
                    {
                        new BookAccessGrant
                        {
                            GrantId = "grant-1",
                            BookId = "book-1",
                            PrincipalId = "user-1",
                            PrincipalType = "user",
                            AccessLevel = "read",
                            GrantedAt = DateTimeOffset.UtcNow,
                            GrantedBy = "system",
                        },
                    },
                }),
            "BookAnalytics.GetSummary"
                => JsonResponse(new AnalyticsSummary { TotalViews = 1, TotalDownloads = 1, ActiveReaders = 1 }),
            "BookAuditLogs.List"
                => JsonResponse(new PagedResult<AuditLog>
                {
                    Items = SingleAuditLogItems,
                }),
            "Webhooks.Register" or "Webhooks.Update"
                => JsonResponse(new Webhook
                {
                    Id = "wh-1",
                    EndpointUrl = "https://hooks.example.test/books",
                    Events = BookPublishedEvents,
                    IsActive = true,
                    CreatedAt = DateTimeOffset.UtcNow,
                }),
            "Webhooks.Delete"
                => new HttpResponseMessage(HttpStatusCode.NoContent),
            "Webhooks.List"
                => JsonResponse(new PagedResult<Webhook>
                {
                    Items = SingleWebhookItems,
                }),
            _ => throw new InvalidOperationException($"No synthetic success response configured for operation '{operationName ?? "<null>"}'."),
        };
    }

    private static HttpResponseMessage JsonResponse<T>(T payload)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(payload),
        };
    }

    private static DiagnosticInvocationCase[] BuildDiagnosticInvocationCases()
    {
        return
        [
            new(
                "IBooksClient.CreateAsync",
                "Books.Create",
                async transport => await new BooksClient(transport).CreateAsync(new CreateBookRequest { Title = "T", Author = "A" })),
            new(
                "IBooksClient.GetByIdAsync",
                "Books.GetById",
                async transport => await new BooksClient(transport).GetByIdAsync("book-1")),
            new(
                "IBooksClient.ListAsync",
                "Books.List",
                async transport => await new BooksClient(transport).ListAsync(new ListBooksRequest { SortBy = "title", PageSize = 10 })),
            new(
                "IBooksClient.ListAllAsync",
                "Books.List",
                async transport => await DrainAsync(new BooksClient(transport).ListAllAsync(new ListBooksRequest { SortBy = "title", PageSize = 10 }))),
            new(
                "IBooksClient.UpdateMetadataAsync",
                "Books.UpdateMetadata",
                async transport => await new BooksClient(transport).UpdateMetadataAsync("book-1", new UpdateBookMetadataRequest { Title = "T", Author = "A" })),
            new(
                "IBooksClient.PatchMetadataAsync",
                "Books.PatchMetadata",
                async transport => await new BooksClient(transport).PatchMetadataAsync("book-1", new UpdateBookPatchRequest { Title = "T" })),
            new(
                "IBooksClient.DeleteAsync",
                "Books.Delete",
                async transport => await new BooksClient(transport).DeleteAsync("book-1")),
            new(
                "IBookContentClient.GetAsync",
                "BookContent.Get",
                async transport => await new BookContentClient(transport).GetAsync("book-1")),
            new(
                "IBookContentClient.UploadOrReplaceAsync",
                "BookContent.UploadOrReplace",
                async transport =>
                {
                    var request = new UploadBookContentRequest
                    {
                        File = new MemoryStream(new byte[] { 1, 2, 3 }),
                        FileName = "book.pdf",
                        Format = "pdf",
                    };
                    await new BookContentClient(transport).UploadOrReplaceAsync("book-1", request);
                }),
            new(
                "IBookPublishingClient.PublishAsync",
                "BookPublishing.Publish",
                async transport => await new BookPublishingClient(transport).PublishAsync("book-1", new PublishBookRequest(), "idem-1")),
            new(
                "IBookPublishingClient.UnpublishAsync",
                "BookPublishing.Unpublish",
                async transport => await new BookPublishingClient(transport).UnpublishAsync("book-1", "idem-1")),
            new(
                "IBookPublishingClient.ScheduleAsync",
                "BookPublishing.Schedule",
                async transport => await new BookPublishingClient(transport).ScheduleAsync("book-1", new ScheduleBookPublishingRequest { ScheduledAt = DateTimeOffset.UtcNow.AddMinutes(5) }, "idem-1")),
            new(
                "IBookPublishingClient.GetStatusAsync",
                "BookPublishing.GetStatus",
                async transport => await new BookPublishingClient(transport).GetStatusAsync("book-1")),
            new(
                "IBookDistributionClient.StartAsync",
                "BookDistribution.Start",
                async transport => await new BookDistributionClient(transport).StartAsync("book-1", new StartBookDistributionRequest { Channels = KindleChannels }, "idem-1")),
            new(
                "IBookDistributionClient.GetStatusAsync",
                "BookDistribution.GetStatus",
                async transport => await new BookDistributionClient(transport).GetStatusAsync("book-1", "op-1")),
            new(
                "IBookDistributionClient.RetryAsync",
                "BookDistribution.Retry",
                async transport => await new BookDistributionClient(transport).RetryAsync("book-1", "op-1", "idem-1")),
            new(
                "IBookDistributionClient.ListAsync",
                "BookDistribution.List",
                async transport => await new BookDistributionClient(transport).ListAsync("book-1")),
            new(
                "IBookAccessClient.GrantAsync",
                "BookAccess.Grant",
                async transport => await new BookAccessClient(transport).GrantAsync(new BookAccessGrantRequest
                {
                    BookId = "book-1",
                    PrincipalId = "user-1",
                    PrincipalType = "user",
                    AccessLevel = "read",
                })),
            new(
                "IBookAccessClient.RevokeAsync",
                "BookAccess.Revoke",
                async transport => await new BookAccessClient(transport).RevokeAsync(new BookAccessRevokeRequest
                {
                    BookId = "book-1",
                    PrincipalId = "user-1",
                    PrincipalType = "user",
                })),
            new(
                "IBookAccessClient.CheckAsync",
                "BookAccess.Check",
                async transport => await new BookAccessClient(transport).CheckAsync(new BookAccessCheckRequest
                {
                    BookId = "book-1",
                    PrincipalId = "user-1",
                    PrincipalType = "user",
                })),
            new(
                "IBookAccessClient.ListAsync",
                "BookAccess.List",
                async transport => await new BookAccessClient(transport).ListAsync(new ListBookAccessRequest { PageSize = 10 })),
            new(
                "IBookAccessClient.ListAllAsync",
                "BookAccess.List",
                async transport => await DrainAsync(new BookAccessClient(transport).ListAllAsync(new ListBookAccessRequest { PageSize = 10 }))),
            new(
                "IBookAnalyticsClient.GetSummaryAsync",
                "BookAnalytics.GetSummary",
                async transport => await new BookAnalyticsClient(transport).GetSummaryAsync(new GetBookAnalyticsRequest
                {
                    BookId = "book-1",
                    From = DateTimeOffset.UtcNow.AddDays(-1),
                    To = DateTimeOffset.UtcNow,
                })),
            new(
                "IBookAuditLogsClient.ListAsync",
                "BookAuditLogs.List",
                async transport => await new BookAuditLogsClient(transport).ListAsync(new ListBookAuditLogsRequest { Page = 0, PageSize = 10 })),
            new(
                "IBookAuditLogsClient.ListAllAsync",
                "BookAuditLogs.List",
                async transport => await DrainAsync(new BookAuditLogsClient(transport).ListAllAsync(new ListBookAuditLogsRequest { Page = 0, PageSize = 10 }))),
            new(
                "IWebhooksClient.RegisterAsync",
                "Webhooks.Register",
                async transport => await new WebhooksClient(transport).RegisterAsync(new RegisterWebhookRequest
                {
                    EndpointUrl = "https://hooks.example.test/books",
                    Events = BookPublishedEvents,
                }, "idem-1")),
            new(
                "IWebhooksClient.UpdateAsync",
                "Webhooks.Update",
                async transport => await new WebhooksClient(transport).UpdateAsync(new UpdateWebhookRequest
                {
                    WebhookId = "wh-1",
                    EndpointUrl = "https://hooks.example.test/books",
                    Events = BookPublishedEvents,
                    IsActive = true,
                })),
            new(
                "IWebhooksClient.DeleteAsync",
                "Webhooks.Delete",
                async transport => await new WebhooksClient(transport).DeleteAsync("wh-1")),
            new(
                "IWebhooksClient.ListAsync",
                "Webhooks.List",
                async transport => await new WebhooksClient(transport).ListAsync(new ListWebhooksRequest { PageSize = 10 })),
            new(
                "IWebhooksClient.ListAllAsync",
                "Webhooks.List",
                async transport => await DrainAsync(new WebhooksClient(transport).ListAllAsync(new ListWebhooksRequest { PageSize = 10 }))),
        ];
    }

    private static async Task DrainAsync<T>(IAsyncEnumerable<T> source)
    {
        await foreach (var _ in source)
        {
        }
    }

    private sealed record DiagnosticInvocationCase(
        string ScenarioName,
        string ExpectedOperationName,
        Func<ISharedHttpTransport, Task> InvokeAsync);
}
