using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Reflection;
using PublishingPlatform.SDK.Abstractions;
using PublishingPlatform.SDK.Models;
using PublishingPlatform.SDK.Models.Common;

namespace PublishingPlatform.SDK.Tests.Infrastructure;

public sealed class OpenApiSpecificationContractsTests
{
    private const string SpecRelativePath = @"docs\openapi-google-books-derived-sdk-contract.yaml";
    private const string ExpectedSha256 = "46f2aefcf0f53a2778616687411d72222dd473b2ea3841a100412c4d908cbfa4";

    private static readonly string[] ExpectedOperations =
    {
        "DELETE /books/{bookId}",
        "DELETE /webhooks/{webhookId}",
        "GET /book-access",
        "GET /book-analytics/summary",
        "GET /book-audit-logs",
        "GET /books",
        "GET /books/{bookId}",
        "GET /books/{bookId}/content/uploads/{uploadSessionId}",
        "GET /books/{bookId}/access",
        "GET /books/{bookId}/access/check",
        "GET /books/{bookId}/content",
        "GET /books/{bookId}/distribution",
        "GET /books/{bookId}/distribution/{operationId}/status",
        "GET /books/{bookId}/publishing/status",
        "GET /webhooks",
        "PATCH /books/{bookId}",
        "PATCH /webhooks/{webhookId}",
        "POST /books",
        "POST /books/{bookId}/access",
        "POST /books/{bookId}/access/revoke",
        "POST /books/{bookId}/content/uploads",
        "POST /books/{bookId}/content/uploads/{uploadSessionId}/complete",
        "POST /books/{bookId}/distribution/{operationId}/retry",
        "POST /books/{bookId}/distribution/start",
        "POST /books/{bookId}/publishing/publish",
        "POST /books/{bookId}/publishing/schedule",
        "POST /books/{bookId}/publishing/unpublish",
        "POST /webhooks",
        "PUT /books/{bookId}",
        "PUT /books/{bookId}/content/uploads/{uploadSessionId}/chunks",
        "PUT /books/{bookId}/content",
    };

    private static readonly string[] OutOfScopePaths =
    {
        "/books/import",
        "/book-discovery/search",
        "/book-assets",
        "/book-distributions",
        "/book-distributions/{distributionId}",
        "/books/{bookId}/access/status",
    };

    private static readonly InterfaceMethodContract[] ExpectedSdkMethodContracts =
    {
        new(typeof(IBooksClient), nameof(IBooksClient.CreateAsync), ["books_create"]),
        new(typeof(IBooksClient), nameof(IBooksClient.GetByIdAsync), ["books_getById"]),
        new(typeof(IBooksClient), nameof(IBooksClient.ListAsync), ["books_list"]),
        new(typeof(IBooksClient), nameof(IBooksClient.UpdateMetadataAsync), ["books_updateMetadata"]),
        new(typeof(IBooksClient), nameof(IBooksClient.PatchMetadataAsync), ["books_patchMetadata"]),
        new(typeof(IBooksClient), nameof(IBooksClient.DeleteAsync), ["books_delete"]),
        new(typeof(IBookContentClient), nameof(IBookContentClient.GetAsync), ["bookContent_get"]),
        new(typeof(IBookContentClient), nameof(IBookContentClient.UploadOrReplaceAsync), ["bookContent_uploadOrReplace"]),
        new(typeof(IBookContentClient), nameof(IBookContentClient.StartResumableUploadAsync), ["bookContent_startResumableUpload"]),
        new(typeof(IBookContentClient), nameof(IBookContentClient.UploadChunkAsync), ["bookContent_uploadChunk"]),
        new(typeof(IBookContentClient), nameof(IBookContentClient.GetUploadSessionAsync), ["bookContent_getUploadSession"]),
        new(typeof(IBookContentClient), nameof(IBookContentClient.CompleteResumableUploadAsync), ["bookContent_completeResumableUpload"]),
        new(typeof(IBookPublishingClient), nameof(IBookPublishingClient.PublishAsync), ["bookPublishing_publish"]),
        new(typeof(IBookPublishingClient), nameof(IBookPublishingClient.UnpublishAsync), ["bookPublishing_unpublish"]),
        new(typeof(IBookPublishingClient), nameof(IBookPublishingClient.ScheduleAsync), ["bookPublishing_schedule"]),
        new(typeof(IBookPublishingClient), nameof(IBookPublishingClient.GetStatusAsync), ["bookPublishing_getStatus"]),
        new(typeof(IBookDistributionClient), nameof(IBookDistributionClient.StartAsync), ["bookDistribution_start"]),
        new(typeof(IBookDistributionClient), nameof(IBookDistributionClient.GetStatusAsync), ["bookDistribution_getStatus"]),
        new(typeof(IBookDistributionClient), nameof(IBookDistributionClient.RetryAsync), ["bookDistribution_retry"]),
        new(typeof(IBookDistributionClient), nameof(IBookDistributionClient.ListAsync), ["bookDistribution_list"]),
        new(typeof(IBookAccessClient), nameof(IBookAccessClient.GrantAsync), ["bookAccess_grant"]),
        new(typeof(IBookAccessClient), nameof(IBookAccessClient.RevokeAsync), ["bookAccess_revoke"]),
        new(typeof(IBookAccessClient), nameof(IBookAccessClient.CheckAsync), ["bookAccess_check"]),
        new(typeof(IBookAccessClient), nameof(IBookAccessClient.ListAsync), ["bookAccess_listForBook", "bookAccess_listGlobal"]),
        new(typeof(IBookAnalyticsClient), nameof(IBookAnalyticsClient.GetSummaryAsync), ["bookAnalytics_getSummary"]),
        new(typeof(IBookAuditLogsClient), nameof(IBookAuditLogsClient.ListAsync), ["bookAuditLogs_list"]),
        new(typeof(IWebhooksClient), nameof(IWebhooksClient.RegisterAsync), ["webhooks_register"]),
        new(typeof(IWebhooksClient), nameof(IWebhooksClient.UpdateAsync), ["webhooks_update"]),
        new(typeof(IWebhooksClient), nameof(IWebhooksClient.DeleteAsync), ["webhooks_delete"]),
        new(typeof(IWebhooksClient), nameof(IWebhooksClient.ListAsync), ["webhooks_list"]),
    };

