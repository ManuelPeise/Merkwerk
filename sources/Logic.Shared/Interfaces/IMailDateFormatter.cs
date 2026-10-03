namespace Logic.Shared.Interfaces;

/// <summary>Formats a UTC time for a mail in the instance's time zone and the mail's language.</summary>
public interface IMailDateFormatter
{
    string Format(DateTimeOffset value, string language);
}
