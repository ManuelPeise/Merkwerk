using Logic.Notifications.Formatting;
using Logic.Notifications.Links;
using Microsoft.Extensions.Options;

namespace Logic.Notifications.Tests;

public sealed class MailDateFormatterTests
{
    private static readonly DateTimeOffset SummerNoonUtc = new(2026, 7, 1, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Format_German_UsesBerlinTimeAndGermanFormat()
    {
        var formatter = new MailDateFormatter(Options.Create(new PublicUrlOptions()));

        Assert.Equal("01.07.2026 um 12:00 Uhr", formatter.Format(SummerNoonUtc, "de"));
    }

    [Fact]
    public void Format_English_UsesEnglishFormat()
    {
        var formatter = new MailDateFormatter(Options.Create(new PublicUrlOptions()));

        Assert.Equal("July 1, 2026 at 12:00 PM", formatter.Format(SummerNoonUtc, "en"));
    }

    [Fact]
    public void Format_UnknownTimeZone_FallsBackToUtc()
    {
        var formatter = new MailDateFormatter(Options.Create(new PublicUrlOptions { TimeZoneId = "Nowhere/Nothing" }));

        Assert.Equal("01.07.2026 um 10:00 Uhr", formatter.Format(SummerNoonUtc, "de"));
    }
}
