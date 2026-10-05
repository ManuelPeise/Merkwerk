using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Data.Database.Converters;

/// <summary>
/// A calendar day (e.g. a due date, LP-114) in a MySQL DATE column. MySql.EntityFrameworkCore maps <see cref="DateOnly"/>
/// to DATE but reads it back as <see cref="DateTime"/> (InvalidCastException), so it travels as a DateTime at midnight.
/// </summary>
internal sealed class DateOnlyConverter() : ValueConverter<DateOnly, DateTime>(
    value => value.ToDateTime(TimeOnly.MinValue),
    value => DateOnly.FromDateTime(value));

/// <summary>Nullable variant of <see cref="DateOnlyConverter"/>.</summary>
internal sealed class NullableDateOnlyConverter() : ValueConverter<DateOnly?, DateTime?>(
    value => value.HasValue ? value.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
    value => value.HasValue ? DateOnly.FromDateTime(value.Value) : (DateOnly?)null);