    [Fact]
    public void OpenApiSpec_FileHash_RemainsImmutable()
    {
        var specPath = ResolveSpecPath();
        var bytes = File.ReadAllBytes(specPath);
        var hashBytes = SHA256.HashData(bytes);
        var actualHash = Convert.ToHexString(hashBytes).ToLowerInvariant();

        actualHash.Should().Be(
            ExpectedSha256,
            "the OpenAPI contract is treated as immutable and any change should require deliberate test baseline updates");
    }

    [Fact]
    public void OpenApiSpec_OperationMatrix_MatchesExpectedContract()
    {
        var specPath = ResolveSpecPath();
        var text = File.ReadAllText(specPath);
        var operations = ParseOperations(text);
        var expectedOperations = ExpectedOperations.OrderBy(x => x, StringComparer.Ordinal).ToArray();

        operations.Should().Equal(expectedOperations);
        operations.Should().HaveCount(31);
    }

    [Fact]
    public void OpenApiSpec_DoesNotContain_OutOfScopePaths()
    {
        var specPath = ResolveSpecPath();
        var text = File.ReadAllText(specPath);

        foreach (var path in OutOfScopePaths)
        {
            text.Should().NotContain(
                $"\n  {path}:",
                $"path '{path}' is out of scope and must stay removed from the implemented contract");
        }
    }

    [Fact]
    public void OpenApiSpec_AnalyticsQuery_DoesNotExposeFutureFacingFields()
    {
        var specPath = ResolveSpecPath();
        var text = File.ReadAllText(specPath);

        text.Should().NotContain("name: granularity");
        text.Should().NotContain("name: includeUniqueReaders");
    }

    [Fact]
    public void OpenApiSpec_TaskBasedSdkMethods_AreFullyMappedToOperationIds_ViaReflection()
    {
        var sdkTaskMethodKeys = GetSdkTaskMethods()
            .Select(BuildMethodKey)
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToArray();

        var expectedMethodKeys = ExpectedSdkMethodContracts
            .Select(ResolveMethod)
            .Select(BuildMethodKey)
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToArray();

        sdkTaskMethodKeys.Should().Equal(
            expectedMethodKeys,
            "all public task-based SDK client methods must be intentionally mapped to OpenAPI operations");

        var operationsById = ParseOperationContracts(File.ReadAllText(ResolveSpecPath()))
            .Where(op => !string.IsNullOrWhiteSpace(op.OperationId))
            .ToDictionary(op => op.OperationId!, StringComparer.Ordinal);

        var expectedOperationIds = ExpectedSdkMethodContracts
            .SelectMany(c => c.OperationIds)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToArray();

        operationsById.Keys.OrderBy(x => x, StringComparer.Ordinal).Should().Equal(
            expectedOperationIds,
            "every implemented OpenAPI operation should be represented by an SDK task-based client method mapping");
    }

