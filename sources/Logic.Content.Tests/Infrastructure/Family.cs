namespace Logic.Content.Tests.Infrastructure;

/// <summary>A family created for a test: its owner (admin) and a plain member.</summary>
public sealed record Family(long OrganizationId, long AdminUserId, long MemberUserId);
