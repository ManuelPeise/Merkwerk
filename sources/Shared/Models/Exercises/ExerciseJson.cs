using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shared.Models.Exercises;

/// <summary>
/// The one JSON setup for exercise content (ADR 005) – database columns and API alike. MySQL stores JSON in its own
/// binary form and returns the keys sorted, so the type discriminator is not necessarily the first property:
/// <see cref="JsonSerializerOptions.AllowOutOfOrderMetadataProperties"/> must stay on.
/// </summary>
public static class ExerciseJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        AllowOutOfOrderMetadataProperties = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };
}