    [Fact]
    public void OpenApiSpec_RequestAndResponseSchemas_MatchSdkMethodSignatures_ViaReflection()
    {
        var operationsById = ParseOperationContracts(File.ReadAllText(ResolveSpecPath()))
            .Where(op => !string.IsNullOrWhiteSpace(op.OperationId))
            .ToDictionary(op => op.OperationId!, StringComparer.Ordinal);

        foreach (var contract in ExpectedSdkMethodContracts)
        {
            var method = ResolveMethod(contract);
            var expectedRequestSchema = ResolveExpectedRequestSchema(method);
            var expectedResponseSchema = ResolveExpectedResponseSchema(method.ReturnType);

            foreach (var operationId in contract.OperationIds)
            {
                operationsById.Should().ContainKey(operationId);
                var operation = operationsById[operationId];

                if (OperationSupportsRequestBody(operation.HttpMethod))
                {
                    operation.RequestSchema.Should().Be(
                        expectedRequestSchema,
                        $"operation '{operationId}' should carry the request DTO used by {BuildMethodKey(method)}");
                }
                else
                {
                    operation.RequestSchema.Should().BeNull(
                        $"operation '{operationId}' should not define a request body");
                }

                if (expectedResponseSchema is null)
                {
                    operation.SuccessResponseSchemas.Should().BeEmpty(
                        $"operation '{operationId}' maps to non-generic Task and should not return a response schema payload");
                }
                else
                {
                    operation.SuccessResponseSchemas.Should().ContainSingle().Which.Should().Be(
                        expectedResponseSchema,
                        $"operation '{operationId}' should return the DTO exposed by {BuildMethodKey(method)}");
                }
            }
        }
    }

