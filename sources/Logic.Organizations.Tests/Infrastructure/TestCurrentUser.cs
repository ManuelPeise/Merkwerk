using Data.Database.Abstractions;

namespace Logic.Organizations.Tests.Infrastructure;

/// <summary>The organization the "request" works in – set by the test, read by the tenant filter.</summary>
public sealed class TestCurrentUser : ICurrentUser
{
    public string Actor => SystemCurrentUser.SystemActor;

    public long? OrganizationId { get; set; }
}
