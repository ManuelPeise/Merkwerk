using Shared.Models.Devices;

namespace Web.Core.Services.ApiControllers.Devices.Dtos;

/// <summary>A paired device in the parents' area. Times in UTC.</summary>
public sealed record DeviceDto(long Id, string Name, DateTimeOffset PairedAt, DateTimeOffset? LastSeenAt, DateTimeOffset ExpiresAt)
{
    public static DeviceDto From(DeviceInfo info) => new(
        info.Id,
        info.Name,
        AsUtc(info.PairedAt),
        info.LastSeenAt is { } lastSeen ? AsUtc(lastSeen) : null,
        AsUtc(info.ExpiresAt));

    private static DateTimeOffset AsUtc(DateTime value) => new(DateTime.SpecifyKind(value, DateTimeKind.Utc), TimeSpan.Zero);
}
