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

    /// <summary>Like <see cref="HasJsonColumn{T}"/>, but the column may be NULL (e.g. generator settings, LP-131).</summary>
    public static PropertyBuilder<T?> HasOptionalJsonColumn<T>(this PropertyBuilder<T?> property)
        where T : class
    {
        var comparer = new ValueComparer<T?>(
            (left, right) => Serialize(left) == Serialize(right),
            value => value == null ? 0 : Serialize(value).GetHashCode(StringComparison.Ordinal),
            value => value == null ? null : Deserialize<T>(Serialize(value)));

        property
            .HasConversion(value => Serialize(value), json => Deserialize<T>(json), comparer)
            .HasColumnType("json")
            .IsRequired(false);

        return property;
    }

    private static string Serialize<T>(T? value) => JsonSerializer.Serialize(value, ExerciseJson.Options);

    private static T Deserialize<T>(string json) =>
        JsonSerializer.Deserialize<T>(json, ExerciseJson.Options)
        ?? throw new JsonException($"Empty JSON for {typeof(T).Name}.");
}
