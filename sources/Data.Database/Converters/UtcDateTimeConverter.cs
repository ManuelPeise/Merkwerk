using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Data.Database.Converters;

/// <summary>
/// MySQL DATETIME has no time zone: values are stored as UTC and read back with <see cref="DateTimeKind.Utc"/> (ADR 011).
/// </summary>
internal sealed class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
    value => ToUtc(value),
    value => DateTime.SpecifyKind(value, DateTimeKind.Utc))
{
    internal static DateTime ToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        // Unspecified: we only ever create UTC values (TimeProvider), so treat it as UTC.
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
    };
}

/// <summary>Nullable variant of <see cref="UtcDateTimeConverter"/>.</summary>
internal sealed class NullableUtcDateTimeConverter() : ValueConverter<DateTime?, DateTime?>(
    value => value.HasValue ? UtcDateTimeConverter.ToUtc(value.Value) : value,
    value => value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : value);
