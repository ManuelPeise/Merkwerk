using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Data.Database.Converters;

/// <summary>
/// DateTimeOffset (e.g. Identity's LockoutEnd) is stored as a UTC DATETIME and read back with offset zero,
/// independent of how the MySQL provider would map it.
/// </summary>
internal sealed class UtcDateTimeOffsetConverter() : ValueConverter<DateTimeOffset, DateTime>(
    value => value.UtcDateTime,
    value => new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc)));

/// <summary>Nullable variant of <see cref="UtcDateTimeOffsetConverter"/>.</summary>
internal sealed class NullableUtcDateTimeOffsetConverter() : ValueConverter<DateTimeOffset?, DateTime?>(
    value => value.HasValue ? value.Value.UtcDateTime : null,
    value => value.HasValue ? new DateTimeOffset(DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)) : null);
