namespace Shared.Models.Devices;

/// <summary>A paired device's view of itself.</summary>
public sealed record DeviceStatus(long DeviceId, string FamilyName);
