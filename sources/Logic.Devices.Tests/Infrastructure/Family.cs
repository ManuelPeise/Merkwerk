namespace Logic.Devices.Tests.Infrastructure;

/// <summary>A family created for a test, with its owner (admin).</summary>
public sealed record Family(long OrganizationId, long OwnerUserId, string Name);
