namespace PublishingPlatform.SDK.Tests.Infrastructure;

public sealed class DocumentationTradeoffsContractTests
{
    private static readonly string[] RequiredTradeoffPhrases =
    {
        "depth over breadth",
        "central internal transport",
        "public models are strict",
        "retry defaults are conservative",
        "diagnostics are privacy-preserving",
        "errors are normalized",
    };

    private static readonly string[] RequiredOpenApiTradeoffKeys =
    {
        "depthOverBreadth",
        "centralTransport",
        "typedModelStrictness",
        "retryAndIdempotency",
        "diagnosticsPrivacy",
        "errorNormalization",
    };

    [Fact]
    public void Readme_IncludesRequiredArchitectureTradeoffs()
    {
        var readme = ReadRepositoryFile("README.md");
        var normalizedReadme = readme.ToLowerInvariant();

        readme.Should().Contain("## Architecture tradeoffs");

        foreach (var phrase in RequiredTradeoffPhrases)
        {
            normalizedReadme.Should().Contain(phrase);
        }
    }

    [Fact]
    public void ArchitectureDocs_IncludeRequiredTradeoffSections()
    {
        var architecture = ReadRepositoryFile(@"docs\architecture.md");

        foreach (var heading in new[]
        {
            "### Depth Over Breadth",
            "### Central Transport Over Per-Client Flexibility",
            "### Typed Model Strictness",
            "### Retry Defaults And Idempotency",
            "### Diagnostics Privacy Defaults",
            "### Error Normalization",
        })
        {
            architecture.Should().Contain(heading);
        }
    }

    [Fact]
    public void OpenApiSpec_DeclaresSdkArchitectureTradeoffExtension()
    {
        var spec = ReadRepositoryFile(@"docs\openapi-google-books-derived-sdk-contract.yaml");

        spec.Should().Contain("x-sdk-architecture-tradeoffs:");

        foreach (var key in RequiredOpenApiTradeoffKeys)
        {
            spec.Should().Contain($"{key}:");
        }
    }

    [Fact]
    public void ArchitectureTradeoffsExample_IsLinkedFromReadme()
    {
        var readme = ReadRepositoryFile("README.md");
        var example = ReadRepositoryFile(@"examples\ArchitectureTradeoffs\ArchitectureTradeoffsExample.csx");

        readme.Should().Contain("examples/ArchitectureTradeoffs/ArchitectureTradeoffsExample.csx");
        example.Should().Contain("RetryNonIdempotentMethods = false");
        example.Should().Contain("idempotencyKey:");
        example.Should().Contain("EnableLogging = false");
    }

    private static string ReadRepositoryFile(string relativePath)
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return File.ReadAllText(candidate);
            }

            current = current.Parent;
        }

        throw new FileNotFoundException($"Could not locate repository file '{relativePath}'.");
    }
}
