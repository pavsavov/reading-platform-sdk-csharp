using Bogus;
using PublishingPlatform.SDK.Clients;
using PublishingPlatform.SDK.Exceptions;

namespace PublishingPlatform.SDK.Tests.Infrastructure;

public sealed class PublishingPlatformClientBuilderConvenienceOverloadTests
{
    [Fact]
    public void Create_WithValidBaseUrlAndApiKey_BuildsClient()
    {
        Randomizer.Seed = new Random(110);
        var faker = new Faker();
        var host = faker.Internet.DomainName();
        var apiKey = faker.Random.AlphaNumeric(32);

        var client = PublishingPlatformClientBuilder
            .Create($"https://{host}", apiKey)
            .Build();

        client.Should().NotBeNull();
    }

    [Fact]
    public void Create_WithWhitespaceBaseUrl_ThrowsArgumentException()
    {
        var act = () => PublishingPlatformClientBuilder.Create(" ", "valid-key");

        var exception = act.Should().Throw<ArgumentException>().Which;
        exception.ParamName.Should().Be("baseUrl");
    }

    [Fact]
    public void Create_WithWhitespaceApiKey_ThrowsArgumentException()
    {
        var act = () => PublishingPlatformClientBuilder.Create("https://api.books.example", " ");

        var exception = act.Should().Throw<ArgumentException>().Which;
        exception.ParamName.Should().Be("apiKey");
    }

    [Fact]
    public void Create_WithHttpBaseUrl_ThrowsPublishingPlatformConfigurationExceptionOnBuild()
    {
        var act = () => PublishingPlatformClientBuilder
            .Create("http://api.books.example", "valid-key")
            .Build();

        var exception = act.Should().Throw<PublishingPlatformConfigurationException>().Which;
        exception.Message.Should().Be("BaseUrl must use HTTPS.");
    }
}
