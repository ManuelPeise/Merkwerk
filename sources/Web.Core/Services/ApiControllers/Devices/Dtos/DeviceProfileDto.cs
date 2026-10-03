using Logic.Devices;

namespace Web.Core.Services.ApiControllers.Devices.Dtos;

/// <summary>A child on the profile selection: first name and avatar only.</summary>
public sealed record DeviceProfileDto(long LearnerId, string FirstName, string AvatarId)
{
    public static DeviceProfileDto From(DeviceProfile profile) => new(profile.LearnerId, profile.FirstName, profile.AvatarId);
}
