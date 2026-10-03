namespace Logic.Devices;

/// <summary>A paired device as the parents' area lists it.</summary>
public sealed record DeviceInfo(long Id, string Name, DateTime PairedAt, DateTime? LastSeenAt, DateTime ExpiresAt);