    private static string ResolveSpecPath()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, SpecRelativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            current = current.Parent;
        }

        throw new FileNotFoundException($"Could not locate OpenAPI spec at '{SpecRelativePath}' from '{AppContext.BaseDirectory}'.");
    }

    private static string[] ParseOperations(string specText)
    {
        return ParseOperationContracts(specText)
            .Select(op => $"{op.HttpMethod} {op.Path}")
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToArray();
    }

    private static OpenApiOperationContract[] ParseOperationContracts(string specText)
    {
        var pathPattern = new Regex(@"^\s{2}(/[^:]+):\s*$", RegexOptions.Compiled);
        var operationPattern = new Regex(@"^\s{4}(get|post|put|patch|delete):\s*$", RegexOptions.Compiled);
        var operationIdPattern = new Regex(@"^\s{6}operationId:\s*(\S+)\s*$", RegexOptions.Compiled);
        var sectionPattern = new Regex(@"^\s{6}([A-Za-z][A-Za-z0-9_-]*):\s*$", RegexOptions.Compiled);
        var statusCodePattern = new Regex(@"^\s{8}'(\d{3})':\s*$", RegexOptions.Compiled);
        var schemaRefPattern = new Regex(@"^\s{14,}\$ref:\s*'#/components/schemas/([^']+)'", RegexOptions.Compiled);

        var lines = specText.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        var currentPath = string.Empty;
        var currentOperation = default(OpenApiOperationContract);
        var operations = new List<OpenApiOperationContract>();
        var inRequestBody = false;
        var inResponses = false;
        var currentStatusCode = default(string);

        foreach (var line in lines)
        {
            var pathMatch = pathPattern.Match(line);
            if (pathMatch.Success)
            {
                currentPath = pathMatch.Groups[1].Value;
                currentOperation = null;
                inRequestBody = false;
                inResponses = false;
                currentStatusCode = null;
                continue;
            }

            var operationMatch = operationPattern.Match(line);
            if (operationMatch.Success && !string.IsNullOrWhiteSpace(currentPath))
            {
                currentOperation = new OpenApiOperationContract
                {
                    HttpMethod = operationMatch.Groups[1].Value.ToUpperInvariant(),
                    Path = currentPath,
                };

                operations.Add(currentOperation);
                inRequestBody = false;
                inResponses = false;
                currentStatusCode = null;
                continue;
            }

            if (currentOperation is null)
            {
                continue;
            }

            var sectionMatch = sectionPattern.Match(line);
            if (sectionMatch.Success)
            {
                var section = sectionMatch.Groups[1].Value;
                if (!section.Equals("requestBody", StringComparison.Ordinal) && !section.Equals("responses", StringComparison.Ordinal))
                {
                    inRequestBody = false;

                    if (!section.Equals("responses", StringComparison.Ordinal))
                    {
                        inResponses = false;
                        currentStatusCode = null;
                    }
                }
            }

            var operationIdMatch = operationIdPattern.Match(line);
            if (operationIdMatch.Success)
            {
                currentOperation.OperationId = operationIdMatch.Groups[1].Value;
                continue;
            }

            if (line.Equals("      requestBody:", StringComparison.Ordinal))
            {
                inRequestBody = true;
                inResponses = false;
                currentStatusCode = null;
                continue;
            }

            if (line.Equals("      responses:", StringComparison.Ordinal))
            {
                inResponses = true;
                inRequestBody = false;
                currentStatusCode = null;
                continue;
            }

            if (inResponses)
            {
                var statusCodeMatch = statusCodePattern.Match(line);
                if (statusCodeMatch.Success)
                {
                    currentStatusCode = statusCodeMatch.Groups[1].Value;
                    continue;
                }
            }

            var schemaRefMatch = schemaRefPattern.Match(line);
            if (!schemaRefMatch.Success)
            {
                continue;
            }

            var schemaName = schemaRefMatch.Groups[1].Value;
            if (inRequestBody)
            {
                currentOperation.RequestSchema ??= schemaName;
                continue;
            }

            if (inResponses && currentStatusCode is not null && currentStatusCode.StartsWith("2", StringComparison.Ordinal))
            {
                currentOperation.SuccessResponseSchemas.Add(schemaName);
            }
        }

        return operations.ToArray();
    }

    private static MethodInfo[] GetSdkTaskMethods()
    {
        return typeof(IPublishingPlatformClient)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(property => property.PropertyType)
            .Distinct()
            .SelectMany(interfaceType => interfaceType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            .Where(IsTaskBasedMethod)
            .OrderBy(BuildMethodKey, StringComparer.Ordinal)
            .ToArray();
    }

    private static MethodInfo ResolveMethod(InterfaceMethodContract contract)
    {
        return contract.InterfaceType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Single(method => method.Name == contract.MethodName && IsTaskBasedMethod(method));
    }

    private static bool IsTaskBasedMethod(MethodInfo method)
    {
        var returnType = method.ReturnType;
        return returnType == typeof(Task)
            || (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>));
    }

    private static string BuildMethodKey(MethodInfo method)
    {
        var parameterSignature = string.Join(
            ", ",
            method.GetParameters().Select(parameter => NormalizeTypeName(parameter.ParameterType)));

        return $"{method.DeclaringType!.Name}.{method.Name}({parameterSignature}) -> {NormalizeTypeName(method.ReturnType)}";
    }

    private static string NormalizeTypeName(Type type)
    {
        if (!type.IsGenericType)
        {
            return type.Name;
        }

        var name = type.Name[..type.Name.IndexOf('`')];
        var arguments = string.Join(", ", type.GetGenericArguments().Select(NormalizeTypeName));
        return $"{name}<{arguments}>";
    }

    private static string? ResolveExpectedRequestSchema(MethodInfo method)
    {
        var requestBodyParameter = method.GetParameters()
            .FirstOrDefault(parameter => IsRequestBodyPayloadType(parameter.ParameterType));

        return requestBodyParameter?.ParameterType.Name;
    }

    private static bool IsRequestBodyPayloadType(Type type)
    {
        return type != typeof(string)
            && type != typeof(CancellationToken)
            && !type.IsPrimitive;
    }

    private static bool OperationSupportsRequestBody(string httpMethod)
    {
        return httpMethod is "POST" or "PUT" or "PATCH";
    }

    private static string? ResolveExpectedResponseSchema(Type methodReturnType)
    {
        if (methodReturnType == typeof(Task))
        {
            return null;
        }

        if (!methodReturnType.IsGenericType || methodReturnType.GetGenericTypeDefinition() != typeof(Task<>))
        {
            throw new InvalidOperationException($"Unsupported async return type '{methodReturnType}'.");
        }

        var taskResultType = methodReturnType.GetGenericArguments()[0];

        if (taskResultType.IsGenericType && taskResultType.GetGenericTypeDefinition() == typeof(PagedResult<>))
        {
            var itemType = taskResultType.GetGenericArguments()[0];
            if (itemType == typeof(Book))
            {
                return "PagedBookResult";
            }

            if (itemType == typeof(BookAccessGrant))
            {
                return "PagedBookAccessGrantResult";
            }

            if (itemType == typeof(AuditLog))
            {
                return "PagedAuditLogResult";
            }

            if (itemType == typeof(Webhook))
            {
                return "PagedWebhookResult";
            }

            throw new InvalidOperationException($"PagedResult<{itemType.Name}> is not mapped to an OpenAPI schema.");
        }

        return taskResultType.Name;
    }

    private sealed class OpenApiOperationContract
    {
        public required string HttpMethod { get; init; }

        public required string Path { get; init; }

        public string? OperationId { get; set; }

        public string? RequestSchema { get; set; }

        public HashSet<string> SuccessResponseSchemas { get; } = [];
    }

    private sealed record InterfaceMethodContract(
        Type InterfaceType,
        string MethodName,
        string[] OperationIds);
}
