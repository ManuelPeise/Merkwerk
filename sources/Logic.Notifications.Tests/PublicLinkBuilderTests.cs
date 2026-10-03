using Logic.Notifications.Links;
using Microsoft.Extensions.Options;

namespace Logic.Notifications.Tests;

public sealed class PublicLinkBuilderTests
{
    [Theory]
    [InlineData("https://merkwerk.example")]
    [InlineData("https://merkwerk.example/")]
    public void Build_PathWithoutQuery_JoinsWithSingleSlash(string baseUrl)
    {
        var builder = Create(baseUrl);

        Assert.Equal("https://merkwerk.example/confirm-email", builder.Build("/confirm-email"));
        Assert.Equal("https://merkwerk.example/confirm-email", builder.Build("confirm-email"));
    }

    [Fact]
    public void Build_QueryValues_AreUrlEncoded()
    {
        var builder = Create("http://localhost:65350");

        var link = builder.Build("/reset-password", ("token", "a+b/c="), ("lang", "de"));

        Assert.Equal("http://localhost:65350/reset-password?token=a%2Bb%2Fc%3D&lang=de", link);
    }

    private static PublicLinkBuilder Create(string baseUrl) =>
        new(Options.Create(new PublicUrlOptions { PublicBaseUrl = baseUrl }));
}
