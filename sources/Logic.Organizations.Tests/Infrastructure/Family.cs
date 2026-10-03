namespace Logic.Organizations.Tests.Infrastructure;

/// <summary>A family created for a test, with its owner.</summary>
public sealed record Family(long OrganizationId, long OwnerUserId, string OwnerEmail);
