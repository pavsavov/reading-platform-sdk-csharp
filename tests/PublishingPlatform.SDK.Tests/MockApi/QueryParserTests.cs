using MockPublishingPlatform.Api.Services;

namespace PublishingPlatform.SDK.Tests.MockApi;

public sealed class QueryParserTests
{
    [Theory]
    [InlineData(null, 10, 1, 100, 10)]
    [InlineData("oops", 10, 1, 100, 10)]
    [InlineData("-1", 10, 1, 100, 1)]
    [InlineData("101", 10, 1, 100, 100)]
    [InlineData("42", 10, 1, 100, 42)]
    public void ParsePositiveInt_ClampsAndFallsBack(string? raw, int fallback, int min, int max, int expected)
    {
        QueryParser.ParsePositiveInt(raw, fallback, min, max).Should().Be(expected);
    }

    [Theory]
    [InlineData(null, 5, 5)]
    [InlineData("oops", 5, 5)]
    [InlineData("-2", 5, 5)]
    [InlineData("0", 5, 0)]
    [InlineData("9", 5, 9)]
    public void ParseNonNegativeInt_UsesDefaultForInvalidOrNegative(string? raw, int fallback, int expected)
    {
        QueryParser.ParseNonNegativeInt(raw, fallback).Should().Be(expected);
    }

    [Theory]
    [InlineData("true", false, true)]
    [InlineData("false", true, false)]
    [InlineData("oops", true, true)]
    public void ParseBoolean_UsesFallbackWhenInvalid(string? raw, bool fallback, bool expected)
    {
        QueryParser.ParseBoolean(raw, fallback).Should().Be(expected);
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    [InlineData("oops", null)]
    [InlineData(null, null)]
    public void ParseNullableBoolean_ReturnsNullWhenInvalid(string? raw, bool? expected)
    {
        QueryParser.ParseNullableBoolean(raw).Should().Be(expected);
    }

    [Theory]
    [InlineData(null, 0)]
    [InlineData("", 0)]
    [InlineData("offset:8", 8)]
    [InlineData("OFFSET:11", 11)]
    [InlineData("offset:-1", 0)]
    [InlineData("bad:12", 0)]
    [InlineData("offset:oops", 0)]
    public void ParseContinuationOffset_ParsesOnlyValidOffsetTokens(string? raw, int expected)
    {
        QueryParser.ParseContinuationOffset(raw).Should().Be(expected);
    }
}

