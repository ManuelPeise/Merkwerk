using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Models.Exercises;

namespace Data.Database.Converters;

/// <summary>
/// Maps a polymorphic record to a MySQL <c>JSON</c> column (ADR 005) with <see cref="ExerciseJson.Options"/>. Changes are
/// detected by comparing the serialized JSON, so replacing or editing a record is always saved.
/// </summary>
internal static class JsonColumnExtensions
{
    public static PropertyBuilder<T> HasJsonColumn<T>(this PropertyBuilder<T> property)
        where T : class
    {
        var comparer = new ValueComparer<T>(
            (left, right) => Serialize(left) == Serialize(right),
            value => Serialize(value).GetHashCode(StringComparison.Ordinal),
            value => Deserialize<T>(Serialize(value)));

        property
            .HasConversion(value => Serialize(value), json => Deserialize<T>(json), comparer)
            .HasColumnType("json")
            .IsRequired();

        return property;
    }

    private static string Serialize<T>(T? value) => JsonSerializer.Serialize(value, ExerciseJson.Options);

    private static T Deserialize<T>(string json) =>
        JsonSerializer.Deserialize<T>(json, ExerciseJson.Options)
        ?? throw new JsonException($"Empty JSON for {typeof(T).Name}.");
}
