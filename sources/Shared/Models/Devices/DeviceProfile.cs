namespace Shared.Models.Devices;

/// <summary>A child on the profile selection of a paired device – first name and avatar only.</summary>
public sealed record DeviceProfile(long LearnerId, string FirstName, string AvatarId);
