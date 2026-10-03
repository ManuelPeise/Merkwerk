using System.Text.Json.Serialization;

namespace Web.Core.Services.ApiControllers.Devices.Dtos;

/// <summary>Is this device paired? <c>familyName</c> only when it is.</summary>
public sealed record DeviceStatusDto(
    bool IsPaired,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? FamilyName = null);
