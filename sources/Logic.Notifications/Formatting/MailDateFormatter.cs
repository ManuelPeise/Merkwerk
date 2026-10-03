using System.Globalization;
using Logic.Notifications.Links;
using Microsoft.Extensions.Options;

namespace Logic.Notifications.Formatting;

internal sealed class MailDateFormatter(IOptions<PublicUrlOptions> options) : IMailDateFormatter
{
    public string Format(DateTimeOffset value, string language)
    {
        var timeZone = FindTimeZone(options.Value.TimeZoneId);
        var local = TimeZoneInfo.ConvertTime(value, timeZone);

        return language == "en"
            ? local.ToString("MMMM d, yyyy 'at' h:mm tt", CultureInfo.GetCultureInfo("en-US"))
            : local.ToString("dd.MM.yyyy 'um' HH:mm 'Uhr'", CultureInfo.GetCultureInfo("de-DE"));
    }

    private static TimeZoneInfo FindTimeZone(string id)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(id);
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.Utc;
        }
    }
}
